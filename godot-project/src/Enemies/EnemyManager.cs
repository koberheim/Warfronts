using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using FrontsOfWar.Waves;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Enemies;

// Owns the lifetime of every live enemy for the current mission. Enemy pools
// are prepared from the complete WaveSequence before wave one. Once prepared,
// an exhausted fixed-capacity pool queues requests in deterministic FIFO order
// until a lease returns; it never drops a spawn or instantiates mid-wave.
public class EnemyManager
{
    private sealed class PendingSpawn
    {
        public EnemyDefinition Definition;
        public PathNetwork Path;
        public float HpScale;
        public int Serial;
    }

    private readonly List<EnemyController> _enemies = new();
    private readonly List<ITargetable> _targetableScratch = new();
    private readonly Dictionary<PackedScene, ObjectPool<EnemyController>> _pools = new();
    private readonly Dictionary<EnemyController, ObjectPool<EnemyController>> _leases = new();
    private readonly Queue<PendingSpawn> _pendingSpawns = new();
    private Node _poolParent;
    private bool _prepared;
    private int _nextId;
    private int _liveLimit = int.MaxValue;

    public IReadOnlyList<EnemyController> Enemies => _enemies;
    public int PendingSpawnCount => _pendingSpawns.Count;
    public bool IsPrepared => _prepared;
    public int PooledInstanceCount
    {
        get
        {
            int count = 0;
            foreach (var pool in _pools.Values) count += pool.Capacity;
            return count;
        }
    }

    public Func<IEnumerable<ISiegeTarget>> SiegeTargetsProvider { get; set; }
    public AirCorridorDefinition AirCorridor { get; set; }

    // Required parent wiring (before WaveRunner can start wave one):
    // Enemies.Prepare(sequence, enemyContainer, config).
    // Peak capacity is the largest same-scene count in one wave, plus tuning
    // headroom and a reserve for recurring boss adds. Sequence waves do not
    // overlap; overflow beyond the hard cap is deferred, never discarded.
    public void Prepare(WaveSequence sequence, Node parent, GameBalanceConfig config)
    {
        if (sequence == null) throw new ArgumentNullException(nameof(sequence));
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (_prepared || _enemies.Count > 0 || _pendingSpawns.Count > 0)
            throw new InvalidOperationException("Enemy pools must be prepared before any enemy spawns.");
        if (_pools.Count > 0 && !_prepared)
            throw new InvalidOperationException("EnemyManager already used its compatibility spawn path; prepare a fresh manager before wave one.");

        config ??= new GameBalanceConfig();
        _liveLimit = Mathf.Max(1, config.PerformanceBudgetEnemies);
        _poolParent = parent;
        var peakByScene = new Dictionary<PackedScene, int>();
        var definitions = new HashSet<EnemyDefinition>();

        foreach (var wave in sequence.Waves ?? Array.Empty<WaveDefinition>())
        {
            var countThisWave = new Dictionary<PackedScene, int>();
            foreach (var group in wave?.Groups ?? Array.Empty<SpawnGroup>())
            {
                if (group?.Enemy == null || group.Count <= 0) continue;
                CollectDefinition(group.Enemy, definitions);
                var scene = RequiredScene(group.Enemy);
                countThisWave.TryGetValue(scene, out int count);
                countThisWave[scene] = count + group.Count;
            }

            foreach (var pair in countThisWave)
            {
                peakByScene.TryGetValue(pair.Key, out int peak);
                peakByScene[pair.Key] = Mathf.Max(peak, pair.Value);
            }
        }

        int bossReserve = Mathf.Max(0, config.EnemyPoolBossAddReserve);
        foreach (var definition in definitions)
        {
            var scene = RequiredScene(definition);
            if (!peakByScene.ContainsKey(scene)) peakByScene[scene] = 1;
            if (definition.IsBoss && definition.AddDefinition != null)
            {
                var addScene = RequiredScene(definition.AddDefinition);
                peakByScene.TryGetValue(addScene, out int peak);
                peakByScene[addScene] = peak + bossReserve;
            }
        }

        int hardCapacity = Mathf.Max(1, config.EnemyPoolHardCapacityPerScene);
        int headroom = Mathf.Max(0, config.EnemyPoolPrewarmHeadroom);
        foreach (var pair in peakByScene)
        {
            int requested = pair.Value + headroom;
            var pool = new ObjectPool<EnemyController>(pair.Key, parent,
                prewarmCount: Mathf.Min(requested, hardCapacity), hardCapacity: hardCapacity);
            pool.FreezeCapacity();
            _pools.Add(pair.Key, pool);
        }

        _prepared = true;
    }

    private static void CollectDefinition(EnemyDefinition definition, HashSet<EnemyDefinition> definitions)
    {
        if (definition == null || !definitions.Add(definition)) return;
        if (definition.IsBoss && definition.AddDefinition != null)
            CollectDefinition(definition.AddDefinition, definitions);
    }

    private static PackedScene RequiredScene(EnemyDefinition definition)
        => definition?.ControllerScene
           ?? throw new InvalidOperationException($"Enemy definition '{definition?.Id ?? "<null>"}' has no ControllerScene.");

    public void Register(EnemyController enemy)
    {
        if (enemy == null || _enemies.Contains(enemy)) return;
        _enemies.Add(enemy);
        enemy.SetEnemyProvider(() => _enemies);
    }

    public void ResetSoftBlocks()
    {
        foreach (var enemy in _enemies) enemy.SetSoftBlocked(false);
    }

    public EnemyController Spawn(EnemyDefinition definition, PathNetwork path, Node parent, float hpScale = 1f)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        var request = new PendingSpawn
        {
            Definition = definition,
            Path = path,
            HpScale = hpScale,
            Serial = _nextId++,
        };

        var scene = RequiredScene(definition);
        if (!_pools.TryGetValue(scene, out var pool))
        {
            if (_prepared)
                throw new InvalidOperationException(
                    $"Enemy '{definition.Id}' was not included when the mission pools were prepared.");

            // Compatibility for isolated tests and editor probes that do not
            // own a WaveSequence. Production missions call Prepare first.
            _poolParent ??= parent;
            if (_poolParent != parent)
                throw new InvalidOperationException("One EnemyManager cannot pool into multiple parents.");
            pool = new ObjectPool<EnemyController>(scene, parent);
            _pools.Add(scene, pool);
        }

        if (_pendingSpawns.Count > 0 || _enemies.Count >= _liveLimit || !pool.TryRent(out var instance))
        {
            _pendingSpawns.Enqueue(request);
            return null;
        }

        Activate(instance, pool, request);
        return instance;
    }

    private void Activate(EnemyController instance, ObjectPool<EnemyController> pool, PendingSpawn request)
    {
        instance.Name = $"Enemy_{request.Definition.Id}_{request.Serial}";
        instance.Definition = request.Definition;
        instance.Initialize(request.Path, request.HpScale, AirCorridor);
        _leases[instance] = pool;
        Register(instance);
    }

    public void Tick(float tickDeltaSeconds)
    {
        float swarmLeadProgress = 0f;
        foreach (var enemy in _enemies)
        {
            if (enemy.Definition?.SpecialAbilityId == "swarm_cohesion")
                swarmLeadProgress = Mathf.Max(swarmLeadProgress, enemy.PathProgress);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            enemy.SetCohesionLeadProgress(swarmLeadProgress);
            enemy.SetSiegeHoldDistance(GetSiegeHoldDistance(enemy));
            enemy.SimTick(tickDeltaSeconds);

            int addCount = enemy.ConsumeBossAddRequest();
            if (addCount > 0 && enemy.IsAlive && !enemy.ReachedEnd)
                EventBus.Instance?.Publish(new BossAddsRequestedEvent(enemy, addCount));

            if (enemy.IsAlive && enemy.ReachedEnd)
            {
                if (enemy.Definition.IsBoss)
                    EventBus.Instance?.Publish(new BossReachedObjectiveEvent(enemy));
                else
                    EventBus.Instance?.Publish(new EnemyLeakedEvent(enemy, enemy.Definition.LeakCost));
                ReleaseAt(i, enemy);
            }
            else if (!enemy.IsAlive)
            {
                ReleaseAt(i, enemy);
            }
        }

        DrainPendingSpawns();
    }

    private void ReleaseAt(int index, EnemyController enemy)
    {
        _enemies.RemoveAt(index);
        if (_leases.Remove(enemy, out var pool)) pool.Return(enemy);
        else enemy.QueueFree();
    }

    private void DrainPendingSpawns()
    {
        while (_pendingSpawns.Count > 0 && _enemies.Count < _liveLimit)
        {
            var request = _pendingSpawns.Peek();
            var pool = _pools[RequiredScene(request.Definition)];
            if (!pool.TryRent(out var instance)) break;
            _pendingSpawns.Dequeue();
            Activate(instance, pool, request);
        }
    }

    private float GetSiegeHoldDistance(EnemyController enemy)
    {
        bool isSiegeLike = enemy.Definition?.SpecialAbilityId == "siege_bombard" || enemy.MultiPhaseBoss?.IsSiegePhase == true;
        if (!isSiegeLike) return float.MaxValue;
        float holdDistance = float.MaxValue;
        float rangePixels = enemy.Definition.SiegeBombardRangeTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var target in SiegeTargetsProvider?.Invoke() ?? Array.Empty<ISiegeTarget>())
        {
            if (target == null || target.IsSiegeImmune) continue;
            float targetDistance = enemy.PathNetwork.GetClosestDistance(target.SiegePosition) - rangePixels;
            if (targetDistance >= enemy.PathDistancePixels)
                holdDistance = Mathf.Min(holdDistance, targetDistance);
        }
        return holdDistance;
    }

    // Returned list is reused across calls; callers must finish before the
    // next Tick/rebuild.
    public List<ITargetable> GetTargetables()
    {
        _targetableScratch.Clear();
        foreach (var enemy in _enemies)
            _targetableScratch.Add(enemy);
        return _targetableScratch;
    }
}
