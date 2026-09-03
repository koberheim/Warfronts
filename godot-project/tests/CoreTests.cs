using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

public class CoreTests : TestClass
{
    public CoreTests(Node testScene) : base(testScene) { }

    [Test]
    public void DamageTableCoversAllSixteenCells()
    {
        var expected = new[,]
        {
            { 1.00f, 0.55f, 0.20f, 0.08f },
            { 0.85f, 1.00f, 0.65f, 0.40f },
            { 0.45f, 0.85f, 1.25f, 1.50f },
            { 1.00f, 0.75f, 0.00f, 0.00f },
        };

        var table = DamageTable.Default;
        for (int type = 0; type < 4; type++)
        for (int armor = 0; armor < 4; armor++)
            RequireApproximately(expected[type, armor],
                table.Multiplier((DamageType)type, (ArmorClass)armor),
                $"DamageTable cell [{type},{armor}]");
    }

    [Test]
    public void DamageResolverAppliesSpottedModifier()
    {
        float normal = DamageResolver.ResolveDamage(
            100f, DamageType.ArmorPiercing, ArmorClass.Armored, false, DamageTable.Default);
        float spotted = DamageResolver.ResolveDamage(
            100f, DamageType.ArmorPiercing, ArmorClass.Armored, true, DamageTable.Default);

        RequireApproximately(156.25f, spotted, "Spotted AP damage");
        RequireApproximately(1.25f, spotted / normal, "Spotted ratio");
    }

    [Test]
    public void EventBusPublishesAndUnsubscribesTypedHandlers()
    {
        var bus = EventBus.Instance;
        Require(bus != null, "EventBus autoload exists");

        int received = 0;
        Action<TestEvent> handler = evt => received += evt.Value;
        bus.Subscribe(handler);
        bus.Publish(new TestEvent(3));
        bus.Unsubscribe(handler);
        bus.Publish(new TestEvent(5));

        Require(received == 3, "EventBus publish/unsubscribe behavior");
    }

    [Test]
    public void ObjectPoolReusesReturnedInstances()
    {
        var source = new Node2D { Name = "PoolItem" };
        var packed = new PackedScene();
        Require(packed.Pack(source) == Error.Ok, "PackedScene can pack a test node");
        source.Free();

        var parent = new Node { Name = "PoolParent" };
        TestScene.AddChild(parent);
        var pool = new ObjectPool<Node2D>(packed, parent, prewarmCount: 1);

        var first = pool.Rent();
        Require(pool.LiveCount == 1, "Pool live count after rent");
        pool.Return(first);
        Require(pool.LiveCount == 0, "Pool live count after return");
        var second = pool.Rent();

        Require(ReferenceEquals(first, second), "Pool reuses returned instance");
        pool.Return(second);
        parent.Free();
    }

    [Test]
    public void SeededRandomProducesRepeatableSequences()
    {
        var first = new SeededRandom(0xF0A123UL);
        var second = new SeededRandom(0xF0A123UL);

        for (int i = 0; i < 12; i++)
        {
            Require(first.NextInt(-10, 50) == second.NextInt(-10, 50), "Seeded integer sequence");
            RequireApproximately(first.NextFloat(), second.NextFloat(), "Seeded float sequence");
            Require(first.NextBool() == second.NextBool(), "Seeded boolean sequence");
        }
    }

    [Test]
    public void CommandPostAuraAppliesExactBonuses()
    {
        var towerDefinition = new TowerDefinition
        {
            Levels = new[] { new TowerStatBlock { Cost = 100, RangeTiles = 5f, RateOfFirePerSec = 1f } },
        };
        var postDefinition = new TowerDefinition
        {
            Levels = new[]
            {
                new TowerStatBlock
                {
                    AuraRadiusTiles = 6f,
                    AuraRangeBonusPercent = 0.12f,
                    AuraRateOfFireBonusPercent = 0.08f,
                },
            },
        };
        var tower = new TowerController { Definition = towerDefinition, Position = Vector2.Zero };
        var post = new CommandPostController { Definition = postDefinition, Position = new Vector2(3f * 32f, 0f) };
        var towers = new TowerManager();

        tower._Ready();
        post._Ready();
        towers.Register(tower);
        post.ApplyAuraTo(towers, 32f);

        RequireApproximately(1.12f, tower.AuraRangeMultiplier, "Command Post range aura");
        RequireApproximately(1.08f, tower.AuraRateOfFireMultiplier, "Command Post rate-of-fire aura");

        tower.Free();
        post.Free();
    }

    private readonly struct TestEvent
    {
        public readonly int Value;
        public TestEvent(int value) => Value = value;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }

    private static void RequireApproximately(float expected, float actual, string message)
    {
        if (MathF.Abs(expected - actual) > 0.0001f)
            throw new InvalidOperationException(
                $"Assertion failed: {message}; expected {expected}, got {actual}");
    }
}
