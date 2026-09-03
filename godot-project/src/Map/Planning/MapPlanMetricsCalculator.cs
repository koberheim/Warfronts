using System;

namespace FrontsOfWar.Map.Planning;

public static class MapPlanMetricsCalculator
{
    public static MapPlanMetrics Calculate(MapPlanDefinition plan)
    {
        var metrics = new MapPlanMetrics();
        foreach (var path in plan.Paths)
        {
            metrics.TotalPathLength += MapPlanGeometry.PathLength(path);
            metrics.BendCount += MapPlanGeometry.BendCount(path);
            if (MapPlanGeometry.SelfIntersects(path, out _))
                metrics.IntentionalCrossings += path.IntentionalCrossing ? 1 : 0;
        }
        metrics.AveragePathLength = plan.Paths.Count == 0 ? 0f : metrics.TotalPathLength / plan.Paths.Count;
        for (int i = 0; i < plan.Paths.Count; i++)
        for (int j = i + 1; j < plan.Paths.Count; j++)
        {
            if (MapPlanGeometry.PathsIntersect(plan.Paths[i], plan.Paths[j], out _))
                metrics.IntentionalCrossings += plan.Paths[i].IntentionalCrossing || plan.Paths[j].IntentionalCrossing ? 1 : 0;
            float routeDistance = MapPlanGeometry.Distance(plan.Paths[i].Points[^1], plan.Paths[j].Points[^1]);
            if (routeDistance < 3f) metrics.SharedCoverage += 0.25f;
        }
        metrics.SharedCoverage = Math.Clamp(metrics.SharedCoverage, 0f, 1f);
        metrics.RouteSeparation = MapPlanGeometry.MinimumSeparation(plan.Paths);
        plan.Metrics = metrics;
        return metrics;
    }
}
