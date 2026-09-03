using System;
using System.Collections.Generic;

namespace FrontsOfWar.Map.Planning;

public static class CandidateDiversity
{
    public static List<MapPlanDefinition> Filter(IReadOnlyList<MapPlanDefinition> candidates, int maximum,
        float minimumSignatureDistance = 0.25f)
    {
        var result = new List<MapPlanDefinition>();
        foreach (var candidate in candidates)
        {
            bool distinct = true;
            foreach (var selected in result)
                if (SignatureDistance(candidate, selected) < minimumSignatureDistance) { distinct = false; break; }
            if (distinct) result.Add(candidate);
            if (result.Count >= maximum) break;
        }
        return result;
    }

    public static float SignatureDistance(MapPlanDefinition first, MapPlanDefinition second)
    {
        int count = Math.Min(first.Paths.Count, second.Paths.Count);
        if (count == 0) return 0f;
        float distance = 0f;
        for (int i = 0; i < count; i++)
        {
            int points = Math.Min(first.Paths[i].Points.Count, second.Paths[i].Points.Count);
            for (int p = 0; p < points; p++) distance += MapPlanGeometry.Distance(first.Paths[i].Points[p], second.Paths[i].Points[p]);
        }
        return distance / count;
    }
}
