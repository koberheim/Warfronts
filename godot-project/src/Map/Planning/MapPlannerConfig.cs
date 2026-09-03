namespace FrontsOfWar.Map.Planning;

// One tuning surface for editor-time planner behavior. These values describe
// normalized design space, not runtime gameplay balance.
public sealed class MapPlannerConfig
{
    public float CanvasWidth { get; init; } = 100f;
    public float CanvasHeight { get; init; } = 56.25f;
    public int MinGroundEntries { get; init; } = 1;
    public int MaxGroundEntries { get; init; } = 3;
    public int MinPads { get; init; } = 18;
    public int MaxPads { get; init; } = 34;
    public float MinimumPathSeparation { get; init; } = 3f;
    public float PadSpacing { get; init; } = 4f;
    public float PadInfluenceRadius { get; init; } = 6f;
    public static MapPlannerConfig Default { get; } = new();
}
