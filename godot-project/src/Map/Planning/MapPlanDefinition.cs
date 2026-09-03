using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FrontsOfWar.Map.Planning;

public enum MapPlanStatus { Draft, Candidate, Accepted, Rejected }

public sealed class PlanPoint
{
    public float X { get; set; }
    public float Y { get; set; }
    public PlanPoint() { }
    public PlanPoint(float x, float y) { X = x; Y = y; }
}

public sealed class PathPlan
{
    public string Id { get; set; } = "path_0";
    public string StartEntryId { get; set; } = "entry_0";
    public string ObjectiveId { get; set; } = "objective_0";
    public string BranchGroupId { get; set; } = "";
    public bool IntentionalCrossing { get; set; }
    public int ActiveFromWave { get; set; } = 1;
    public List<string> Tags { get; set; } = new();
    public List<PlanPoint> Points { get; set; } = new();
}

public sealed class PadPlan
{
    public string Id { get; set; } = "pad_0";
    public PlanPoint Position { get; set; } = new();
    public string Tag { get; set; } = "Standard";
    public string StrategicRole { get; set; } = "Corner";
    public PadExposureMetrics Exposure { get; set; } = new();
}

public sealed class PadExposureMetrics
{
    public float RouteTimeInRange { get; set; }
    public int DistinctLogicalRoutes { get; set; }
    public float RepeatExposure { get; set; }
    public float CornerQuality { get; set; }
    public float StraightLaneQuality { get; set; }
    public float IndirectFireQuality { get; set; }
    public float SupportClusterQuality { get; set; }
    public float BacklineValue { get; set; }
    public float AirCorridorOverlap { get; set; }
    public float PadTagValue { get; set; }
    public float Score { get; set; }
}

public sealed class PlanZone
{
    public string Type { get; set; } = "GameplayLane";
    public PlanPoint Center { get; set; } = new();
    public PlanPoint Size { get; set; } = new(10f, 10f);
}

public sealed class MapPlanMetrics
{
    public float TotalPathLength { get; set; }
    public float AveragePathLength { get; set; }
    public int BendCount { get; set; }
    public int IntentionalCrossings { get; set; }
    public int AccidentalCrossings { get; set; }
    public float SharedCoverage { get; set; }
    public float RouteSeparation { get; set; }
    public float Score { get; set; }
    public Dictionary<string, float> ScoreComponents { get; set; } = new();
    public List<string> Diagnostics { get; set; } = new();
}

public sealed class MapPlanValidation
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public sealed class MapPlanDefinition
{
    public string Id { get; set; } = "draft_plan";
    public string DisplayName { get; set; } = "Draft Map Plan";
    public string SourceTemplateId { get; set; } = "";
    public ulong Seed { get; set; }
    public PlanPoint Canvas { get; set; } = new(100f, 56.25f);
    public List<PlanPoint> Entries { get; set; } = new();
    public PlanPoint Objective { get; set; } = new();
    public List<PathPlan> Paths { get; set; } = new();
    public List<PlanPoint> AirCorridors { get; set; } = new();
    public List<PadPlan> Pads { get; set; } = new();
    public List<PlanZone> Zones { get; set; } = new();
    public List<string> GimmickHooks { get; set; } = new();
    public MapPlanMetrics Metrics { get; set; } = new();
    public MapPlanValidation Validation { get; set; } = new();
    public MapPlanStatus Status { get; set; } = MapPlanStatus.Draft;
}

public static class MapPlanSerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string SaveToJson(MapPlanDefinition plan)
        => JsonSerializer.Serialize(plan, Options);

    public static MapPlanDefinition LoadFromJson(string json)
        => JsonSerializer.Deserialize<MapPlanDefinition>(json, Options) ?? new MapPlanDefinition();

    public static void SaveFile(MapPlanDefinition plan, string path)
        => File.WriteAllText(path, SaveToJson(plan));

    public static MapPlanDefinition LoadFile(string path)
        => LoadFromJson(File.ReadAllText(path));
}
