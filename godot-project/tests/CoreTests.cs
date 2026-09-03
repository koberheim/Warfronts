using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;
using System.Collections.Generic;

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

    [Test]
    public void TowerUpgradesFollowCostCurveAndRefundWindow()
    {
        var definition = new TowerDefinition
        {
            Levels = new[]
            {
                new TowerStatBlock { Cost = 100, DamagePerShot = 10f },
                new TowerStatBlock { Cost = 120, DamagePerShot = 12f },
            },
            BranchA = new TowerBranch
            {
                Levels = new[]
                {
                    new TowerStatBlock { Cost = 160, DamagePerShot = 20f },
                    new TowerStatBlock { Cost = 220, DamagePerShot = 30f },
                },
            },
            BranchB = new TowerBranch
            {
                Levels = new[]
                {
                    new TowerStatBlock { Cost = 140, DamagePerShot = 15f },
                    new TowerStatBlock { Cost = 180, DamagePerShot = 18f },
                },
            },
        };
        var upgrades = new TowerUpgradeController(definition, new GameBalanceConfig());

        Require(upgrades.TotalInvested == 100, "Initial tower investment");
        Require(upgrades.UpgradeCost() == 65, "L2 upgrade cost");
        upgrades.Upgrade(TowerBranchChoice.A);
        Require(upgrades.Level == 2 && upgrades.TotalInvested == 165, "L2 upgrade");
        Require(upgrades.UpgradeCost() == 115, "L3 upgrade cost");
        upgrades.Upgrade(TowerBranchChoice.A);
        Require(upgrades.Level == 3 && upgrades.Branch == TowerBranchChoice.A, "Branch A fork");
        RequireApproximately(20f, upgrades.CurrentStats().DamagePerShot, "Branch A L3 stats");
        upgrades.Upgrade(TowerBranchChoice.A);
        Require(upgrades.Level == 4 && upgrades.TotalInvested == 490, "L4 upgrade");

        upgrades.Tick(5f);
        Require(upgrades.SellRefund() == 368, "Post-window sell refund");

        var fresh = new TowerUpgradeController(definition, new GameBalanceConfig());
        Require(fresh.SellRefund() == 100, "Full refund window");
    }

    [Test]
    public void StatusControllerEnforcesSuppressionCapAndExpiration()
    {
        var status = new StatusController();
        status.ApplySuppressed(3f, 4f);
        status.Tick(1f);
        status.ApplySuppressed(3f, 4f);
        Require(status.IsSuppressed, "Suppression remains active after refresh");
        status.Tick(3f);
        Require(!status.IsSuppressed, "Suppression hard cap expires");

        status.ApplySpotted(2f);
        status.Tick(1f);
        Require(status.IsSpotted, "Spotted remains active");
        status.Tick(1f);
        Require(!status.IsSpotted, "Spotted expires");
    }

    [Test]
    public void AbilitiesSpendCommandPointsWhilePaused()
    {
        var config = new GameBalanceConfig();
        var commandPoints = new CommandPointLedger(config, () => 0);
        var defenseLine = new DefenseLineLedger(Difficulty.Regular, config);
        var abilities = new AbilitySystem(config);
        var time = new TimeController();
        time.Pause();
        commandPoints.Credit(5);
        defenseLine.ForceDeplete();

        bool activated = abilities.TryActivate(
            AbilityType.EmergencyRepair, Vector2.Zero, commandPoints,
            new TowerManager(), defenseLine);

        Require(activated, "Emergency Repair activates while paused");
        Require(commandPoints.Balance == 0, "Ability CP cost");
        Require(defenseLine.Integrity == 3, "Emergency Repair restores Defense Line");
        RequireApproximately(45f, abilities.CooldownRemaining(AbilityType.EmergencyRepair), "Ability cooldown");

        commandPoints.Dispose();
        defenseLine.Dispose();
    }

    [Test]
    public void M2WaveSequenceContainsTwelveAuthoredWavesAndFourEnemies()
    {
        var sequence = GD.Load<WaveSequence>("res://assets/data/missions/m2_wave_sequence.tres");
        Require(sequence != null && sequence.Waves.Length == 12, "M2 wave sequence length");

        var enemyIds = new HashSet<string>();
        for (int i = 0; i < sequence.Waves.Length; i++)
        {
            Require(sequence.Waves[i].WaveNumber == i + 1, "Sequential wave numbering");
            foreach (var group in sequence.Waves[i].Groups) enemyIds.Add(group.Enemy.Id);
        }

        Require(enemyIds.SetEquals(new[]
        {
            "e1_basic_infantry", "e4_armored_infantry",
            "e5_light_vehicle", "e6_medium_armor",
        }), "M2 enemy roster");
    }

    [Test]
    public void TowerTracksDamageAttributedThroughCombatEvents()
    {
        var tower = new TowerController
        {
            Name = "AttributionTower",
            Definition = new TowerDefinition
            {
                Levels = new[] { new TowerStatBlock { Cost = 100 } },
            },
        };
        tower._Ready();

        EventBus.Instance.Publish(new FrontsOfWar.Enemies.EnemyDamagedEvent(
            null, 42f, 1f, DamageType.Explosive, tower));

        RequireApproximately(42f, tower.LifetimeDamage, "Tower lifetime damage attribution");
        tower._ExitTree();
        tower.Free();
    }

    [Test]
    public void FieldMortarUsesAuthoredPointTargetingAndDensestCluster()
    {
        var mortar = GD.Load<TowerDefinition>("res://assets/data/towers/t3_field_mortar.tres");
        Require(mortar != null, "Field Mortar resource loads");
        Require(mortar.DefaultTargeting == TargetingProfile.DensestCluster, "Field Mortar targeting profile");
        RequireApproximately(2f, mortar.Levels[0].MinRangeTiles, "Field Mortar minimum range");
        RequireApproximately(1.6f, mortar.Levels[0].BlastRadiusTiles, "Field Mortar blast radius");

        var candidates = new ITargetable[]
        {
            new FakeTargetable(new Vector2(100f, 100f)),
            new FakeTargetable(new Vector2(110f, 100f)),
            new FakeTargetable(new Vector2(120f, 100f)),
            new FakeTargetable(new Vector2(300f, 300f)),
        };
        var point = TargetingService.SelectDensestClusterPoint(candidates, 16f);
        Require(point == new Vector2(110f, 100f), "Densest cluster point selection");
    }

    private sealed class FakeTargetable : ITargetable
    {
        public Vector2 GlobalPosition { get; }
        public float PathProgress => 0f;
        public float CurrentHp => 100f;
        public bool IsAir => false;
        public bool IsAlive => true;
        public Vector2 Velocity => Vector2.Zero;

        public FakeTargetable(Vector2 position) => GlobalPosition = position;
        public void ApplyDamage(float baseDamage, DamageType type) { }
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
