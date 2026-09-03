using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

public static class SignatureTargeting
{
    public static bool IsInsidePathCorridor(ITargetable target, PathNetwork path,
                                             float centerDistance, float lengthPixels, float widthPixels)
    {
        if (target == null || !target.IsAlive || path == null) return false;
        float targetDistance = path.GetClosestDistance(target.GlobalPosition);
        float pathDistance = Mathf.Abs(targetDistance - centerDistance);
        if (pathDistance > lengthPixels * 0.5f) return false;
        Vector2 nearest = path.GetPositionAtDistance(targetDistance);
        return nearest.DistanceTo(target.GlobalPosition) <= widthPixels * 0.5f;
    }

    public static List<ITargetable> CorridorTargets(IReadOnlyList<ITargetable> targets,
                                                      PathNetwork path, float centerDistance,
                                                      float lengthPixels, float widthPixels,
                                                      bool airOnly = false, bool groundOnly = false)
    {
        var result = new List<ITargetable>();
        if (targets == null) return result;
        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive) continue;
            if (airOnly && !target.IsAir) continue;
            if (groundOnly && target.IsAir) continue;
            if (IsInsidePathCorridor(target, path, centerDistance, lengthPixels, widthPixels)) result.Add(target);
        }
        return result;
    }

    public static void ApplyDamage(ITargetable target, float damage, DamageType type, IDamageSource source)
    {
        if (target is IDamageReceiver receiver) receiver.ApplyDamage(damage, type, source);
        else target?.ApplyDamage(damage, type);
    }

    public static int ApplyBlast(IReadOnlyList<ITargetable> targets, Vector2 center,
                                 float radiusPixels, float damage, DamageType type,
                                 IDamageSource source, bool includeAir = false)
    {
        int hitCount = 0;
        if (targets == null) return 0;
        float radiusSquared = radiusPixels * radiusPixels;
        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive || (!includeAir && target.IsAir)) continue;
            if (target.GlobalPosition.DistanceSquaredTo(center) > radiusSquared) continue;
            ApplyDamage(target, damage, type, source);
            hitCount++;
        }
        return hitCount;
    }
}
