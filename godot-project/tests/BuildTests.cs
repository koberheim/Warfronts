using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

// GDD §7.4, §7.5, §6, §19 prompts 18-19. Builds fixtures from real ledgers,
// managers, and the actual scenes/resources rather than fakes, per the task
// brief — placement/selling is exactly the kind of flow where a stub
// TowerController could hide a real Godot lifecycle bug.
public class BuildTests : TestClass
{
    public BuildTests(Node testScene) : base(testScene) { }

    [Test]
    public void PlacementSpendsExactCostAndRegistersTower()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        int startBalance = fixture.Supply.Balance;
        int cost = definition.PreForkStatsForLevel(1).Cost;

        var outcome = fixture.Placement.TryPlace(definition, pad);

        Require(outcome.Result == TowerPlacementResult.Success, "placement succeeds");
        Require(outcome.PlacedInstance is TowerController, "placed instance is a TowerController");
        Require(fixture.Supply.Balance == startBalance - cost, "exact L1 cost spent");
        Require(fixture.Towers.Towers.Count == 1, "tower registered with TowerManager");
        Require(fixture.CommandPosts.Posts.Count == 0, "T1 does not register with CommandPostManager");
        Require(pad.IsOccupied, "pad marked occupied");

        fixture.Dispose();
    }

    [Test]
    public void UnaffordablePlacementIsRefusedWithShortfallAndInstantiatesNothing()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t4_anti_tank_gun.tres");
        int cost = definition.PreForkStatsForLevel(1).Cost;
        fixture.Supply.TrySpend(fixture.Supply.Balance - 50); // leaves exactly 50

        var outcome = fixture.Placement.TryPlace(definition, pad);

        Require(outcome.Result == TowerPlacementResult.InsufficientSupply, "refused for insufficient Supply");
        Require(outcome.SupplyShortfall == cost - 50, "shortfall is exact");
        Require(outcome.PlacedInstance == null, "no instance returned");
        Require(fixture.Towers.Towers.Count == 0, "nothing registered");
        Require(fixture.Supply.Balance == 50, "balance untouched by a refused placement");
        Require(!pad.IsOccupied, "pad remains free");

        fixture.Dispose();
    }

    [Test]
    public void OccupiedPadIsRefused()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        int cost = definition.PreForkStatsForLevel(1).Cost;

        var first = fixture.Placement.TryPlace(definition, pad);
        int balanceAfterFirst = fixture.Supply.Balance;
        var second = fixture.Placement.TryPlace(definition, pad);

        Require(first.Success, "first placement succeeds");
        Require(second.Result == TowerPlacementResult.PadOccupied, "second placement is refused");
        Require(second.PlacedInstance == null, "refused placement returns no instance");
        Require(fixture.Towers.Towers.Count == 1, "only the first tower registered");
        Require(fixture.Supply.Balance == balanceAfterFirst, "no double spend on a refused placement");

        fixture.Dispose();
    }

    [Test]
    public void SellingFreesThePadAndCreditsTheRefund()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        int startBalance = fixture.Supply.Balance;

        var outcome = fixture.Placement.TryPlace(definition, pad);
        var tower = (TowerController)outcome.PlacedInstance;
        int refund = tower.Sell(); // within the full-refund window (no Tick() called)
        fixture.Supply.Credit(refund);
        fixture.Towers.Unregister(tower);
        bool released = fixture.Placement.ReleasePad(tower);

        Require(released, "ReleasePad reports it freed a tracked pad");
        Require(!pad.IsOccupied, "pad is free again after selling");
        Require(refund == definition.PreForkStatsForLevel(1).Cost, "full refund inside the placement-time window");
        Require(fixture.Supply.Balance == startBalance, "spend + full refund nets back to the starting balance");
        Require(fixture.Towers.Towers.Count == 0, "sold tower is unregistered");

        // Re-placing on the now-free pad must succeed.
        var replaced = fixture.Placement.TryPlace(definition, pad);
        Require(replaced.Success, "the freed pad accepts a new placement");

        fixture.Dispose();
    }

    [Test]
    public void PlacingCommandPostRegistersWithCommandPostManagerNotTowerManager()
    {
        var fixture = new Fixture(TestScene);
        var pad = fixture.MakePad(new Vector2(200, 200));
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t9_command_post.tres");

        var outcome = fixture.Placement.TryPlace(definition, pad);

        Require(outcome.Success, "Command Post placement succeeds");
        Require(outcome.PlacedInstance is CommandPostController, "placed instance is a CommandPostController");
        Require(fixture.CommandPosts.Posts.Count == 1, "registered with CommandPostManager");
        Require(fixture.Towers.Towers.Count == 0, "not registered with TowerManager");

        fixture.Dispose();
    }

    // Groups the real (non-fake) collaborators TowerPlacementService needs,
    // built fresh per test so Supply/pad-occupancy state never leaks between
    // cases.
    private sealed class Fixture
    {
        public readonly SupplyLedger Supply;
        public readonly TowerManager Towers = new();
        public readonly CommandPostManager CommandPosts = new();
        public readonly TowerPlacementService Placement;

        private readonly Node _testScene;
        private readonly Node _towerContainer = new();
        private readonly Node _commandPostContainer = new();

        public Fixture(Node testScene)
        {
            _testScene = testScene;
            Supply = new SupplyLedger(Difficulty.Regular, new GameBalanceConfig());
            testScene.AddChild(_towerContainer);
            testScene.AddChild(_commandPostContainer);
            Placement = new TowerPlacementService(_towerContainer, _commandPostContainer, Supply, Towers, CommandPosts);
        }

        public BuildPad MakePad(Vector2 position)
        {
            var pad = GD.Load<PackedScene>("res://scenes/map/build_pad.tscn").Instantiate<BuildPad>();
            pad.Position = position;
            _testScene.AddChild(pad);
            return pad;
        }

        public void Dispose()
        {
            Supply.Dispose();
            _towerContainer.QueueFree();
            _commandPostContainer.QueueFree();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }

    private static void RequireApproximately(float expected, float actual, string message)
    {
        if (MathF.Abs(expected - actual) > 0.0001f)
            throw new InvalidOperationException($"Assertion failed: {message}; expected {expected}, got {actual}");
    }
}
