namespace FrontsOfWar.Doctrines;

// The six shared ability behaviors every doctrine's fourth ability must be
// one of (GDD §8.3, §19 prompt 39). No doctrine may introduce a seventh.
public enum DoctrineAbilityKind
{
    PointBlast,
    LineBlast,
    AuraBuff,
    SpawnFriendly,
    InstantRefund,
    StatusApplication,
}

// How a LineBlast ability's corridor is anchored (GDD §8.2 "Doctrines:"
// lines: "a drawn line" / "a chosen path segment" / "from the nearest map
// edge"). DoctrineSystem resolves all three onto the same path-corridor
// grammar SignatureTargeting already uses for RAF Scramble.
public enum DoctrineLineMode
{
    DrawnLine,
    PathSegment,
    FromMapEdge,
}

// What the ability hotbar's doctrine slot should ask the player to click
// before calling MapRuntime.ActivateDoctrineAbility (GDD §19 prompt 39).
public enum DoctrineTargetingMode
{
    None,
    Point,
    Tower,
    TowerThenPad,
    Enemy,
    TwoPoints,
}
