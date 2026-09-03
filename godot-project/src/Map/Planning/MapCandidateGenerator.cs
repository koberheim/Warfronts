using System;
using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Core;

namespace FrontsOfWar.Map.Planning;

public static class MapCandidateGenerator
{
    public static MapPlanDefinition GenerateSingle(MapLayoutTemplate template, ulong seed,
        MapPlannerConfig config = null)
    {
        config ??= MapPlannerConfig.Default;
        var random = new SeededRandom(seed);
        var objective = new PlanPoint(94f, 28f);
        string family = template.Family.ToUpperInvariant();
        int entryCount = Math.Clamp(template.Topology.GroundEntries, 1, config.MaxGroundEntries);
        var entries = BuildEntries(entryCount);
        var plan = new MapPlanDefinition
        {
            Id = $"{template.Id.ToLowerInvariant()}_{seed:X8}",
            DisplayName = $"{template.TemplateName} Candidate {seed}",
            SourceTemplateId = template.Id,
            Seed = seed,
            Canvas = new PlanPoint(config.CanvasWidth, config.CanvasHeight),
            Entries = entries,
            Objective = objective,
            Status = MapPlanStatus.Candidate,
            Zones = BuildZones(family),
        };

        plan.Paths = BuildPaths(family, entries, objective, random);
        int minimumPads = template.Planner.RecommendedPadCount.Length > 0 ? template.Planner.RecommendedPadCount[0] : config.MinPads;
        int maximumPads = template.Planner.RecommendedPadCount.Length > 1 ? template.Planner.RecommendedPadCount[1] : config.MaxPads;
        PadSuggestionService.Suggest(plan, minimumPads, maximumPads, config);
        MapPlanMetricsCalculator.Calculate(plan);
        MapPlanScorer.Score(plan, template);
        MapPlanValidator.Validate(plan, config);
        return plan;
    }

    public static List<MapPlanDefinition> Generate(MapLayoutTemplate template, ulong seed, int count,
        MapPlannerConfig config = null)
    {
        var candidates = new List<MapPlanDefinition>();
        for (int i = 0; i < count; i++) candidates.Add(GenerateSingle(template, seed + (ulong)i, config));
        candidates.Sort((a, b) => b.Metrics.Score.CompareTo(a.Metrics.Score));
        return CandidateDiversity.Filter(candidates, count);
    }

    private static List<PlanPoint> BuildEntries(int count)
    {
        var entries = new List<PlanPoint> { new(5f, 8f), new(5f, 28f), new(5f, 48f) };
        return entries.Take(count).ToList();
    }

    private static List<PathPlan> BuildPaths(string family, List<PlanPoint> entries, PlanPoint objective, SeededRandom random)
    {
        var paths = family switch
        {
            "SERPENTINE" => OnePath("path_0", entries[0], new[] { new PlanPoint(22, 8), new PlanPoint(22, 18), new PlanPoint(78, 18), new PlanPoint(78, 38), new PlanPoint(22, 38), new PlanPoint(78, 48), objective }, false),
            "HAIRPIN" => OnePath("path_0", entries[0], new[] { new PlanPoint(90, 8), new PlanPoint(90, 18), new PlanPoint(18, 18), new PlanPoint(18, 32), new PlanPoint(86, 32), new PlanPoint(86, 44), objective }, false),
            "LOOP" => OnePath("path_0", entries[0], new[] { new PlanPoint(18, 8), new PlanPoint(78, 8), new PlanPoint(78, 48), new PlanPoint(26, 48), new PlanPoint(26, 20), new PlanPoint(68, 20), new PlanPoint(68, 38), objective }, true),
            "MERGE" => MergePaths(entries, objective, false),
            "SPLIT_MERGE" => SplitMergePaths(entries, objective),
            "DUAL_LANE" => DualLanePaths(entries, objective),
            "CROSSING" => CrossingPaths(entries, objective),
            "HUB" => HubPaths(entries, objective),
            "GAUNTLET" => OnePath("path_0", entries[0], new[] { new PlanPoint(20, 8), new PlanPoint(20, 22), new PlanPoint(80, 22), new PlanPoint(80, 34), new PlanPoint(20, 34), new PlanPoint(20, 48), new PlanPoint(80, 48), objective }, false),
            "ASYMMETRIC" => MergePaths(entries, objective, true),
            _ => OnePath("path_0", entries[0], new[] { new PlanPoint(48, 8), new PlanPoint(48, 44), objective }, false),
        };
        JitterInteriorPoints(paths, random);
        return paths;
    }

    private static List<PathPlan> OnePath(string id, PlanPoint entry, PlanPoint[] middle, bool intentional)
    {
        var points = new List<PlanPoint> { entry };
        points.AddRange(middle);
        return new List<PathPlan> { new() { Id = id, StartEntryId = "entry_0", IntentionalCrossing = intentional, Points = points } };
    }

    private static List<PathPlan> MergePaths(List<PlanPoint> entries, PlanPoint objective, bool asymmetric)
    {
        var first = new List<PlanPoint> { entries[0], new(34, 8), new(34, 22), new(68, 22), objective };
        var second = asymmetric
            ? new List<PlanPoint> { entries[1], new(18, 48), new(52, 42), new(78, 32), objective }
            : new List<PlanPoint> { entries[1], new(34, 48), new(34, 34), new(68, 34), objective };
        return new List<PathPlan>
        {
            new() { Id = "path_0", StartEntryId = "entry_0", BranchGroupId = "merge_0", Points = first },
            new() { Id = "path_1", StartEntryId = "entry_1", BranchGroupId = "merge_0", Points = second },
        };
    }

    private static List<PathPlan> SplitMergePaths(List<PlanPoint> entries, PlanPoint objective)
    {
        var top = new List<PlanPoint> { entries[0], new(24, 16), new(54, 12), new(70, 28), objective };
        var bottom = new List<PlanPoint> { entries[0], new(24, 40), new(54, 44), new(70, 28), objective };
        return new List<PathPlan>
        {
            new() { Id = "path_0", StartEntryId = "entry_0", BranchGroupId = "split_merge_0", Points = top },
            new() { Id = "path_1", StartEntryId = "entry_0", BranchGroupId = "split_merge_0", Points = bottom },
        };
    }

    private static List<PathPlan> DualLanePaths(List<PlanPoint> entries, PlanPoint objective)
    {
        return new List<PathPlan>
        {
            new() { Id = "path_0", StartEntryId = "entry_0", BranchGroupId = "dual_0", Points = new() { entries[0], new(30, 8), new(72, 16), new(86, 22), objective } },
            new() { Id = "path_1", StartEntryId = "entry_1", BranchGroupId = "dual_0", Points = new() { entries[1], new(30, 48), new(72, 40), new(86, 34), objective } },
        };
    }

    private static List<PathPlan> CrossingPaths(List<PlanPoint> entries, PlanPoint objective)
    {
        return new List<PathPlan>
        {
            new() { Id = "path_0", StartEntryId = "entry_0", IntentionalCrossing = true, BranchGroupId = "crossing_0", Points = new() { entries[0], new(42, 48), new(72, 8), objective } },
            new() { Id = "path_1", StartEntryId = "entry_1", IntentionalCrossing = true, BranchGroupId = "crossing_0", Points = new() { entries[1], new(42, 8), new(72, 48), objective } },
        };
    }

    private static List<PathPlan> HubPaths(List<PlanPoint> entries, PlanPoint objective)
    {
        var paths = new List<PathPlan>();
        for (int i = 0; i < entries.Count; i++)
            paths.Add(new PathPlan { Id = $"path_{i}", StartEntryId = $"entry_{i}", BranchGroupId = "hub_0", Points = new() { entries[i], new(34, entries[i].Y), new(64, 28), objective } });
        return paths;
    }

    private static void JitterInteriorPoints(List<PathPlan> paths, SeededRandom random)
    {
        foreach (var path in paths)
        for (int i = 1; i < path.Points.Count - 1; i++)
        {
            bool sharedPoint = paths.Exists(other => !ReferenceEquals(other, path) &&
                other.Points.Exists(point => MapPlanGeometry.Distance(point, path.Points[i]) < 0.01f));
            if (sharedPoint) continue;
            path.Points[i] = new PlanPoint(path.Points[i].X + random.NextFloat(-0.65f, 0.65f),
                path.Points[i].Y + random.NextFloat(-0.65f, 0.65f));
        }
    }

    private static List<PlanZone> BuildZones(string family)
    {
        var zones = new List<PlanZone>
        {
            new() { Type = "GameplayLane", Center = new(50f, 28f), Size = new(90f, 48f) },
            new() { Type = "GameplayClearZone", Center = new(50f, 28f), Size = new(75f, 40f) },
            new() { Type = "GeneralTerrain", Center = new(50f, 28f), Size = new(98f, 54f) },
        };
        if (family is "HUB" or "CROSSING" or "LOOP")
            zones.Add(new PlanZone { Type = "Storytelling", Center = new(66f, 28f), Size = new(18f, 18f) });
        return zones;
    }
}
