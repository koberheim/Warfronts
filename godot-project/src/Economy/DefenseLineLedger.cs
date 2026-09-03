using System;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;

namespace FrontsOfWar.Economy;

// The "lives" system (GDD §7.8, §19 prompt 8). Subscribes to
// EnemyLeakedEvent so EnemyManager doesn't need to know this ledger exists.
public class DefenseLineLedger : IDisposable
{
    public int Integrity { get; private set; }
    public int MaxIntegrity { get; }
    public bool IsDepleted => Integrity <= 0;

    public DefenseLineLedger(Difficulty difficulty, GameBalanceConfig config)
    {
        MaxIntegrity = StartingIntegrityFor(difficulty, config);
        Integrity = MaxIntegrity;
        EventBus.Instance?.Subscribe<EnemyLeakedEvent>(OnEnemyLeaked);
    }

    public void Dispose()
    {
        EventBus.Instance?.Unsubscribe<EnemyLeakedEvent>(OnEnemyLeaked);
    }

    public static int StartingIntegrityFor(Difficulty difficulty, GameBalanceConfig config) => difficulty switch
    {
        Difficulty.Recruit => config.DefenseLineIntegrityRecruit,
        Difficulty.Regular => config.DefenseLineIntegrityRegular,
        Difficulty.Veteran => config.DefenseLineIntegrityVeteran,
        Difficulty.Elite => config.DefenseLineIntegrityElite,
        _ => config.DefenseLineIntegrityRegular,
    };

    private void OnEnemyLeaked(EnemyLeakedEvent evt)
    {
        bool wasDepleted = IsDepleted;
        Integrity = System.Math.Max(0, Integrity - evt.LeakCost);
        EventBus.Instance?.Publish(new DefenseLineChangedEvent(Integrity, -evt.LeakCost));

        if (IsDepleted && !wasDepleted)
            EventBus.Instance?.Publish(new DefenseLineDepletedEvent());
    }

    // A boss reaching the objective is an instant loss regardless of
    // remaining integrity (§7.8) — bosses land at M3, this is the hook.
    public void ForceDeplete()
    {
        Integrity = 0;
        EventBus.Instance?.Publish(new DefenseLineDepletedEvent());
    }

    // Used by the Emergency Repair ability (§7.6: "restores 3 Defense Line
    // HP"). Never exceeds the starting max.
    public void Restore(int amount)
    {
        int clamped = System.Math.Min(MaxIntegrity, Integrity + amount);
        int delta = clamped - Integrity;
        Integrity = clamped;
        if (delta != 0) EventBus.Instance?.Publish(new DefenseLineChangedEvent(Integrity, delta));
    }
}
