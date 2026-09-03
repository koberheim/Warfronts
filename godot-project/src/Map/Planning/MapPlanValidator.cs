using System;
using System.Collections.Generic;

namespace FrontsOfWar.Map.Planning;

public static class MapPlanValidator
{
    public static MapPlanValidation Validate(MapPlanDefinition plan, MapPlannerConfig config = null)
    {
        config ??= MapPlannerConfig.Default;
        var result = new MapPlanValidation();
        if (plan == null) { result.Errors.Add("Plan is null."); return result; }
        if (plan.Canvas == null || plan.Canvas.X <= 0f || plan.Canvas.Y <= 0f)
            result.Errors.Add("Canvas dimensions must be positive.");
        if (plan.Entries.Count < config.MinGroundEntries || plan.Entries.Count > config.MaxGroundEntries)
            result.Errors.Add($"Ground entries must be between {config.MinGroundEntries} and {config.MaxGroundEntries}.");
        if (plan.Objective == null || !Inside(plan.Objective, plan.Canvas)) result.Errors.Add("Objective is outside the canvas.");
        if (plan.Paths.Count == 0) result.Errors.Add("At least one logical path is required.");

        for (int i = 0; i < plan.Entries.Count; i++)
            if (!Inside(plan.Entries[i], plan.Canvas)) result.Errors.Add($"Entry {i} is outside the canvas.");

        foreach (var path in plan.Paths)
        {
            if (path.Points.Count < 2) { result.Errors.Add($"{path.Id} needs at least two points."); continue; }
            if (!Inside(path.Points[0], plan.Canvas) || !Inside(path.Points[^1], plan.Canvas))
                result.Errors.Add($"{path.Id} leaves the canvas.");
            if (!HasEntry(plan, path.StartEntryId)) result.Errors.Add($"{path.Id} references missing {path.StartEntryId}.");
            if (path.ObjectiveId != "objective_0") result.Errors.Add($"{path.Id} references an unknown objective.");
            if (MapPlanGeometry.PathLength(path) <= 0.1f) result.Errors.Add($"{path.Id} has zero length.");
            if (MapPlanGeometry.SelfIntersects(path, out _) && !path.IntentionalCrossing)
                result.Errors.Add($"{path.Id} contains an accidental self-crossing.");
        }

        for (int i = 0; i < plan.Paths.Count; i++)
        for (int j = i + 1; j < plan.Paths.Count; j++)
        {
            if (!MapPlanGeometry.PathsIntersect(plan.Paths[i], plan.Paths[j], out _) ||
                plan.Paths[i].IntentionalCrossing || plan.Paths[j].IntentionalCrossing) continue;
            result.Errors.Add($"{plan.Paths[i].Id} and {plan.Paths[j].Id} cross without an intentional crossing flag.");
        }

        if (plan.Pads.Count < config.MinPads || plan.Pads.Count > config.MaxPads)
            result.Warnings.Add($"Pad count is outside the recommended {config.MinPads}-{config.MaxPads} range.");
        for (int i = 0; i < plan.Pads.Count; i++)
        {
            if (!Inside(plan.Pads[i].Position, plan.Canvas)) result.Errors.Add($"Pad {i} is outside the canvas.");
            for (int j = 0; j < i; j++)
                if (MapPlanGeometry.Distance(plan.Pads[i].Position, plan.Pads[j].Position) < 1f)
                    result.Errors.Add($"Pads {j} and {i} overlap.");
        }
        if (plan.Paths.Count > 1 && MapPlanGeometry.MinimumSeparation(plan.Paths) < config.MinimumPathSeparation)
            result.Warnings.Add("Some logical routes share or approach the minimum separation; confirm that this is intentional.");

        result.IsValid = result.Errors.Count == 0;
        plan.Validation = result;
        return result;
    }

    private static bool HasEntry(MapPlanDefinition plan, string id)
    {
        if (!id.StartsWith("entry_", StringComparison.Ordinal) || !int.TryParse(id[6..], out int index)) return false;
        return index >= 0 && index < plan.Entries.Count;
    }

    private static bool Inside(PlanPoint point, PlanPoint canvas)
        => point != null && point.X >= 0f && point.X <= canvas.X && point.Y >= 0f && point.Y <= canvas.Y;
}
