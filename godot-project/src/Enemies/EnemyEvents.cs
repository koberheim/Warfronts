using FrontsOfWar.Combat;

namespace FrontsOfWar.Enemies;

public readonly struct EnemyKilledEvent
{
    public readonly EnemyController Enemy;
    public readonly int Bounty;

    public EnemyKilledEvent(EnemyController enemy, int bounty)
    {
        Enemy = enemy;
        Bounty = bounty;
    }
}

public readonly struct EnemyLeakedEvent
{
    public readonly EnemyController Enemy;
    public readonly int LeakCost;

    public EnemyLeakedEvent(EnemyController enemy, int leakCost)
    {
        Enemy = enemy;
        LeakCost = leakCost;
    }
}

// Published on every damage instance — the ricochet/ineffective-damage
// audiovisual feedback (GDD §5.7, M2 prompt 22) subscribes to this rather
// than TowerController reaching into UI/VFX directly.
public readonly struct EnemyDamagedEvent
{
    public readonly EnemyController Enemy;
    public readonly float DamageDealt;
    public readonly float Multiplier;
    public readonly DamageType DamageType;

    public EnemyDamagedEvent(EnemyController enemy, float damageDealt, float multiplier, DamageType damageType)
    {
        Enemy = enemy;
        DamageDealt = damageDealt;
        Multiplier = multiplier;
        DamageType = damageType;
    }
}
