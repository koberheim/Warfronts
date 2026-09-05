using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using System.Collections.Generic;

namespace FrontsOfWar.Waves;

// Plays back one WaveDefinition's spawn groups on their authored schedule
// (GDD §19 prompt 13). Ticked explicitly by MapRuntime, first in the system
// order (§15.4: Time → Spawns → ...).
public class WaveRunner
{
    private class GroupState
    {
        public SpawnGroup Group;
        public int SpawnedCount;
        public float Elapsed;
    }

    private readonly EnemyManager _enemyManager;
    private readonly PathNetwork _path;
    private readonly PathNetworkSet _paths;
    private readonly Node _enemyParent;
    private readonly List<GroupState> _activeGroups = new();
    private readonly Queue<WaveDefinition> _upcoming = new();

    private int _currentWaveNumber;
    private bool _isRunning;

    public bool IsRunning => _isRunning;
    public int CurrentWaveNumber => _currentWaveNumber;

    public WaveRunner(EnemyManager enemyManager, PathNetwork path, Node enemyParent)
    {
        _enemyManager = enemyManager; _path = path; _paths = new PathNetworkSet(); _paths.Add(path); _enemyParent = enemyParent;
    }

    public WaveRunner(EnemyManager enemyManager, PathNetworkSet paths, PathNetwork fallbackPath, Node enemyParent)
    {
        _enemyManager = enemyManager; _paths = paths ?? new PathNetworkSet(); _path = fallbackPath; _paths.Add(fallbackPath); _enemyParent = enemyParent;
    }

    // Queues a mission's wave list for the preview strip (GDD §10.7). The
    // MapRuntime owns the build timer and calls StartNextWave between waves.
    public void QueueWaves(IEnumerable<WaveDefinition> waves)
    {
        foreach (var wave in waves) _upcoming.Enqueue(wave);
    }

    public void StartNextWave()
    {
        if (_upcoming.Count > 0) StartWave(_upcoming.Dequeue());
    }

    // Index 0 = the wave after whatever's currently running (N+1), etc. —
    // matches the wave preview's three tiers of disclosure (§10.7).
    public IReadOnlyList<WaveDefinition> PeekUpcoming(int count)
    {
        var result = new List<WaveDefinition>(count);
        int i = 0;
        foreach (var wave in _upcoming)
        {
            if (i >= count) break;
            result.Add(wave);
            i++;
        }
        return result;
    }

    public void StartWave(WaveDefinition wave)
    {
        _currentWaveNumber = wave.WaveNumber;
        _activeGroups.Clear();
        foreach (var group in wave.Groups)
            _activeGroups.Add(new GroupState { Group = group });

        _isRunning = true;
        EventBus.Instance?.Publish(new WaveStartedEvent(_currentWaveNumber));
    }

    public void Tick(float tickDeltaSeconds)
    {
        if (!_isRunning) return;

        bool anyPending = false;
        foreach (var state in _activeGroups)
        {
            if (state.SpawnedCount >= state.Group.Count) continue;
            anyPending = true;

            state.Elapsed += tickDeltaSeconds;
            float nextSpawnTime = state.Group.StartDelaySeconds + state.SpawnedCount * state.Group.IntervalSeconds;
            if (state.Elapsed >= nextSpawnTime)
                SpawnOne(state);
        }

        if (!anyPending)
        {
            _isRunning = false;
            EventBus.Instance?.Publish(new WaveSpawningCompleteEvent(_currentWaveNumber));
        }
    }

    private void SpawnOne(GroupState state)
    {
        float hpMultiplier = state.Group.HpMultiplierOverride > 0f ? state.Group.HpMultiplierOverride : 1f;
        _enemyManager.Spawn(state.Group.Enemy, _paths.ResolveForWave(state.Group.PathId, _currentWaveNumber, _path), _enemyParent, hpMultiplier);

        state.SpawnedCount++;
    }
}
