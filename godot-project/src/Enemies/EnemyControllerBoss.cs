using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System;
using System.Linq;

namespace FrontsOfWar.Enemies;

// B2 (Convoy) and B3 (Formation) boss mechanics (GDD §10.3), plus Elite
// Medium Armor's Frontal Plate. Each reuses the same nearby-ally aura
// pattern EnemyControllerSupport.cs already established for E9/E10/E11
// (query _enemyProvider, filter by distance) rather than inventing a
// parallel discovery mechanism.
public partial class EnemyController
{
    // B2's command vehicle projects this to nearby escorts (any archetype -
    // the GDD's "4x Medium Armor" is a spawn-composition choice, not a type
    // filter this mechanic itself needs to enforce).
    private float ConvoyDamageResistanceMultiplier()
    {
        float multiplier = 1f;
        float tile = GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var commander in _enemyProvider?.Invoke() ?? Array.Empty<EnemyController>())
        {
            if (commander == null || commander == this || !commander.IsAlive || commander.Definition.ConvoyAuraRadiusTiles <= 0f) continue;
            float radius = commander.Definition.ConvoyAuraRadiusTiles * tile;
            if (GlobalPosition.DistanceSquaredTo(commander.GlobalPosition) <= radius * radius)
                multiplier = Mathf.Min(multiplier, 1f - commander.Definition.ConvoyDamageResistancePercent);
        }
        return multiplier;
    }

    private bool IsConvoyProtectedFromSuppression()
    {
        float tile = GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var commander in _enemyProvider?.Invoke() ?? Array.Empty<EnemyController>())
        {
            if (commander == null || commander == this || !commander.IsAlive ||
                !commander.Definition.ConvoyGrantsSuppressionImmunity || commander.Definition.ConvoyAuraRadiusTiles <= 0f) continue;
            float radius = commander.Definition.ConvoyAuraRadiusTiles * tile;
            if (GlobalPosition.DistanceSquaredTo(commander.GlobalPosition) <= radius * radius) return true;
        }
        return false;
    }

    // Called once, when this enemy's HP reaches 0 (see ApplyDamage) - "killing
    // the command vehicle... instantly collapses the escorts to 50% HP."
    // Only ever reduces HP, never heals a healthier escort up to the cap.
    private void TriggerConvoyCollapseOnDeath()
    {
        if (Definition.ConvoyAuraRadiusTiles <= 0f || Definition.ConvoyCollapseHpFraction >= 1f) return;
        float radius = Definition.ConvoyAuraRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var escort in _enemyProvider?.Invoke() ?? Array.Empty<EnemyController>())
        {
            if (escort == null || escort == this || !escort.IsAlive ||
                GlobalPosition.DistanceSquaredTo(escort.GlobalPosition) > radius * radius) continue;
            escort.CapHealth(escort.MaxHp * Definition.ConvoyCollapseHpFraction);
        }
    }

    // B3's formation bonus/penalty. Counts this enemy in its own formation
    // (an alive bomber always counts itself), so FormationSize == the count
    // while every member (including this one) is alive.
    private (float damageMultiplier, float speedMultiplier) FormationState()
    {
        if (string.IsNullOrEmpty(Definition.FormationGroupId) || Definition.FormationSize <= 0) return (1f, 1f);
        int alive = 1 + (_enemyProvider?.Invoke() ?? Array.Empty<EnemyController>())
            .Count(other => other != null && other != this && other.IsAlive && other.Definition?.FormationGroupId == Definition.FormationGroupId);
        int dead = Mathf.Max(0, Definition.FormationSize - alive);
        float damageMultiplier = dead == 0 ? 1f - Definition.FormationDamageReductionPercent : 1f;
        float speedMultiplier = Mathf.Pow(1f - Mathf.Clamp(Definition.FormationBreakSpeedPenaltyPercent, 0f, 1f), dead);
        return (damageMultiplier, speedMultiplier);
    }

    // Elite Medium Armor: reduced damage from towers within a frontal cone
    // of this enemy's current heading, active only once it drops to
    // FrontalPlateActivateHpFraction of max HP. A stationary enemy (no
    // reliable heading yet) gets no protection rather than an arbitrary one.
    private float FrontalPlateDamageMultiplier(Vector2 sourcePosition)
    {
        if (!Definition.HasFrontalPlate || Velocity == Vector2.Zero) return 1f;
        if (_maxHp > 0f && _currentHp / _maxHp > Definition.FrontalPlateActivateHpFraction) return 1f;
        float headingDegrees = Mathf.RadToDeg(Velocity.Angle());
        return GimmickRules.IsWithinArc(GlobalPosition, headingDegrees, Definition.FrontalPlateHalfAngleDegrees, sourcePosition)
            ? 1f - Definition.FrontalPlateDamageReductionPercent : 1f;
    }
}
