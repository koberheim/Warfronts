using FrontsOfWar.Core;

namespace FrontsOfWar.Towers;

public enum TowerBranchChoice { None, A, B }

// Level/branch/cost state for one placed tower (GDD §19 prompt 14, §6, §7.4).
// A plain object owned by TowerController rather than a Node — no Godot
// lifecycle needed.
public class TowerUpgradeController
{
    private readonly TowerDefinition _definition;
    private readonly GameBalanceConfig _config;
    private float _secondsSincePlacement;

    public int Level { get; private set; } = 1;
    public TowerBranchChoice Branch { get; private set; } = TowerBranchChoice.None;
    public int TotalInvested { get; private set; }

    // Set each tick by DoctrineSystem's passive pass (GDD §19 prompt 39) —
    // e.g. Deep Battle's "Heavy Artillery towers cost −15%" also discounts
    // that tower's own upgrades via UpgradeCostMultiplier.
    public float DoctrineCostMultiplier = 1f;

    public const int MaxLevel = 4;
    public const int ForkLevel = 3;

    public TowerUpgradeController(TowerDefinition definition, GameBalanceConfig config)
    {
        _definition = definition;
        _config = config;
        TotalInvested = definition.PreForkStatsForLevel(1).Cost;
    }

    public void Tick(float tickDeltaSeconds) => _secondsSincePlacement += tickDeltaSeconds;

    public bool IsWithinFullRefundWindow => _secondsSincePlacement <= _config.SellFullRefundWindowSeconds;

    public TowerStatBlock CurrentStats()
    {
        if (Level < ForkLevel) return _definition.PreForkStatsForLevel(Level);

        var branch = Branch == TowerBranchChoice.B ? _definition.BranchB : _definition.BranchA;
        int branchIndex = Level - ForkLevel; // L3 -> 0, L4 -> 1
        return branch.Levels[branchIndex];
    }

    public bool CanUpgrade => Level < MaxLevel;

    // The stats the next purchase would give (GDD §13.5's "diff preview:
    // Damage 45 → 62"). At the fork the caller says which branch it is
    // previewing; null once the tower is at max level.
    public TowerStatBlock PreviewStats(TowerBranchChoice branchAtFork = TowerBranchChoice.A)
    {
        if (!CanUpgrade) return null;
        int next = Level + 1;
        if (next < ForkLevel) return _definition.PreForkStatsForLevel(next);

        var choice = Level >= ForkLevel ? Branch : branchAtFork;
        var branch = choice == TowerBranchChoice.B ? _definition.BranchB : _definition.BranchA;
        int branchIndex = next - ForkLevel;
        return branch != null && branchIndex < branch.Levels.Length ? branch.Levels[branchIndex] : null;
    }

    // At the fork level (L3), the caller must supply which branch to enter;
    // ignored at every other level.
    public int UpgradeCost(TowerBranchChoice branchAtFork = TowerBranchChoice.A)
    {
        int baseCost = _definition.PreForkStatsForLevel(1).Cost;
        float multiplier = Level switch
        {
            1 => _config.UpgradeCostMultiplierL2,
            2 => _config.UpgradeCostMultiplierL3,
            3 => _config.UpgradeCostMultiplierL4,
            _ => 0f,
        };
        // Multipliers are authored to two decimals (GDD §7.4), so work in
        // integer hundredths and round half up: float representation
        // (2.10f ≈ 2.0999999) and banker's rounding would otherwise turn
        // 225 × 2.10 = 472.5 into 472 instead of the table's 473.
        int hundredths = (int)System.MathF.Round(multiplier * DoctrineCostMultiplier * 100f);
        return (baseCost * hundredths + 50) / 100;
    }

    public int Upgrade(TowerBranchChoice branchAtFork = TowerBranchChoice.A)
    {
        if (!CanUpgrade) return 0;

        int cost = UpgradeCost(branchAtFork);
        if (Level == ForkLevel - 1) Branch = branchAtFork; // entering the fork
        Level++;
        TotalInvested += cost;
        return cost;
    }

    public int SellRefund()
        => IsWithinFullRefundWindow ? TotalInvested : (int)System.MathF.Round(TotalInvested * _config.SellRefundFraction);
}
