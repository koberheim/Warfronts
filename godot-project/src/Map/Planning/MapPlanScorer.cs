using System;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.Map.Planning;

public sealed class MapScoreReport
{
    public float Total { get; set; }
    public Dictionary<string, float> Components { get; } = new();
    public List<string> Diagnostics { get; } = new();
    public bool IsValid { get; set; }
}

public static class MapPlanScorer
{
    public static MapScoreReport Score(MapPlanDefinition plan, MapLayoutTemplate template = null)
    {
        var validation = MapPlanValidator.Validate(plan);
        var metrics = MapPlanMetricsCalculator.Calculate(plan);
        var report = new MapScoreReport { IsValid = validation.IsValid };
        float route = validation.IsValid ? 20f : 0f;
        float coverage = Normalized(Math.Min(1f, plan.Pads.Count / 24f)) * 12f + Normalized(metrics.BendCount / 8f) * 8f;
        float counterplay = plan.Pads.Count == 0 ? 0f : Math.Clamp(plan.Pads.Count(p => p.Exposure.DistinctLogicalRoutes > 1) / 4f, 0f, 1f) * 15f;
        float difficulty = DifficultyScore(metrics.AveragePathLength, template) * 10f;
        float padEconomy = PadEconomyScore(plan.Pads.Count, template) * 10f;
        float composition = Math.Clamp(plan.Zones.Count / 4f, 0f, 1f) * 10f;
        float theater = plan.Zones.Any(z => z.Type == "Storytelling") ? 5f : 2.5f;
        float technical = validation.IsValid ? 10f : 0f;

        Add(report, "Route Readability", route);
        Add(report, "Strategic Coverage Variety", coverage);
        Add(report, "Counterplay Support", counterplay);
        Add(report, "Difficulty Shape", difficulty);
        Add(report, "Pad Economy", padEconomy);
        Add(report, "Spatial Composition", composition);
        Add(report, "Theater Plausibility", theater);
        Add(report, "Technical Validity", technical);
        report.Total = report.Components.Values.Sum();
        report.Diagnostics.AddRange(validation.Errors);
        report.Diagnostics.AddRange(validation.Warnings);
        metrics.ScoreComponents = new Dictionary<string, float>(report.Components);
        metrics.Score = report.Total;
        metrics.Diagnostics = new List<string>(report.Diagnostics);
        plan.Metrics = metrics;
        return report;
    }

    private static float DifficultyScore(float pathLength, MapLayoutTemplate template)
    {
        if (template?.Topology.TargetPathLengthTiles == null || template.Topology.TargetPathLengthTiles.Length < 2)
            return 0.5f;
        float low = template.Topology.TargetPathLengthTiles[0];
        float high = template.Topology.TargetPathLengthTiles[1];
        float normalized = (pathLength - low) / Math.Max(1f, high - low);
        return 1f - Math.Clamp(MathF.Abs(normalized - 0.5f) * 1.5f, 0f, 1f);
    }

    private static float PadEconomyScore(int count, MapLayoutTemplate template)
    {
        int low = template?.Planner.RecommendedPadCount?.Length > 0 ? template.Planner.RecommendedPadCount[0] : 18;
        int high = template?.Planner.RecommendedPadCount?.Length > 1 ? template.Planner.RecommendedPadCount[1] : 34;
        if (count >= low && count <= high) return 1f;
        int distance = count < low ? low - count : count - high;
        return Math.Clamp(1f - distance / 10f, 0f, 1f);
    }

    private static float Normalized(float value) => Math.Clamp(value, 0f, 1f);

    private static void Add(MapScoreReport report, string name, float value)
        => report.Components[name] = MathF.Round(value, 3);
}
