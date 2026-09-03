using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Towers;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.Economy;

// The three universal tactical abilities (GDD §7.6, §19 prompt 17). Owned
// by MapRuntime; cooldowns and Artillery Strike's fire-delay are ticked
// alongside everything else in MapRuntime.SimTick.
public class AbilitySystem
{
    private class PendingStrike
    {
        public float TimeRemaining;
        public Vector2 Point;
        public float Damage;
        public float RadiusPixels;
    }

    private static readonly Dictionary<AbilityType, (int CpCost, float CooldownSeconds)> Specs = new()
    {
        { AbilityType.ArtilleryStrike, (4, 20f) },
        { AbilityType.Rally, (3, 30f) },
        { AbilityType.EmergencyRepair, (5, 45f) },
    };

    private const float ArtilleryStrikeDamage = 120f;
    private const float ArtilleryStrikeRadiusTiles = 3f;
    private const float ArtilleryStrikeDelaySeconds = 1.5f;
    private const float RallyRadiusTiles = 7f;
    private const float RallyDurationSeconds = 8f;
    private const float RallyRateOfFireMultiplier = 1.5f;
    private const int EmergencyRepairDefenseLineRestore = 3;

    private readonly GameBalanceConfig _config;
    private readonly Dictionary<AbilityType, float> _cooldownRemaining = new();
    private readonly List<PendingStrike> _pendingStrikes = new();

    public AbilitySystem(GameBalanceConfig config)
    {
        _config = config;
    }

    public bool IsOnCooldown(AbilityType type) => _cooldownRemaining.TryGetValue(type, out var t) && t > 0f;
    public float CooldownRemaining(AbilityType type) => _cooldownRemaining.TryGetValue(type, out var t) ? Mathf.Max(0f, t) : 0f;
    public int CpCost(AbilityType type) => Specs[type].CpCost;

    // targetPoint is required for ArtilleryStrike and Rally (both are
    // point-centered, GDD §7.6); ignored for EmergencyRepair, which affects
    // every tower and the Defense Line directly.
    public bool TryActivate(AbilityType type, Vector2 targetPoint, CommandPointLedger commandPoints,
                             TowerManager towers, Economy.DefenseLineLedger defenseLine)
    {
        if (IsOnCooldown(type)) return false;

        var (cpCost, cooldown) = Specs[type];
        if (!commandPoints.TrySpend(cpCost)) return false;

        switch (type)
        {
            case AbilityType.ArtilleryStrike:
                _pendingStrikes.Add(new PendingStrike
                {
                    TimeRemaining = ArtilleryStrikeDelaySeconds,
                    Point = targetPoint,
                    Damage = ArtilleryStrikeDamage,
                    RadiusPixels = ArtilleryStrikeRadiusTiles * _config.TilePixelSize,
                });
                break;

            case AbilityType.Rally:
                float radiusPixels = RallyRadiusTiles * _config.TilePixelSize;
                float radiusSquared = radiusPixels * radiusPixels;
                foreach (var tower in towers.Towers.Where(t => t.GlobalPosition.DistanceSquaredTo(targetPoint) <= radiusSquared))
                    tower.ApplyRally(RallyDurationSeconds, RallyRateOfFireMultiplier);
                break;

            case AbilityType.EmergencyRepair:
                foreach (var tower in towers.Towers) tower.ClearSuppression();
                defenseLine.Restore(EmergencyRepairDefenseLineRestore);
                break;
        }

        _cooldownRemaining[type] = cooldown;
        return true;
    }

    public void Tick(float tickDeltaSeconds, SpatialGrid grid)
    {
        foreach (var type in _cooldownRemaining.Keys.ToList())
            if (_cooldownRemaining[type] > 0f) _cooldownRemaining[type] -= tickDeltaSeconds;

        for (int i = _pendingStrikes.Count - 1; i >= 0; i--)
        {
            var strike = _pendingStrikes[i];
            strike.TimeRemaining -= tickDeltaSeconds;
            if (strike.TimeRemaining > 0f) continue;

            foreach (var target in grid.QueryRadius(strike.Point, strike.RadiusPixels))
                target.ApplyDamage(strike.Damage, DamageType.Explosive);
            _pendingStrikes.RemoveAt(i);
        }
    }
}
