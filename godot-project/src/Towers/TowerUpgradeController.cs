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
        return (int)System.MathF.Round(baseCost * multiplier);
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
