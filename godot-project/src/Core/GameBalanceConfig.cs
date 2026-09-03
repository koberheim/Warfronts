using Godot;

namespace FrontsOfWar.Core;

// The one tuning surface (GDD §15.1 principle 2). No magic numbers belong in
// behavior scripts — every cross-system balance constant lives here as an
// exported field, editable in-editor once overridden by a
// res://assets/data/config/game_balance_config.tres asset. Field defaults
// below are the GDD's authoritative baseline values (§7.2, §7.3, §7.4, §5.5).
[GlobalClass]
public partial class GameBalanceConfig : Resource
{
    [ExportGroup("Economy")]
    [Export] public int StartingSupplyRecruit = 650;
    [Export] public int StartingSupplyRegular = 500;
    [Export] public int StartingSupplyVeteran = 420;
    [Export] public int StartingSupplyElite = 380;
    [Export] public float BountyEffectiveHpDivisor = 10f;
    [Export] public float BountyFraction = 0.35f;
    [Export] public int WaveIncomeBase = 60;
    [Export] public int WaveIncomePerWave = 8;
    [Export] public float EarlyCallBonusFraction = 0.35f;
    [Export] public int EarlyCallBonusCap = 90;
    [Export] public float SellRefundFraction = 0.75f;
    [Export] public float SellFullRefundWindowSeconds = 4f;
    [Export] public float DifficultyIncomeScalarRecruit = 1.20f;
    [Export] public float DifficultyIncomeScalarRegular = 1.00f;
    [Export] public float DifficultyIncomeScalarVeteran = 0.90f;
    [Export] public float DifficultyIncomeScalarElite = 0.82f;

    [ExportGroup("Command Points")]
    [Export] public int CommandPointCap = 12;
    [Export] public int CommandPointBasePerWave = 3;
    [Export] public int CommandPointPerCommandPost = 2;
    [Export] public int CommandPointPerLogisticsPost = 4;

    [ExportGroup("Upgrades")]
    [Export] public float UpgradeCostMultiplierL2 = 0.65f;
    [Export] public float UpgradeCostMultiplierL3 = 1.15f;
    [Export] public float UpgradeCostMultiplierL4 = 2.10f;

    [ExportGroup("Status Effects")]
    [Export] public float SuppressedMoveSpeedMultiplier = 0.60f;
    [Export] public float SuppressedAbilityFrequencyMultiplier = 0.50f;
    [Export] public float SuppressedDurationSeconds = 2.5f;
    [Export] public float SpottedDamageMultiplier = 1.25f;

    [ExportGroup("National Balance")]
    [Export] public float NationalStatLeanEnvelope = 0.15f;
    [Export] public float NationalParityTolerance = 0.03f;

    [ExportGroup("Simulation")]
    [Export] public int SimulationHz = 60;
    [Export] public int PerformanceBudgetEnemies = 200;
    [Export] public int PerformanceBudgetTowers = 40;
    [Export] public int PerformanceBudgetProjectiles = 300;

    [ExportGroup("World Scale")]
    // GDD §6: "Range values are in tiles ... sized to a 64px reference
    // sprite." All gameplay range/speed numbers are authored in tiles; this
    // is the one place that number gets turned into Godot pixel units.
    [Export] public float TilePixelSize = 64f;
    [Export] public int SpatialGridCellSizeTiles = 4;

    [ExportGroup("Economy: Defense Line")]
    [Export] public int DefenseLineIntegrityRecruit = 30;
    [Export] public int DefenseLineIntegrityRegular = 20;
    [Export] public int DefenseLineIntegrityVeteran = 12;
    [Export] public int DefenseLineIntegrityElite = 8;
}
