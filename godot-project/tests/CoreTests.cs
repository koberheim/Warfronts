using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;

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
