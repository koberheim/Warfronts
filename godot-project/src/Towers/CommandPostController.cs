using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.Towers;

// T9 Command Post (GDD §6, §19 prompt 15) — the one archetype that never
// fires. Reuses TowerDefinition/TowerUpgradeController for its cost/level
// bookkeeping (it has the same 4-level, branch-fork shape as every other
// archetype), but is ticked by CommandPostManager, not TowerManager, since
// its only job each tick is projecting an aura rather than targeting/firing.
public partial class CommandPostController : Node2D
{
    [Export] public TowerDefinition Definition;

    public TowerUpgradeController Upgrade { get; private set; }

    public override void _Ready()
    {
        Upgrade = new TowerUpgradeController(Definition, GameBalanceConfigAutoload.Config);
    }

    public void SimTick(float tickDeltaSeconds) => Upgrade.Tick(tickDeltaSeconds);

    public int CurrentCommandPointsPerWave => Upgrade.CurrentStats().CommandPointsPerWave;
    public int CurrentSupplyPerWave => Upgrade.CurrentStats().SupplyPerWave;

    // Sets (never adds — auras from multiple posts don't stack, GDD §6 T9)
    // this post's bonus on every tower within its radius, but only if it's
    // stronger than whatever that tower already has this tick.
    public void ApplyAuraTo(TowerManager towers, float tilePixelSize)
    {
        var stats = Upgrade.CurrentStats();
        float radiusPixels = stats.AuraRadiusTiles * tilePixelSize;
        float radiusSquared = radiusPixels * radiusPixels;
        float rangeMultiplier = 1f + stats.AuraRangeBonusPercent;
        float rofMultiplier = 1f + stats.AuraRateOfFireBonusPercent;

        foreach (var tower in towers.Towers)
        {
            if (GlobalPosition.DistanceSquaredTo(tower.GlobalPosition) > radiusSquared) continue;
            tower.AuraRangeMultiplier = Mathf.Max(tower.AuraRangeMultiplier, rangeMultiplier);
            tower.AuraRateOfFireMultiplier = Mathf.Max(tower.AuraRateOfFireMultiplier, rofMultiplier);
        }
    }
}
