using Godot;
using FrontsOfWar.Core;
using System;

namespace FrontsOfWar.Enemies;

public partial class EnemyController
{
    private float AbsorbShieldedDamage(float damage)
    {
        if (damage <= 0f) return 0f;
        if (Definition.Archetype == EnemyArchetype.Escort) return damage - AbsorbShield(damage);
        foreach (var escort in _enemyProvider?.Invoke() ?? Array.Empty<EnemyController>())
        {
            if (escort == null || escort == this || escort.Definition.Archetype != EnemyArchetype.Escort ||
                !escort.IsAlive || GlobalPosition.DistanceTo(escort.GlobalPosition) >
                escort.Definition.EscortShieldRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize) continue;
            float remaining = damage - escort.AbsorbShield(damage);
            if (remaining < damage) return remaining;
        }
        return damage;
    }

    private float AbsorbShield(float damage)
    {
        float absorbed = Mathf.Min(_shieldRemaining, damage);
        _shieldRemaining -= absorbed;
        QueueRedraw();
        return absorbed;
    }

    private void TickFieldRepair(float delta)
    {
        EnemyController best = null;
        float bestDistance = float.MaxValue;
        float range = Definition.SupportRepairRangeTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var candidate in _enemyProvider?.Invoke() ?? Array.Empty<EnemyController>())
        {
            if (candidate == null || candidate == this || !candidate.IsAlive || candidate.IsAir ||
                candidate.CurrentHp >= candidate.MaxHp || !IsRepairableVehicle(candidate.Definition.Archetype)) continue;
            float distance = GlobalPosition.DistanceSquaredTo(candidate.GlobalPosition);
            if (distance <= range * range && distance < bestDistance) { best = candidate; bestDistance = distance; }
        }
        _repairTarget = best;
        if (best != null) best.RestoreHealth(best.MaxHp * Definition.SupportRepairPercentMaxHpPerSecond * delta);
    }

    private static bool IsRepairableVehicle(EnemyArchetype archetype)
        => archetype is EnemyArchetype.LightVehicle or EnemyArchetype.MediumArmor or EnemyArchetype.HeavyArmor or
            EnemyArchetype.Escort or EnemyArchetype.Siege;

    private float NearbyReconSpeedMultiplier()
    {
        float multiplier = 1f;
        float tile = GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var recon in _enemyProvider?.Invoke() ?? Array.Empty<EnemyController>())
        {
            if (recon == null || recon == this || recon.Definition.Archetype != EnemyArchetype.Recon || !recon.IsAlive) continue;
            float radius = recon.Definition.ReconAuraRadiusTiles * tile;
            if (GlobalPosition.DistanceSquaredTo(recon.GlobalPosition) <= radius * radius)
                multiplier = Mathf.Max(multiplier, recon.Definition.ReconSpeedBonusMultiplier);
        }
        return multiplier;
    }
}
