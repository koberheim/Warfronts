using System;
using FrontsOfWar.Core;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Economy;

public readonly struct CommandPointsChangedEvent
{
    public readonly int NewBalance;
    public readonly int Delta;
    public CommandPointsChangedEvent(int newBalance, int delta) { NewBalance = newBalance; Delta = delta; }
}

// The ability currency (GDD §7.3, §19 prompt 17). Gains +3 per wave
// completed, plus whatever Command Posts contribute — the caller supplies
// that contribution via a delegate since only MapRuntime's CommandPostManager
// knows the live total across all placed Command Posts and their branches.
public class CommandPointLedger : IDisposable
{
    private readonly GameBalanceConfig _config;
    private readonly Func<int> _commandPostBonusProvider;

    public int Balance { get; private set; }

    public CommandPointLedger(GameBalanceConfig config, Func<int> commandPostBonusProvider)
    {
        _config = config;
        _commandPostBonusProvider = commandPostBonusProvider;
        EventBus.Instance?.Subscribe<WaveSpawningCompleteEvent>(OnWaveComplete);
    }

    public void Dispose()
    {
        EventBus.Instance?.Unsubscribe<WaveSpawningCompleteEvent>(OnWaveComplete);
    }

    private void OnWaveComplete(WaveSpawningCompleteEvent evt)
        => Credit(_config.CommandPointBasePerWave + _commandPostBonusProvider());

    public void Credit(int amount)
    {
        int clamped = System.Math.Min(_config.CommandPointCap, Balance + amount);
        int delta = clamped - Balance;
        Balance = clamped;
        if (delta != 0) EventBus.Instance?.Publish(new CommandPointsChangedEvent(Balance, delta));
    }

    public bool TrySpend(int amount)
    {
        if (amount > Balance) return false;
        Balance -= amount;
        EventBus.Instance?.Publish(new CommandPointsChangedEvent(Balance, -amount));
        return true;
    }
}
