using System;
using System.Collections.Generic;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

// GDD §8.3, §19 prompt 39. Exercises the data (all 18 doctrines, the six
// shared ability behaviors, the passive hooks) rather than any per-doctrine
// bespoke code — there isn't any.
public class DoctrineTests : TestClass
{
    public DoctrineTests(Node testScene) : base(testScene) { }

    private static readonly (string Nation, string[] Ids)[] NationDoctrines =
    {
        ("united_states", new[] { "lend_lease", "airborne", "combined_arms" }),
        ("britain", new[] { "desert_rats", "bomber_command", "home_guard" }),
        ("soviet_union", new[] { "deep_battle", "scorched_earth", "guards_rifles" }),
        ("germany", new[] { "panzer_doctrine", "fortress", "kampfgruppe" }),
        ("italy", new[] { "alpini", "celere", "regia_marina" }),
        ("japan", new[] { "island_defense", "fortified_line", "naval_support" }),
    };

    // --- (a) data validation ------------------------------------------------

    [Test]
    public void AllEighteenDoctrinesLoadWithUniqueIdsValidKindsAndValidUtilityIds()
    {
        var seenIds = new HashSet<string>();
        foreach (var (nation, ids) in NationDoctrines)
        {
            foreach (var id in ids)
            {
                var doctrine = GD.Load<DoctrineDefinition>($"res://assets/data/doctrines/{nation}_{id}.tres");
                Require(doctrine != null, $"{nation}_{id} loads");
                Require(doctrine.Id == id, $"{nation}_{id} Id matches filename");
                Require(doctrine.NationId == nation, $"{nation}_{id} NationId matches");
                Require(seenIds.Add(doctrine.Id), $"{doctrine.Id} is unique across all 18");
                Require(doctrine.Passive != null, $"{id} has a Passive");
                Require(doctrine.Ability != null, $"{id} has an Ability");
                Require(Enum.IsDefined(typeof(DoctrineAbilityKind), doctrine.Ability.Kind), $"{id} Kind is one of the six shared behaviors");
                if (doctrine.Ability.Kind == DoctrineAbilityKind.InstantRefund)
                    Require(DoctrineUtilityId.IsValid(doctrine.Ability.UtilityId), $"{id} UtilityId is in the closed set");
            }
        }
        Require(seenIds.Count == 18, "all 18 doctrines present");
    }

    [Test]
    public void NationProfilesDeclareExactlyTheirThreeDoctrineIds()
    {
        foreach (var (nation, ids) in NationDoctrines)
        {
            var profile = GD.Load<NationProfile>($"res://assets/data/nations/{nation}.tres");
            Require(profile.DoctrineIds.Length == 3, $"{nation} declares three doctrines");
            foreach (var id in ids)
                Require(Array.IndexOf(profile.DoctrineIds, id) >= 0, $"{nation}.DoctrineIds contains {id}");
        }
    }

    // --- (b) the six shared behaviors resolve in a headless fixture --------

    [Test]
    public void PointBlastKindDealsRadiusDamage()
    {
        var fixture = new Fixture(TestScene);
        var enemy = fixture.SpawnEnemy(320f);
        fixture.Grid.Rebuild(new List<ITargetable> { enemy });
        var ability = new DoctrineAbility
        {
            Kind = DoctrineAbilityKind.PointBlast, Damage = 100f, DamageType = DamageType.Explosive, RadiusTiles = 3f,
        };
        var system = fixture.BuildSystem(MakeDoctrine(ability));
        float startHp = enemy.CurrentHp;

        Require(system.TryActivate(enemy.GlobalPosition, fixture.Grid), "point blast activates");
        Require(enemy.CurrentHp < startHp, "point blast damage lands");

        fixture.Dispose();
    }

    [Test]
    public void LineBlastKindDamagesTargetsAlongThePath()
    {
        var fixture = new Fixture(TestScene);
        var enemy = fixture.SpawnEnemy(320f);
        fixture.Grid.Rebuild(new List<ITargetable> { enemy });
        var ability = new DoctrineAbility
        {
            Kind = DoctrineAbilityKind.LineBlast, LineMode = DoctrineLineMode.PathSegment, SweepSeconds = 2f,
            Damage = 100f, DamageType = DamageType.Explosive, LengthTiles = 6f, WidthTiles = 2f,
        };
        var system = fixture.BuildSystem(MakeDoctrine(ability));
        float startHp = enemy.CurrentHp;

        Require(system.TryActivate(enemy.GlobalPosition, fixture.Grid), "line blast activates (queues a sweep)");
        system.Tick(2f, fixture.Grid); // resolves the full sweep in one tick
        Require(enemy.CurrentHp < startHp, "line blast damage lands along the path");

        fixture.Dispose();
    }

    [Test]
    public void AuraBuffKindAppliesTowerMultipliersForItsDuration()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        var tower = (TowerController)fixture.Placement.TryPlace(definition, pad).PlacedInstance;
        var ability = new DoctrineAbility
        {
            Kind = DoctrineAbilityKind.AuraBuff, DamageMultiplier = 1.5f, RangeMultiplier = 1.2f, DurationSeconds = 5f,
        };
        var system = fixture.BuildSystem(MakeDoctrine(ability));

        Require(system.TryActivate(Vector2.Zero, fixture.Grid), "aura buff activates");
        system.Tick(0.01f, fixture.Grid);
        Require(tower.DoctrineDamageMultiplier == 1.5f, "aura buff applies its damage multiplier");
        Require(tower.DoctrineRangeMultiplier == 1.2f, "aura buff applies its range multiplier");

        fixture.Dispose();
    }

    [Test]
    public void SpawnFriendlyKindSpawnsTheAuthoredUnitCount()
    {
        var fixture = new Fixture(TestScene);
        var ability = new DoctrineAbility
        {
            Kind = DoctrineAbilityKind.SpawnFriendly,
            FriendlyUnit = GD.Load<FriendlyUnitDefinition>("res://assets/data/enemies/friendly_rifle_squad.tres"),
            FriendlyUnitScene = GD.Load<PackedScene>("res://scenes/enemies/friendly_unit.tscn"),
            FriendlyCount = 2,
        };
        var system = fixture.BuildSystem(MakeDoctrine(ability));

        Require(system.TryActivate(fixture.Path.GetPositionAtDistance(320f), fixture.Grid), "spawn friendly activates");
        Require(fixture.FriendlyUnits.LivingCount == 2, "authored FriendlyCount spawned");

        fixture.Dispose();
    }

    [Test]
    public void InstantRefundKindCreditsTheTowersFullInvestedCost()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        var tower = (TowerController)fixture.Placement.TryPlace(definition, pad).PlacedInstance;
        int investedCost = tower.Upgrade.TotalInvested;
        int balanceBefore = fixture.Supply.Balance;
        var ability = new DoctrineAbility { Kind = DoctrineAbilityKind.InstantRefund, UtilityId = DoctrineUtilityId.RefundTower };
        var system = fixture.BuildSystem(MakeDoctrine(ability));

        Require(system.TryActivate(Vector2.Zero, fixture.Grid, towerTarget: tower), "refund_tower activates");
        Require(fixture.Supply.Balance == balanceBefore + investedCost, "100% of invested cost is credited");
        Require(fixture.Towers.Towers.Count == 0, "refunded tower is unregistered");

        fixture.Dispose();
    }

    [Test]
    public void StatusApplicationKindAppliesSuppressed()
    {
        var fixture = new Fixture(TestScene);
        var enemy = fixture.SpawnEnemy(320f);
        fixture.Grid.Rebuild(new List<ITargetable> { enemy });
        var ability = new DoctrineAbility
        {
            Kind = DoctrineAbilityKind.StatusApplication, StatusId = "Suppressed", RadiusTiles = 5f, DurationSeconds = 6f,
        };
        var system = fixture.BuildSystem(MakeDoctrine(ability));

        Require(system.TryActivate(enemy.GlobalPosition, fixture.Grid), "status application activates");
        Require(enemy.Status.IsSuppressed, "target is Suppressed");

        fixture.Dispose();
    }

    // --- (c) passive hooks ---------------------------------------------------

    [Test]
    public void CostMultiplierChangesPlacementCost()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        int fullCost = definition.PreForkStatsForLevel(1).Cost;
        var passive = new DoctrinePassive { TowerCostMultiplier = 0.5f };
        var system = fixture.BuildSystem(MakeDoctrine(new DoctrineAbility(), passive));
        fixture.Placement.DoctrineCostMultiplierProvider = system.PlacementCostMultiplier;
        int startBalance = fixture.Supply.Balance;

        var outcome = fixture.Placement.TryPlace(definition, pad);

        Require(outcome.Success, "placement succeeds at the discounted cost");
        Require(fixture.Supply.Balance == startBalance - Mathf.RoundToInt(fullCost * 0.5f), "half-price placement cost");

        fixture.Dispose();
    }

    [Test]
    public void IncomeMultiplierChangesEndOfWaveIncome()
    {
        var fixture = new Fixture(TestScene);
        int baseline = fixture.Supply.EndOfWaveIncome(5);
        var passive = new DoctrinePassive { SupplyIncomeMultiplier = 1.5f };
        var system = fixture.BuildSystem(MakeDoctrine(new DoctrineAbility(), passive));

        system.ApplyMissionStart();

        Require(fixture.Supply.EndOfWaveIncome(5) == Mathf.RoundToInt(baseline * 1.5f), "end-of-wave income scales with the doctrine's income multiplier");

        fixture.Dispose();
    }

    [Test]
    public void ArchetypeFilteredRateOfFireAppliesOnlyToThatArchetype()
    {
        var fixture = new Fixture(TestScene);
        var padA = fixture.MakePad(new Vector2(200, 200));
        var padB = fixture.MakePad(new Vector2(300, 200));
        var automaticGun = (TowerController)fixture.Placement.TryPlace(
            GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres"), padA).PlacedInstance;
        var antiTankGun = (TowerController)fixture.Placement.TryPlace(
            GD.Load<TowerDefinition>("res://assets/data/towers/t4_anti_tank_gun.tres"), padB).PlacedInstance;
        var passive = new DoctrinePassive { ArchetypeFilter = (int)TowerArchetype.AutomaticGun, RateOfFireMultiplier = 2f };
        var system = fixture.BuildSystem(MakeDoctrine(new DoctrineAbility(), passive));

        system.Tick(0.01f, fixture.Grid);

        Require(automaticGun.DoctrineRateOfFireMultiplier == 2f, "the filtered archetype gets the rate-of-fire multiplier");
        Require(antiTankGun.DoctrineRateOfFireMultiplier == 1f, "a different archetype is unaffected");

        fixture.Dispose();
    }

    [Test]
    public void PadTagFilteredDamageAppliesOnlyOnThatTag()
    {
        var fixture = new Fixture(TestScene);
        var enclosedPad = fixture.MakePad(new Vector2(200, 200));
        enclosedPad.Tag = PadTag.Enclosed;
        var standardPad = fixture.MakePad(new Vector2(300, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        var enclosedTower = (TowerController)fixture.Placement.TryPlace(definition, enclosedPad).PlacedInstance;
        var standardTower = (TowerController)fixture.Placement.TryPlace(definition, standardPad).PlacedInstance;
        var passive = new DoctrinePassive { PadTagFilter = (int)PadTag.Enclosed, DamageMultiplier = 1.5f };
        var system = fixture.BuildSystem(MakeDoctrine(new DoctrineAbility(), passive));

        system.Tick(0.01f, fixture.Grid);

        Require(enclosedTower.DoctrineDamageMultiplier == 1.5f, "the filtered pad tag gets the damage multiplier");
        Require(standardTower.DoctrineDamageMultiplier == 1f, "a different pad tag is unaffected");

        fixture.Dispose();
    }

    [Test]
    public void DefenseLineBonusRaisesMaxIntegrity()
    {
        var fixture = new Fixture(TestScene);
        int startMax = fixture.DefenseLine.MaxIntegrity;
        var passive = new DoctrinePassive { DefenseLineBonus = 6 };
        var system = fixture.BuildSystem(MakeDoctrine(new DoctrineAbility(), passive));

        system.ApplyMissionStart();

        Require(fixture.DefenseLine.MaxIntegrity == startMax + 6, "Defense Line max integrity is raised");
        Require(fixture.DefenseLine.Integrity == startMax + 6, "the bonus is also granted as current Integrity");

        fixture.Dispose();
    }

    // --- (d) once per mission --------------------------------------------

    [Test]
    public void OncePerMissionAbilityRefusesASecondActivation()
    {
        var fixture = new Fixture(TestScene);
        var ability = new DoctrineAbility
        {
            Kind = DoctrineAbilityKind.InstantRefund, UtilityId = DoctrineUtilityId.DefenseLineBonus,
            DefenseLineBonus = 6, OncePerMission = true, CooldownSeconds = 0f,
        };
        var system = fixture.BuildSystem(MakeDoctrine(ability));

        Require(system.TryActivate(Vector2.Zero, fixture.Grid), "first activation succeeds");
        Require(!system.TryActivate(Vector2.Zero, fixture.Grid), "second activation is refused");
        Require(system.IsExhausted, "the ability reports itself exhausted");

        fixture.Dispose();
    }

    private static DoctrineDefinition MakeDoctrine(DoctrineAbility ability, DoctrinePassive passive = null) => new()
    {
        Id = "test_doctrine",
        NationId = "test",
        DisplayName = "Test Doctrine",
        AbilityName = "Test Ability",
        Passive = passive ?? new DoctrinePassive(),
        Ability = ability,
    };

    // Groups the real (non-fake) collaborators DoctrineSystem needs, built
    // fresh per test — matches BuildTests.Fixture's approach so Supply/pad
    // state never leaks between cases.
    private sealed class Fixture
    {
        public readonly GameBalanceConfig Config = new();
        public readonly SupplyLedger Supply;
        public readonly DefenseLineLedger DefenseLine;
        public readonly CommandPointLedger CommandPoints;
        public readonly TowerManager Towers = new();
        public readonly CommandPostManager CommandPosts = new();
        public readonly MinefieldManager Minefields = new();
        public readonly SignatureManager Signatures = new();
        public readonly FriendlyUnitManager FriendlyUnits;
        public readonly TowerPlacementService Placement;
        public readonly ProjectileManager Projectiles;
        public readonly PathNetwork Path;
        public readonly SpatialGrid Grid;

        private readonly Node _testScene;
        private readonly Node _towerContainer = new();
        private readonly Node _commandPostContainer = new();
        private readonly Node _projectileContainer = new();
        private readonly Node _friendlyContainer = new();

        public Fixture(Node testScene)
        {
            _testScene = testScene;
            Supply = new SupplyLedger(Difficulty.Regular, Config);
            DefenseLine = new DefenseLineLedger(Difficulty.Regular, Config);
            CommandPoints = new CommandPointLedger(Config, () => 0);
            testScene.AddChild(_towerContainer);
            testScene.AddChild(_commandPostContainer);
            testScene.AddChild(_projectileContainer);
            testScene.AddChild(_friendlyContainer);
            Placement = new TowerPlacementService(_towerContainer, _commandPostContainer, Supply, Towers, CommandPosts);
            Projectiles = new ProjectileManager(_projectileContainer);
            FriendlyUnits = new FriendlyUnitManager(_friendlyContainer);
            Path = CreatePath(testScene);
            Grid = new SpatialGrid(Config.SpatialGridCellSizeTiles * Config.TilePixelSize);
        }

        public DoctrineSystem BuildSystem(DoctrineDefinition doctrine) => new(doctrine, Config, Towers, CommandPosts,
            Minefields, Signatures, FriendlyUnits, Path, Placement, Projectiles, CommandPoints, Supply, DefenseLine);

        public BuildPad MakePad(Vector2 position)
        {
            var pad = GD.Load<PackedScene>("res://scenes/map/build_pad.tscn").Instantiate<BuildPad>();
            pad.Position = position;
            _testScene.AddChild(pad);
            return pad;
        }

        public EnemyController SpawnEnemy(float distanceAlongPath)
        {
            var enemy = new EnemyController { Definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e6_medium_armor.tres") };
            _testScene.AddChild(enemy);
            enemy.Initialize(Path, 1f);
            enemy.GlobalPosition = Path.GetPositionAtDistance(distanceAlongPath);
            return enemy;
        }

        public void Dispose()
        {
            Supply.Dispose();
            DefenseLine.Dispose();
            CommandPoints.Dispose();
            _towerContainer.QueueFree();
            _commandPostContainer.QueueFree();
            _projectileContainer.QueueFree();
            _friendlyContainer.QueueFree();
            Path.QueueFree();
        }

        private static PathNetwork CreatePath(Node testScene)
        {
            var path = new PathNetwork();
            var curve = new Curve2D();
            curve.AddPoint(Vector2.Zero);
            curve.AddPoint(new Vector2(640f, 0f));
            path.AddChild(new Path2D { Name = "Route", Curve = curve });
            testScene.AddChild(path);
            path._Ready();
            return path;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }
}
