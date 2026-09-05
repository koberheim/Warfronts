using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FrontsOfWar.Map;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Map.Planning;

// Converts normalized planner output into ordinary editable map data. The
// source plan is never modified and all generated records retain provenance.
public static class MapPlanConverter
{
    public static MapDefinition ToMapDefinition(MapPlanDefinition plan, string mapId = null, string acceptedBy = "planner", string convertedAtUtc = "")
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        int width = Math.Max(1, Mathf.CeilToInt(plan.Canvas?.X ?? 1f));
        int height = Math.Max(1, Mathf.CeilToInt(plan.Canvas?.Y ?? 1f));
        var map = MapDefinition.CreateNew(mapId ?? plan.Id, plan.DisplayName, width, height);
        map.Metadata.Theater = "fictionalized_western_front";
        map.Metadata.Biome = "planner_draft";
        map.Metadata.TerrainSetId = "default";
        var markers = new List<GameplayMarker>();
        for (int i = 0; i < plan.Entries.Count; i++)
            markers.Add(new GameplayMarker { Id = $"entry_{i}", Kind = GameplayMarkerKind.GroundEntry, PositionTiles = ToTile(plan.Entries[i], plan.Canvas) });
        markers.Add(new GameplayMarker { Id = "objective_0", Kind = GameplayMarkerKind.Objective, PositionTiles = ToTile(plan.Objective, plan.Canvas) });
        map.Markers = markers.ToArray();
        map.Paths = (plan.Paths ?? new List<PathPlan>()).Select(path => new PathDefinition
        {
            Id = path.Id, EntryMarkerId = path.StartEntryId, ObjectiveMarkerId = "objective_0",
            Points = path.Points.Select(point => ToTile(point, plan.Canvas)).ToArray(), BakedRuntimePoints = path.Points.Select(point => ToTile(point, plan.Canvas)).ToArray(),
            BranchGroupId = path.BranchGroupId, ActiveFromWave = path.ActiveFromWave,
            Tags = path.Tags?.ToArray() ?? Array.Empty<string>(),
        }).ToArray();
        map.TowerNodes = (plan.Pads ?? new List<PadPlan>()).Select(pad => new TowerPlacementNode
        {
            Id = pad.Id, PositionTiles = ToTile(pad.Position, plan.Canvas), Tag = ParsePadTag(pad.Tag), GeneratedSuggestion = true,
            RuntimeMetadata = new[] { new MapProperty { Key = "strategic_role", Value = pad.StrategicRole ?? "" }, new MapProperty { Key = "planner_score", Value = pad.Exposure?.Score.ToString("0.###") ?? "0" } },
        }).ToArray();
        map.Zones = (plan.Zones ?? new List<PlanZone>()).Select((zone, index) => new MapZone
        {
            Id = $"zone_{index}", Kind = ParseZone(zone.Type), CenterTiles = ToTile(zone.Center, plan.Canvas), SizeTiles = new Vector2(zone.Size.X, zone.Size.Y),
        }).ToArray();
        map.Provenance = new GenerationProvenance
        {
            SourceTemplateId = plan.SourceTemplateId, Seed = unchecked((long)plan.Seed), GeneratorVersion = "planner-v1",
            InitialScore = plan.Metrics?.Score ?? 0f, ConvertedAtUtc = convertedAtUtc ?? "", AcceptedBy = acceptedBy,
            InitialMetrics = (plan.Metrics?.ScoreComponents ?? new Dictionary<string, float>()).OrderBy(pair => pair.Key).Select(pair => new MapProperty { Key = pair.Key, Value = pair.Value.ToString("0.###") }).ToArray(),
        };
        return map;
    }

    private static Vector2 ToTile(PlanPoint point, PlanPoint canvas)
    {
        if (point == null || canvas == null) return Vector2.Zero;
        return new Vector2(point.X / Math.Max(0.01f, canvas.X) * canvas.X, point.Y / Math.Max(0.01f, canvas.Y) * canvas.Y);
    }
    private static PadTag ParsePadTag(string value) => Enum.TryParse(value, true, out PadTag tag) ? tag : PadTag.Standard;
    private static MapZoneKind ParseZone(string value) => value?.ToLowerInvariant() switch { "gameplayclearzone" => MapZoneKind.NoPlacement, "generalterrain" => MapZoneKind.RestrictedTerrain, _ => MapZoneKind.GameplayLane };
}
