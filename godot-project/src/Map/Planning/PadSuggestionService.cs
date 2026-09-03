using System;
using System.Collections.Generic;

namespace FrontsOfWar.Map.Planning;

public static class PadSuggestionService
{
    public static IReadOnlyList<PadPlan> Suggest(MapPlanDefinition plan, int minimum = 18, int maximum = 34,
        MapPlannerConfig config = null)
    {
        config ??= MapPlannerConfig.Default;
        int target = Math.Clamp((minimum + maximum) / 2, config.MinPads, config.MaxPads);
        var candidates = new List<PlanPoint>();
        foreach (var path in plan.Paths)
        {
            for (int i = 1; i < path.Points.Count; i++)
            {
                var a = path.Points[i - 1];
                var b = path.Points[i];
                float length = MapPlanGeometry.Distance(a, b);
                int samples = Math.Max(2, (int)MathF.Ceiling(length / 7f));
                for (int sample = 0; sample <= samples; sample++)
                {
                    float t = sample / (float)samples;
                    var point = new PlanPoint(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
                    float dx = b.X - a.X;
                    float dy = b.Y - a.Y;
                    float inverse = 1f / Math.Max(0.01f, MathF.Sqrt(dx * dx + dy * dy));
                    var normal = new PlanPoint(-dy * inverse, dx * inverse);
                    candidates.Add(Clamp(point, plan.Canvas));
                    candidates.Add(Clamp(new PlanPoint(point.X + normal.X * config.PadInfluenceRadius * 0.65f,
                        point.Y + normal.Y * config.PadInfluenceRadius * 0.65f), plan.Canvas));
                    candidates.Add(Clamp(new PlanPoint(point.X - normal.X * config.PadInfluenceRadius * 0.65f,
                        point.Y - normal.Y * config.PadInfluenceRadius * 0.65f), plan.Canvas));
                }
            }
        }

        var result = new List<PadPlan>();
        foreach (var point in candidates)
        {
            if (!FarEnough(point, result, config.PadSpacing)) continue;
            result.Add(CreatePad(plan, point, result.Count));
            if (result.Count >= target) break;
        }
        plan.Pads = result;
        RecalculateExposure(plan, result, config);
        return result;
    }

    private static PadPlan CreatePad(MapPlanDefinition plan, PlanPoint point, int index)
    {
        float nearestBend = float.MaxValue;
        float nearestSegment = float.MaxValue;
        int routes = 0;
        float progress = 0f;
        foreach (var path in plan.Paths)
        {
            float distance = MapPlanGeometry.DistanceToPath(point, path);
            nearestSegment = MathF.Min(nearestSegment, distance);
            if (distance <= MapPlannerConfig.Default.PadInfluenceRadius) routes++;
            for (int i = 1; i < path.Points.Count - 1; i++)
                nearestBend = MathF.Min(nearestBend, MapPlanGeometry.Distance(point, path.Points[i]));
            if (path.Points.Count > 1 && distance <= MapPlannerConfig.Default.PadInfluenceRadius)
                progress = MathF.Max(progress, ProgressAtNearestPoint(point, path));
        }
        string role = nearestBend <= 4f ? "Corner" : progress > 0.72f ? "Backline" : "StraightLane";
        if (routes > 1) role = "Shared";
        return new PadPlan { Id = $"pad_{index}", Position = point, StrategicRole = role, Tag = "Standard" };
    }

    private static void RecalculateExposure(MapPlanDefinition plan, List<PadPlan> pads, MapPlannerConfig config)
    {
        foreach (var pad in pads)
        {
            var exposure = new PadExposureMetrics();
            foreach (var path in plan.Paths)
            {
                float distance = MapPlanGeometry.DistanceToPath(pad.Position, path);
                if (distance > config.PadInfluenceRadius) continue;
                exposure.DistinctLogicalRoutes++;
                exposure.RouteTimeInRange += MathF.Max(0f, config.PadInfluenceRadius - distance);
                exposure.RepeatExposure = MathF.Max(exposure.RepeatExposure, 1f - distance / config.PadInfluenceRadius);
            }
            exposure.CornerQuality = pad.StrategicRole == "Corner" ? 1f : 0.2f;
            exposure.StraightLaneQuality = pad.StrategicRole == "StraightLane" ? 1f : 0.4f;
            exposure.IndirectFireQuality = pad.StrategicRole == "Backline" ? 1f : 0.55f;
            exposure.SupportClusterQuality = pad.StrategicRole == "Shared" ? 1f : 0.35f;
            exposure.BacklineValue = pad.StrategicRole == "Backline" ? 1f : 0.25f;
            exposure.PadTagValue = 1f;
            exposure.Score = MathF.Round(exposure.RouteTimeInRange + exposure.DistinctLogicalRoutes * 2f +
                exposure.CornerQuality + exposure.StraightLaneQuality + exposure.IndirectFireQuality, 3);
            pad.Exposure = exposure;
        }
    }

    private static float ProgressAtNearestPoint(PlanPoint point, PathPlan path)
    {
        float total = MapPlanGeometry.PathLength(path);
        float travelled = 0f;
        float nearest = float.MaxValue;
        foreach (var segment in Segments(path))
        {
            float distance = MapPlanGeometry.PointToSegmentDistance(point, segment.a, segment.b);
            if (distance < nearest)
            {
                nearest = distance;
                travelled = segment.before + MapPlanGeometry.Distance(segment.a, point);
            }
        }
        return total <= 0.1f ? 0f : travelled / total;
    }

    private static IEnumerable<(PlanPoint a, PlanPoint b, float before)> Segments(PathPlan path)
    {
        float before = 0f;
        for (int i = 1; i < path.Points.Count; i++)
        {
            yield return (path.Points[i - 1], path.Points[i], before);
            before += MapPlanGeometry.Distance(path.Points[i - 1], path.Points[i]);
        }
    }

    private static bool FarEnough(PlanPoint point, List<PadPlan> pads, float spacing)
    {
        foreach (var pad in pads)
            if (MapPlanGeometry.Distance(point, pad.Position) < spacing) return false;
        return true;
    }

    private static PlanPoint Clamp(PlanPoint point, PlanPoint canvas)
        => new(Math.Clamp(point.X, 1f, canvas.X - 1f), Math.Clamp(point.Y, 1f, canvas.Y - 1f));
}
