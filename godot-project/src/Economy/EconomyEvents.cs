namespace FrontsOfWar.Economy;

public readonly struct SupplyChangedEvent
{
    public readonly int NewBalance;
    public readonly int Delta;

    public SupplyChangedEvent(int newBalance, int delta)
    {
        NewBalance = newBalance;
        Delta = delta;
    }
}

public readonly struct DefenseLineChangedEvent
{
    public readonly int NewIntegrity;
    public readonly int Delta;

    public DefenseLineChangedEvent(int newIntegrity, int delta)
    {
        NewIntegrity = newIntegrity;
        Delta = delta;
    }
}

public readonly struct DefenseLineDepletedEvent
{
}
