using System;
using System.Collections.Generic;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Meta;

// Owned by MapRuntime (GDD §19 prompt 41). Listens to the same typed
// EventBus everything else uses rather than reaching into managers
// directly, except for the handful of facts (Defense Line integrity,
// difficulty, wave reached) that have no dedicated event and are cheap to
// hand in at construction instead of inventing one just for this.
public sealed class MissionStatsCollector : IDisposable
{
    private readonly Difficulty _difficulty;
    private readonly DefenseLineLedger _defenseLine;
    private readonly Func<int> _waveReachedProvider;

    private int _towersBuilt;
    private readonly HashSet<TowerArchetype> _archetypesUsed = new();
    private bool _bossKilled;
    private bool _victory;
    private bool _completed;

    public MissionStatsSnapshot LastSnapshot { get; private set; }

    public MissionStatsCollector(Difficulty difficulty, DefenseLineLedger defenseLine, Func<int> waveReachedProvider)
    {
        _difficulty = difficulty;
        _defenseLine = defenseLine;
        _waveReachedProvider = waveReachedProvider;
        EventBus.Instance?.Subscribe<TowerPlacedEvent>(OnTowerPlaced);
        EventBus.Instance?.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        EventBus.Instance?.Subscribe<MissionCompletedEvent>(OnMissionCompleted);
    }

    public void Dispose()
    {
        EventBus.Instance?.Unsubscribe<TowerPlacedEvent>(OnTowerPlaced);
        EventBus.Instance?.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        EventBus.Instance?.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);
    }

    private void OnTowerPlaced(TowerPlacedEvent evt)
    {
        // Counts every build-bar placement, T9 Command Post included — the
        // GDD's worked example ("no more than 8 towers") reads as an
        // economy/footprint cap, not an archetype filter.
        _towersBuilt++;
        if (evt.Definition != null) _archetypesUsed.Add(evt.Definition.Archetype);
    }

    private void OnEnemyKilled(EnemyKilledEvent evt)
    {
        if (evt.Enemy?.Definition?.IsBoss == true) _bossKilled = true;
    }

    private void OnMissionCompleted(MissionCompletedEvent evt)
    {
        if (_completed) return;
        _completed = true;
        _victory = evt.Victory;

        // Captured synchronously, before scene teardown can dispose the
        // DefenseLine ledger this reads from (GDD §19 prompt 41: "its
        // snapshot is stored in MissionSession.LastResult at mission end").
        LastSnapshot = BuildSnapshot();
        MissionSession.StoreCompletedResult(LastSnapshot);
    }

    public MissionStatsSnapshot BuildSnapshot() => new()
    {
        Victory = _victory,
        TowersBuilt = _towersBuilt,
        ArchetypesUsed = new HashSet<TowerArchetype>(_archetypesUsed),
        BossKilled = _bossKilled,
        FinalIntegrity = _defenseLine?.Integrity ?? 0,
        MaxIntegrity = _defenseLine?.MaxIntegrity ?? 0,
        Difficulty = _difficulty,
        WaveReached = _waveReachedProvider?.Invoke() ?? 0,
    };
}
