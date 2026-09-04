using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using System.Linq;

namespace FrontsOfWar.Doctrines;

// The six shared ability behaviors' executors (GDD §19 prompt 39), split
// from DoctrineSystem.cs to keep each file under the ~300-line guideline.
// Every method here is generic over its DoctrineAbility data — none branch
// on which doctrine is active.
public partial class DoctrineSystem
{
    private sealed class PendingLineSweep
    {
        public float CenterDistance;
        public float LengthPixels;
        public float WidthPixels;
        public float TotalDamage;
        public DamageType DamageType;
        public float Elapsed;
        public float Duration;
    }

    private void ExecutePointBlast(DoctrineAbility ability, Vector2 point, SpatialGrid grid)
    {
        float radiusPixels = ability.RadiusTiles * _config.TilePixelSize;
        var targets = grid.QueryRadius(point, radiusPixels).ToList();
        SignatureTargeting.ApplyBlast(targets, point, radiusPixels, ability.Damage, ability.DamageType, null);
    }

    private void ExecuteLineBlast(DoctrineAbility ability, Vector2 primary, Vector2? secondary, SpatialGrid grid)
    {
        if (_path == null) return;
        float tile = _config.TilePixelSize;
        float centerDistance = ability.LineMode == DoctrineLineMode.DrawnLine && secondary.HasValue
            ? (_path.GetClosestDistance(primary) + _path.GetClosestDistance(secondary.Value)) * 0.5f
            : _path.GetClosestDistance(primary);
        float lengthPixels = ability.LengthTiles * tile;
        float widthPixels = ability.WidthTiles * tile;

        if (ability.LineMode == DoctrineLineMode.PathSegment && ability.SweepSeconds > 0f)
        {
            _pendingSweeps.Add(new PendingLineSweep
            {
                CenterDistance = centerDistance,
                LengthPixels = lengthPixels,
                WidthPixels = widthPixels,
                TotalDamage = ability.Damage,
                DamageType = ability.DamageType,
                Duration = ability.SweepSeconds,
            });
            return;
        }

        ApplyCorridorBlast(centerDistance, lengthPixels, widthPixels, ability.Damage, ability.DamageType, grid);
    }

    private void ExecuteAuraBuff(DoctrineAbility ability, TowerController towerTarget)
    {
        _abilityBuffRemaining = ability.DurationSeconds;
        _abilityBuffSingleTarget = ability.SingleTarget ? towerTarget : null;
    }

    private void ExecuteSpawnFriendly(DoctrineAbility ability, Vector2 point)
    {
        if (ability.FriendlyUnit == null || ability.FriendlyUnitScene == null || _path == null) return;
        float distance = _path.GetClosestDistance(point);
        int count = Mathf.Max(1, ability.FriendlyCount);
        for (int i = 0; i < count; i++)
            _friendlyUnits.Spawn(ability.FriendlyUnit, ability.FriendlyUnitScene, _path, distance);
    }

    private void ExecuteStatusApplication(DoctrineAbility ability, Vector2 point, SpatialGrid grid)
    {
        float radiusPixels = ability.RadiusTiles * _config.TilePixelSize;
        foreach (var target in grid.QueryRadius(point, radiusPixels))
        {
            if (target is not EnemyController enemy || !enemy.IsAlive) continue;
            if (ability.StatusId == "Suppressed") enemy.ApplySuppressed(ability.DurationSeconds, ability.DurationSeconds);
            else if (ability.StatusId == "Spotted") enemy.ApplySpotted(ability.DurationSeconds);
        }
    }

    private void ExecuteUtility(DoctrineAbility ability, Vector2 point, TowerController towerTarget,
        BuildPad padTarget, SpatialGrid grid)
    {
        switch (ability.UtilityId)
        {
            case DoctrineUtilityId.RefundTower: ExecuteRefundTower(towerTarget); break;
            case DoctrineUtilityId.FireAll: ExecuteFireAll(ability, grid); break;
            case DoctrineUtilityId.DetonateMinefields: ExecuteDetonateMinefields(ability); break;
            case DoctrineUtilityId.RefillMinefields: ExecuteRefillMinefields(); break;
            case DoctrineUtilityId.DefenseLineBonus: _defenseLine.RaiseMaxIntegrity(ability.DefenseLineBonus); break;
            case DoctrineUtilityId.RelocateTower: _placement.TryRelocate(towerTarget, padTarget); break;
            case DoctrineUtilityId.ForceTarget: ExecuteForceTarget(ability, point, grid); break;
        }
    }

    private void ExecuteRefundTower(TowerController tower)
    {
        if (tower == null) return;
        int refund = tower.Upgrade.TotalInvested;
        _supply.Credit(refund);
        _towers.Unregister(tower);
        _placement.ReleasePad(tower);
        tower.QueueFree();
    }

    private void ExecuteFireAll(DoctrineAbility ability, SpatialGrid grid)
    {
        int salvo = Mathf.Max(1, ability.SalvoCount);
        foreach (var tower in _towers.Towers.ToList())
        {
            if (ability.ArchetypeFilter >= 0 && (int)tower.Definition.Archetype != ability.ArchetypeFilter) continue;
            tower.ForceFire(salvo, grid, _projectiles);
        }
    }

    private void ExecuteDetonateMinefields(DoctrineAbility ability)
    {
        float multiplier = ability.DamageMultiplier > 0f ? ability.DamageMultiplier : 1f;
        foreach (var field in _minefields.Fields) field.TriggerNow(multiplier);
    }

    private void ExecuteRefillMinefields()
    {
        foreach (var field in _minefields.Fields) field.RefillCharges();
    }

    private void ExecuteForceTarget(DoctrineAbility ability, Vector2 point, SpatialGrid grid)
    {
        const float enemyClickToleranceTiles = 2f;
        var enemy = FindNearestEnemy(point, grid, enemyClickToleranceTiles * _config.TilePixelSize);
        if (enemy == null) return;

        float radiusPixels = ability.RadiusTiles * _config.TilePixelSize;
        float radiusSquared = radiusPixels * radiusPixels;
        foreach (var tower in _towers.Towers)
        {
            if (tower.GlobalPosition.DistanceSquaredTo(point) > radiusSquared) continue;
            tower.ForceTarget(enemy, ability.DurationSeconds);
        }
    }

    private static EnemyController FindNearestEnemy(Vector2 point, SpatialGrid grid, float radiusPixels)
    {
        EnemyController best = null;
        float bestDistanceSquared = float.MaxValue;
        foreach (var candidate in grid.QueryRadius(point, radiusPixels))
        {
            if (candidate is not EnemyController enemy || !enemy.IsAlive) continue;
            float distanceSquared = point.DistanceSquaredTo(enemy.GlobalPosition);
            if (distanceSquared < bestDistanceSquared) { bestDistanceSquared = distanceSquared; best = enemy; }
        }
        return best;
    }

    private void TickPendingSweeps(float delta, SpatialGrid grid)
    {
        if (_pendingSweeps.Count == 0) return;
        for (int i = _pendingSweeps.Count - 1; i >= 0; i--)
        {
            var sweep = _pendingSweeps[i];
            float remaining = Mathf.Max(0f, sweep.Duration - sweep.Elapsed);
            float slice = Mathf.Min(delta, remaining);
            sweep.Elapsed += delta;

            if (grid != null && sweep.Duration > 0f)
            {
                float sliceDamage = sweep.TotalDamage * (slice / sweep.Duration);
                ApplyCorridorBlast(sweep.CenterDistance, sweep.LengthPixels, sweep.WidthPixels, sliceDamage, sweep.DamageType, grid);
            }

            if (sweep.Elapsed >= sweep.Duration) _pendingSweeps.RemoveAt(i);
        }
    }

    private void ApplyCorridorBlast(float centerDistance, float lengthPixels, float widthPixels,
        float damage, DamageType damageType, SpatialGrid grid)
    {
        if (_path == null || damage <= 0f) return;
        Vector2 center = _path.GetPositionAtDistance(Mathf.Clamp(centerDistance, 0f, _path.LengthPixels));
        var nearby = grid.QueryRadius(center, lengthPixels * 0.5f + widthPixels).ToList();
        var hits = SignatureTargeting.CorridorTargets(nearby, _path, centerDistance, lengthPixels, widthPixels);
        foreach (var target in hits) SignatureTargeting.ApplyDamage(target, damage, damageType, null);
    }
}
