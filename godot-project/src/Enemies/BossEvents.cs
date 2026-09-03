namespace FrontsOfWar.Enemies;

public readonly struct BossAddsRequestedEvent
{
    public readonly EnemyController Boss;
    public readonly int Count;
    public BossAddsRequestedEvent(EnemyController boss, int count) { Boss = boss; Count = count; }
}

public readonly struct BossReachedObjectiveEvent
{
    public readonly EnemyController Boss;
    public BossReachedObjectiveEvent(EnemyController boss) => Boss = boss;
}
