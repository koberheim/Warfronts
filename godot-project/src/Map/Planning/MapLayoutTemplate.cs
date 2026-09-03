using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace FrontsOfWar.Map.Planning;

public sealed class MapLayoutTemplate
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("rank_group")] public string RankGroup { get; set; } = "";
    [JsonPropertyName("template_name")] public string TemplateName { get; set; } = "";
    [JsonPropertyName("family")] public string Family { get; set; } = "";
    [JsonPropertyName("family_name")] public string FamilyName { get; set; } = "";
    [JsonPropertyName("genre_prevalence")] public string PrevalenceTier { get; set; } = "";
    [JsonPropertyName("topology")] public MapTemplateTopology Topology { get; set; } = new();
    [JsonPropertyName("planner")] public MapTemplatePlanner Planner { get; set; } = new();
    [JsonPropertyName("fronts_of_war")] public MapTemplateCompatibility Compatibility { get; set; } = new();
}

public sealed class MapTemplateTopology
{
    [JsonPropertyName("ground_entries")] public int GroundEntries { get; set; }
    [JsonPropertyName("objectives")] public int Objectives { get; set; }
    [JsonPropertyName("logical_route_count")] public int LogicalRouteCount { get; set; }
    [JsonPropertyName("split_count")] public int SplitCount { get; set; }
    [JsonPropertyName("merge_count")] public int MergeCount { get; set; }
    [JsonPropertyName("crossing_count")] public int CrossingCount { get; set; }
    [JsonPropertyName("loop_count")] public int LoopCount { get; set; }
    [JsonPropertyName("path_length_class")] public string PathLengthClass { get; set; } = "";
    [JsonPropertyName("target_path_length_tiles")] public float[] TargetPathLengthTiles { get; set; } = System.Array.Empty<float>();
    [JsonPropertyName("shared_coverage")] public string SharedCoverage { get; set; } = "";
}

public sealed class MapTemplatePlanner
{
    [JsonPropertyName("recommended_pad_count")] public int[] RecommendedPadCount { get; set; } = System.Array.Empty<int>();
    [JsonPropertyName("primary_design_lesson")] public string PrimaryDesignLesson { get; set; } = "";
    [JsonPropertyName("difficulty_band")] public string DifficultyBand { get; set; } = "";
    [JsonPropertyName("must_have_constraints")] public List<string> Constraints { get; set; } = new();
    [JsonPropertyName("avoid")] public List<string> AntiPatterns { get; set; } = new();
    [JsonPropertyName("air_corridor_hint")] public string AirCorridorHint { get; set; } = "";
}

public sealed class MapTemplateCompatibility
{
    [JsonPropertyName("compatible")] public bool Compatible { get; set; }
    [JsonPropertyName("launch_candidate")] public bool LaunchCandidate { get; set; }
    [JsonPropertyName("best_use")] public string BestUse { get; set; } = "";
    [JsonPropertyName("fixed_build_pads")] public bool FixedBuildPads { get; set; }
    [JsonPropertyName("deterministic_routes")] public bool DeterministicRoutes { get; set; }
    [JsonPropertyName("runtime_procedural")] public bool RuntimeProcedural { get; set; }
}

public sealed class MapLayoutCatalog
{
    [JsonPropertyName("schema_version")] public string SchemaVersion { get; set; } = "";
    [JsonPropertyName("catalog_title")] public string CatalogTitle { get; set; } = "";
    [JsonPropertyName("intent")] public string Intent { get; set; } = "";
    [JsonPropertyName("records")] public List<MapLayoutTemplate> Records { get; set; } = new();

    public static MapLayoutCatalog LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<MapLayoutCatalog>(json, options) ?? new MapLayoutCatalog();
    }

    public static MapLayoutCatalog LoadFromProject()
        => LoadFromJson(FileAccess.GetFileAsString(
            "res://assets/data/map_layout_templates/map_layout_catalog_100.json"));

    public IEnumerable<MapLayoutTemplate> Filter(string family = "", string difficulty = "", bool launchOnly = false)
    {
        foreach (var record in Records)
        {
            if (!string.IsNullOrWhiteSpace(family) && record.Family != family) continue;
            if (!string.IsNullOrWhiteSpace(difficulty) && record.Planner.DifficultyBand != difficulty) continue;
            if (launchOnly && !record.Compatibility.LaunchCandidate) continue;
            yield return record;
        }
    }
}
