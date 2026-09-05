using System.Collections.Generic;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

// R07 (docs/RELEASE_COMPLETION.md): SpecialPlacementService's route-placed
// Minefield and CommandPostController's Forward Observer Spotted pulse were
// both fully authored/implemented but never called from anywhere (see
// docs/DECISIONS.md D75/D77) - these tests exercise the real behavior now
// that both are wired in.
public class PlacementIntegrationTests : TestClass
{
    public PlacementIntegrationTests(Node testScene) : base(testScene) { }

    [Test]
    public void MinefieldPlacesOnRouteWithinToleranceAndSpendsExactCost()
    {
        var fixture = new MinefieldFixture(TestScene);
        int cost = fixture.Definition.PreForkStatsForLevel(1).Cost;
        int startBalance = fixture.Supply.Balance;

        var outcome = fixture.Placement.TryPlaceMinefield(fixture.Definition, new Vector2(320f, 0f));

        Require(outcome.Success, "placement on the route succeeds");
        Require(outcome.PlacedInstance is MinefieldController, "placed instance is a MinefieldController");
        Require(fixture.Minefields.Fields.Count == 1, "registered with MinefieldManager");
        Require(fixture.Supply.Balance == startBalance - cost, "exact L1 cost spent");

        fixture.Dispose();
    }

    [Test]
    public void MinefieldFarFromAnyRouteIsRefused()
    {
        var fixture = new MinefieldFixture(TestScene);
        int startBalance = fixture.Supply.Balance;

        var outcome = fixture.Placement.TryPlaceMinefield(fixture.Definition, new Vector2(320f, 500f));

        Require(outcome.Result == TowerPlacementResult.RequiresPath, "refused off-route beyond the snap tolerance");
        Require(fixture.Minefields.Fields.Count == 0, "nothing registered");
        Require(fixture.Supply.Balance == startBalance, "no spend on a refused placement");

        fixture.Dispose();
    }

    [Test]
    public void MinefieldTooCloseToAnExistingFieldIsRefused()
    {
        var fixture = new MinefieldFixture(TestScene);

        var first = fixture.Placement.TryPlaceMinefield(fixture.Definition, new Vector2(320f, 0f));
        var second = fixture.Placement.TryPlaceMinefield(fixture.Definition, new Vector2(400f, 0f)); // 80px < 160px (2.5-tile) spacing

        Require(first.Success, "the first field is placed");
        Require(second.Result == TowerPlacementResult.TooCloseToMinefield, "a second field within the spacing rule is refused");
        Require(fixture.Minefields.Fields.Count == 1, "only the first field is registered");

        fixture.Dispose();
    }

    [Test]
    public void MinefieldFieldLimitIsEnforced()
    {
        var fixture = new MinefieldFixture(TestScene);
        fixture.Supply.Credit(10000); // isolate the field-count cap from the Supply cost check
        int max = fixture.Config.MinefieldMaximumFields;

        for (int i = 0; i < max; i++)
        {
            var outcome = fixture.Placement.TryPlaceMinefield(fixture.Definition, new Vector2(i * 320f, 0f));
            Require(outcome.Success, $"field {i} within the map cap is placed");
        }
        var overCap = fixture.Placement.TryPlaceMinefield(fixture.Definition, new Vector2(max * 320f, 0f));

        Require(overCap.Result == TowerPlacementResult.FieldLimitReached, "a field beyond the map cap is refused");
        Require(fixture.Minefields.Fields.Count == max, "field count stays at the cap");

        fixture.Dispose();
    }

    [Test]
    public void ForwardObserverPulseMarksOnlyTheStrongestEnemyInRadius()
    {
        var post = MakeCommandPost(branchIsForwardObserver: true, level: 3);
        var path = MakePath();
        var strong = MakeEnemy(path, hpScale: 2f, position: new Vector2(300f, 0f));
        var weak = MakeEnemy(path, hpScale: 1f, position: new Vector2(400f, 0f));
        var outOfRange = MakeEnemy(path, hpScale: 3f, position: new Vector2(700f, 0f)); // beyond the 10-tile (640px) radius

        post.TickSpottedPulse(4f, new List<EnemyController> { strong, weak, outOfRange }, 64f);

        Require(strong.Status.IsSpotted, "the strongest in-radius enemy is marked Spotted");
        Require(!weak.Status.IsSpotted, "a weaker in-radius enemy is left alone - only the strongest is marked");
        Require(!outOfRange.Status.IsSpotted, "an enemy outside the 10-tile radius is untouched even though it is the strongest overall");

        post.Free();
    }

    [Test]
    public void ForwardObserverPulseIsInertBeforeTheBranchForkAndOnTheLogisticsBranch()
    {
        var preForkPost = MakeCommandPost(branchIsForwardObserver: true, level: 2); // L2: not yet forked
        var logisticsPost = MakeCommandPost(branchIsForwardObserver: false, level: 3);
        var path = MakePath();
        var nearPreFork = MakeEnemy(path, hpScale: 1f, position: new Vector2(100f, 0f));
        var nearLogistics = MakeEnemy(path, hpScale: 1f, position: new Vector2(100f, 0f));

        preForkPost.TickSpottedPulse(4f, new List<EnemyController> { nearPreFork }, 64f);
        logisticsPost.TickSpottedPulse(4f, new List<EnemyController> { nearLogistics }, 64f);

        Require(!nearPreFork.Status.IsSpotted, "L1/L2 Command Posts have no StatusEffectId yet - the pulse must not fire");
        Require(!nearLogistics.Status.IsSpotted, "the Logistics Depot branch never sets StatusEffectId - the pulse must not fire");

        preForkPost.Free();
        logisticsPost.Free();
    }

    private CommandPostController MakeCommandPost(bool branchIsForwardObserver, int level)
    {
        var branch = new TowerBranch
        {
            Levels = new[]
            {
                new TowerStatBlock
                {
                    Cost = 259, AuraRadiusTiles = 8f, AuraRangeBonusPercent = 0.20f, AuraRateOfFireBonusPercent = 0.09f,
                    RangeTiles = 10f,
                    StatusEffectId = branchIsForwardObserver ? "Spotted" : "",
                    StatusDurationSeconds = branchIsForwardObserver ? 4f : 0f,
                },
                new TowerStatBlock
                {
                    Cost = 473, AuraRadiusTiles = 10f, AuraRangeBonusPercent = 0.23f, AuraRateOfFireBonusPercent = 0.09f,
                    RangeTiles = 10f,
                    StatusEffectId = branchIsForwardObserver ? "Spotted" : "",
                    StatusDurationSeconds = branchIsForwardObserver ? 4f : 0f,
                },
            },
        };
        var definition = new TowerDefinition
        {
            Levels = new[]
            {
                new TowerStatBlock { Cost = 225, AuraRadiusTiles = 6f, AuraRangeBonusPercent = 0.12f, AuraRateOfFireBonusPercent = 0.08f },
                new TowerStatBlock { Cost = 146, AuraRadiusTiles = 6.5f, AuraRangeBonusPercent = 0.13f, AuraRateOfFireBonusPercent = 0.09f },
            },
            BranchA = branch,
        };
        var post = new CommandPostController { Definition = definition, Position = Vector2.Zero };
        post._Ready();
        for (int i = 1; i < level; i++) post.Upgrade.Upgrade(TowerBranchChoice.A);
        return post;
    }

    private PathNetwork MakePath()
    {
        var path = new PathNetwork();
        var curve = new Curve2D();
        curve.AddPoint(Vector2.Zero);
        curve.AddPoint(new Vector2(3200f, 0f));
        path.AddChild(new Path2D { Name = "Route", Curve = curve });
        TestScene.AddChild(path);
        path._Ready();
        return path;
    }

    private EnemyController MakeEnemy(PathNetwork path, float hpScale, Vector2 position)
    {
        var enemy = new EnemyController { Definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e1_basic_infantry.tres") };
        TestScene.AddChild(enemy);
        enemy.Initialize(path, hpScale);
        enemy.GlobalPosition = position;
        return enemy;
    }

    // Groups the real collaborators SpecialPlacementService needs, matching
    // BuildTests' Fixture convention for TowerPlacementService.
    private sealed class MinefieldFixture
    {
        public readonly SupplyLedger Supply;
        public readonly MinefieldManager Minefields = new();
        public readonly SpecialPlacementService Placement;
        public readonly TowerDefinition Definition = GD.Load<TowerDefinition>("res://assets/data/towers/t8_minefield.tres");
        public readonly GameBalanceConfig Config = new();

        private readonly PathNetwork _path;

        public MinefieldFixture(Node testScene)
        {
            Supply = new SupplyLedger(Difficulty.Regular, Config);
            _path = new PathNetwork();
            var curve = new Curve2D();
            curve.AddPoint(Vector2.Zero);
            curve.AddPoint(new Vector2(3200f, 0f));
            _path.AddChild(new Path2D { Name = "Route", Curve = curve });
            testScene.AddChild(_path);
            _path._Ready();
            Minefields.Initialize(() => new List<ITargetable>());
            // _path is already a real, tree-resident Node2D at world origin -
            // reusing it as the placement parent means Dispose's QueueFree
            // also cleans up any MinefieldController instances placed below.
            Placement = new SpecialPlacementService(
                _path, Supply, new SignatureManager(), Minefields,
                new FriendlyUnitManager(testScene), new List<PathNetwork> { _path }, Config);
        }

        public void Dispose()
        {
            Supply.Dispose();
            _path.QueueFree();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.InvalidOperationException($"Assertion failed: {message}");
    }
}
