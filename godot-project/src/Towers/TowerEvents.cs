using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Map;

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

// Published by TowerPlacementService after a successful build-bar placement
// (GDD §7.4, §7.5, §13.4). PlacedInstance is a TowerController for every
// archetype except T9 Command Post, which is a CommandPostController.
public readonly struct TowerPlacedEvent
{
    public readonly Node2D PlacedInstance;
    public readonly TowerDefinition Definition;
    public readonly BuildPad Pad;
    public readonly int Cost;

    public TowerPlacedEvent(Node2D placedInstance, TowerDefinition definition, BuildPad pad, int cost)
    {
        PlacedInstance = placedInstance;
        Definition = definition;
        Pad = pad;
        Cost = cost;
    }
}
