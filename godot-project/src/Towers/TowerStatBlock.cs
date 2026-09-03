using Godot;

namespace FrontsOfWar.Towers;

// One upgrade level's stats (GDD §6, §7.4). TowerDefinition.Levels holds one
// of these per level (4 for archetypes, 3 for signatures — signatures land
// in M5).
[GlobalClass]
public partial class TowerStatBlock : Resource
{
    [Export] public int Cost;
    [Export] public float DamagePerShot;
    [Export] public float RateOfFirePerSec;
    [Export] public float RangeTiles;
    [Export] public float MinRangeTiles;
    [Export] public float BlastRadiusTiles;
    [Export] public float TurnRateSeconds = 0.4f;

    // Command Post only (T9, GDD §6). Unused (left at 0) by every combat
    // archetype — a support tower has different stats than a gun, and
    // giving every archetype its own stat-block type would be a lot of
    // near-duplicate classes for the two or three fields each actually
    // needs beyond the shared ones above.
    [Export] public float AuraRadiusTiles;
    [Export] public float AuraRangeBonusPercent;
    [Export] public float AuraRateOfFireBonusPercent;
    [Export] public int CommandPointsPerWave;
    [Export] public int SupplyPerWave;
}
