using FrontsOfWar.Combat;

namespace FrontsOfWar.Towers;

public readonly struct TowerFiredEvent
{
    public readonly TowerController Tower;
    public readonly ITargetable Target;

    public TowerFiredEvent(TowerController tower, ITargetable target)
    {
        Tower = tower;
        Target = target;
    }
}

// Published when the player clicks a placed tower (GDD §13.5 — "Opens on
// click, anchored to the tower").
public readonly struct TowerClickedEvent
{
    public readonly TowerController Tower;
    public TowerClickedEvent(TowerController tower) => Tower = tower;
}

public readonly struct TowerSoldEvent
{
    public readonly TowerController Tower;
    public readonly int Refund;
    public TowerSoldEvent(TowerController tower, int refund) { Tower = tower; Refund = refund; }
}
