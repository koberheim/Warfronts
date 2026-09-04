using System;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;

namespace FrontsOfWar.Economy;

// The build currency (GDD §7.2, §19 prompt 12). Subscribes to
// EnemyKilledEvent for kill bounty income; end-of-wave and early-call income
// are driven explicitly by WaveRunner, since only it knows wave timing.
public class SupplyLedger : IDisposable
{
    private readonly GameBalanceConfig _config;
    private readonly Difficulty _difficulty;

    public int Balance { get; private set; }

    // Set once by DoctrineSystem.ApplyMissionStart (GDD §19 prompt 39 — e.g.
    // Lend-Lease's "Supply income +8%"). 1f (no-op) for any mission with no
    // doctrine loaded.
    public float DoctrineIncomeMultiplier = 1f;

    public SupplyLedger(Difficulty difficulty, GameBalanceConfig config)
    {
        _difficulty = difficulty;
        _config = config;
        Balance = StartingSupplyFor(difficulty, config);

        EventBus.Instance?.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
    }

    public void Dispose()
    {
        EventBus.Instance?.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
    }

    public static int StartingSupplyFor(Difficulty difficulty, GameBalanceConfig config) => difficulty switch
    {
        Difficulty.Recruit => config.StartingSupplyRecruit,
        Difficulty.Regular => config.StartingSupplyRegular,
        Difficulty.Veteran => config.StartingSupplyVeteran,
        Difficulty.Elite => config.StartingSupplyElite,
        _ => config.StartingSupplyRegular,
    };

    private float DifficultyIncomeScalar => _difficulty switch
    {
        Difficulty.Recruit => _config.DifficultyIncomeScalarRecruit,
        Difficulty.Regular => _config.DifficultyIncomeScalarRegular,
        Difficulty.Veteran => _config.DifficultyIncomeScalarVeteran,
        Difficulty.Elite => _config.DifficultyIncomeScalarElite,
        _ => 1f,
    };

    private void OnEnemyKilled(EnemyKilledEvent evt) => Credit(evt.Bounty);

    public int EndOfWaveIncome(int waveNumber)
        => RoundToInt((_config.WaveIncomeBase + _config.WaveIncomePerWave * waveNumber) *
            DifficultyIncomeScalar * DoctrineIncomeMultiplier);

    // fractionOfBuildTimeRemaining is 0..1; the bonus is capped in absolute
    // Supply per §7.2 so an early-rush player never snowballs past ~15%.
    public int EarlyCallBonus(int waveNumber, float fractionOfBuildTimeRemaining)
    {
        int bonus = RoundToInt(EndOfWaveIncome(waveNumber) * _config.EarlyCallBonusFraction * fractionOfBuildTimeRemaining);
        return System.Math.Min(bonus, _config.EarlyCallBonusCap);
    }

    public bool TrySpend(int amount)
    {
        if (amount > Balance) return false;
        Credit(-amount);
        return true;
    }

    public void Credit(int amount)
    {
        Balance += amount;
        EventBus.Instance?.Publish(new SupplyChangedEvent(Balance, amount));
    }

    public int SellRefund(int totalInvested) => RoundToInt(totalInvested * _config.SellRefundFraction);
    public int FullRefund(int totalInvested) => totalInvested;

    private static int RoundToInt(float value) => (int)System.MathF.Round(value);
}
