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

public readonly struct EnemySiegeBombardEvent
{
    public readonly EnemyController Source;
    public readonly Godot.Vector2 Position;
    public readonly float RangeTiles;
    public readonly float DurationSeconds;

    public EnemySiegeBombardEvent(EnemyController source, Godot.Vector2 position,
                                  float rangeTiles, float durationSeconds)
    {
        Source = source;
        Position = position;
        RangeTiles = rangeTiles;
        DurationSeconds = durationSeconds;
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
    public readonly IDamageSource Source;

    public EnemyDamagedEvent(EnemyController enemy, float damageDealt, float multiplier,
                             DamageType damageType, IDamageSource source = null)
    {
        Enemy = enemy;
        DamageDealt = damageDealt;
        Multiplier = multiplier;
        DamageType = damageType;
        Source = source;
    }
}
