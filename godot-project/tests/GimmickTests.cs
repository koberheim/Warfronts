using System.Collections.Generic;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

// R08 (docs/RELEASE_COMPLETION.md): GDD §11.2's five gimmick systems -
// Tide, Sandstorm ("dust"), Mud, Canopy, and the Ruined Town clipped-range
// arc - tested independently of any authored map, since none of the eight
// launch maps exist yet (R09). See docs/DECISIONS.md for the scope this
// deliberately does not cover (Tide's WaveRunner spawn-rerouting, and full
// wall/line-of-sight geometry for the arc gimmick).
public class GimmickTests : TestClass
{
    public GimmickTests(Node testScene) : base(testScene) { }

    [Test]
    public void TideClosesOnlyItsOwnPathAndOnlyDuringItsActiveWindow()
    {
        var tide = new RuntimeGimmickData
        {
            Id = "tide", Type = GimmickSystem.TideType, PathIds = new[] { "beach" },
            Parameters = new Dictionary<string, string> { ["cycle_seconds"] = "90", ["active_seconds"] = "30" },
        };
        var system = new GimmickSystem(new[] { tide });

        // Active (flooded) for the first active_seconds of every cycle_seconds
        // (matches the Sandstorm spec's own "20s on a 60s cycle" phrasing).
        Require(!system.IsPathAvailable("beach"), "the tidal path starts flooded (elapsed 0 is inside the 30s flood window)");
        system.Tick(30f); // elapsed = 30, exactly past the flood window
        Require(system.IsPathAvailable("beach"), "the path re-opens once the flood window elapses");
        system.Tick(59f); // elapsed = 89, still inside the same 90s cycle
        Require(system.IsPathAvailable("beach"), "the path stays open for the rest of the cycle");
        system.Tick(1f); // elapsed = 90 -> wraps to the start of the next cycle
        Require(!system.IsPathAvailable("beach"), "the path floods again at the start of the next cycle");
        Require(system.IsPathAvailable("upper_road"), "an unrelated path is never affected by another path's tide");
    }

    [Test]
    public void SandstormReducesGlobalTowerRangeOnlyDuringItsActiveWindow()
    {
        var sandstorm = new RuntimeGimmickData
        {
            Id = "sandstorm", Type = GimmickSystem.SandstormType,
            Parameters = new Dictionary<string, string> { ["cycle_seconds"] = "60", ["active_seconds"] = "20", ["range_multiplier"] = "0.75" },
        };
        var system = new GimmickSystem(new[] { sandstorm });

        Require(system.GlobalRangeMultiplier() == 0.75f, "active from tick 0 (elapsed 0 < 20s window)");
        system.Tick(25f);
        Require(system.GlobalRangeMultiplier() == 1f, "no penalty between sweeps");
        system.Tick(40f); // elapsed = 65 -> wraps to 5s into the next cycle
        Require(system.GlobalRangeMultiplier() == 0.75f, "the sweep recurs every cycle");
    }

    [Test]
    public void MudSlowsOnlyVehiclesOnItsOwnPath()
    {
        var mud = new RuntimeGimmickData
        {
            Id = "mud", Type = GimmickSystem.MudType, PathIds = new[] { "jungle_track" },
            Parameters = new Dictionary<string, string> { ["speed_multiplier"] = "0.6" },
        };
        var system = new GimmickSystem(new[] { mud });

        Require(system.SpeedMultiplierForPath("jungle_track", isVehicle: true) == 0.6f, "vehicles slow on the muddy path");
        Require(system.SpeedMultiplierForPath("jungle_track", isVehicle: false) == 1f, "infantry is unaffected on the same path");
        Require(system.SpeedMultiplierForPath("dry_bypass", isVehicle: true) == 1f, "a vehicle on an unrelated path is unaffected");

        Require(GimmickRules.IsVehicle(EnemyArchetype.MediumArmor), "Medium Armor is a vehicle");
        Require(GimmickRules.IsVehicle(EnemyArchetype.HeavyArmor), "Heavy Armor is a vehicle");
        Require(!GimmickRules.IsVehicle(EnemyArchetype.ArmoredInfantry), "Armored Infantry still walks - armor rating isn't vehicle-ness");
        Require(!GimmickRules.IsVehicle(EnemyArchetype.BasicInfantry), "Basic Infantry is not a vehicle");
    }

    [Test]
    public void CanopyConcealsEveryEnemyOnItsPathRegardlessOfArchetype()
    {
        var canopy = new RuntimeGimmickData { Id = "canopy", Type = GimmickSystem.CanopyType, PathIds = new[] { "forest_road" } };
        var system = new GimmickSystem(new[] { canopy });

        Require(system.IsPathConcealed("forest_road"), "the marked path is concealed");
        Require(!system.IsPathConcealed("clearing"), "an unrelated path is not concealed");
    }

    [Test]
    public void CanopyConcealmentReusesTheExistingRevealAndSpottedSystems()
    {
        var path = MakePath();
        var enemy = new EnemyController { Definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e1_basic_infantry.tres") };
        TestScene.AddChild(enemy);
        enemy.Initialize(path);

        Require(!enemy.IsConcealed, "a plain infantry enemy is not concealed before entering canopy");
        enemy.SetInCanopy(true);
        Require(enemy.IsConcealed && !enemy.IsRevealed, "canopy conceals it exactly like E11's own concealment");
        enemy.SetRevealed(true);
        Require(enemy.IsRevealed, "a Command Post's reveal still works on a canopy-concealed enemy");
        enemy.SetRevealed(false);
        enemy.ApplySpotted(4f);
        Require(enemy.IsRevealed, "Spotted also reveals a canopy-concealed enemy, same as any other concealment source");
    }

    [Test]
    public void ArcClippingRestrictsTargetingToTheAuthoredCone()
    {
        Require(GimmickRules.IsWithinArc(Vector2.Zero, facingDegrees: 0f, halfAngleDegrees: 45f, target: new Vector2(10f, 0f)),
            "directly ahead is inside a 45deg half-angle cone facing 0deg");
        Require(!GimmickRules.IsWithinArc(Vector2.Zero, facingDegrees: 0f, halfAngleDegrees: 45f, target: new Vector2(0f, 10f)),
            "90deg off-axis is outside a 45deg half-angle cone");
        Require(GimmickRules.IsWithinArc(Vector2.Zero, facingDegrees: 90f, halfAngleDegrees: 10f, target: new Vector2(0f, 10f)),
            "directly ahead of a 90deg-facing pad is inside its own narrow cone");
        Require(GimmickRules.IsWithinArc(Vector2.Zero, facingDegrees: 0f, halfAngleDegrees: 180f, target: new Vector2(0f, -10f)),
            "180deg half-angle means no clipping at all, matching every ordinary pad");
    }

    [Test]
    public void TowerWithAClippedArcRefusesATargetOutsideItsConeEvenInRange()
    {
        var path = MakePath();
        var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        var tower = definition.ControllerScene.Instantiate<TowerController>();
        tower.Definition = definition;
        tower.ArcFacingDegrees = 0f; // faces +X
        tower.ArcHalfAngleDegrees = 30f;
        TestScene.AddChild(tower);

        var ahead = new EnemyController { Definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e1_basic_infantry.tres") };
        TestScene.AddChild(ahead);
        ahead.Initialize(path);
        ahead.GlobalPosition = tower.GlobalPosition + new Vector2(50f, 0f);

        var behind = new EnemyController { Definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e1_basic_infantry.tres") };
        TestScene.AddChild(behind);
        behind.Initialize(path);
        behind.GlobalPosition = tower.GlobalPosition + new Vector2(-50f, 0f); // same distance, opposite side

        var grid = new SpatialGrid(64f);
        grid.Rebuild(new List<ITargetable> { ahead, behind });
        tower.SimTick(0.016f, grid, new ProjectileManager(TestScene));

        Require(ahead.CurrentHp < ahead.MaxHp, "the target inside the cone was hit");
        Require(behind.CurrentHp == behind.MaxHp, "the equally-in-range target outside the cone was never acquired");
    }

    private PathNetwork MakePath()
    {
        var path = new PathNetwork();
        var curve = new Curve2D();
        curve.AddPoint(Vector2.Zero);
        curve.AddPoint(new Vector2(640f, 0f));
        path.AddChild(new Path2D { Name = "Route", Curve = curve });
        TestScene.AddChild(path);
        path._Ready();
        return path;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.InvalidOperationException($"Assertion failed: {message}");
    }
}
