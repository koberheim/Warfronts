using Godot;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.Combat;

// Resolves a tower's target given a priority mode and a candidate set from
// the SpatialGrid (GDD §19 prompt 9). Ground-only towers must pre-filter out
// air targets before calling this — AA-only towers do the inverse — since
// "does this tower see air" is a targeting-acquisition rule (§5.4), not
// something this service infers from the profile.
public static class TargetingService
{
    public static ITargetable SelectTarget(IEnumerable<ITargetable> candidates, TargetingProfile profile, Vector2 towerOrigin)
    {
        ITargetable best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var candidate in candidates)
        {
            float score = profile switch
            {
                TargetingProfile.First => candidate.PathProgress,
                TargetingProfile.Last => -candidate.PathProgress,
                TargetingProfile.Strongest => candidate.CurrentHp,
                TargetingProfile.Weakest => -candidate.CurrentHp,
                TargetingProfile.Closest => -towerOrigin.DistanceSquaredTo(candidate.GlobalPosition),
                TargetingProfile.DensestCluster => 0f, // resolved separately, see SelectDensestClusterPoint
                _ => 0f,
            };

            if (best == null || score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }

        return best;
    }

    // For indirect-fire towers (T3, T7): picks the candidate whose local
    // neighborhood (within clusterRadius) contains the most other candidates,
    // and returns that point to fire at rather than a single target.
    public static Vector2? SelectDensestClusterPoint(IReadOnlyList<ITargetable> candidates, float clusterRadiusPixels)
    {
        if (candidates.Count == 0) return null;

        ITargetable bestCenter = null;
        int bestCount = -1;
        float radiusSquared = clusterRadiusPixels * clusterRadiusPixels;

        foreach (var center in candidates)
        {
            int count = candidates.Count(c => center.GlobalPosition.DistanceSquaredTo(c.GlobalPosition) <= radiusSquared);
            if (count > bestCount)
            {
                bestCount = count;
                bestCenter = center;
            }
        }

        return bestCenter?.GlobalPosition;
    }
}
