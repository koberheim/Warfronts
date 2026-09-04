using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Debug;
using FrontsOfWar.Enemies;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Tests;

// GDD §19 prompt 45 acceptance: "it catches a deliberately broken reference,
// both from inside the editor and from `godot --headless`." (b)-(d) below
// are the headless half of that check — each injects a deliberately broken
// in-memory resource via DataValidator.ValidateResources() (no files
// written) and asserts the validator reports it.
public class DataValidatorTests : TestClass
{
    public DataValidatorTests(Node testScene) : base(testScene) { }

    [Test]
    public void ShippedDataHasZeroErrors()
    {
        var report = DataValidator.ValidateProjectData();
        foreach (var warning in report.Warnings)
            GD.Print(warning.ToString());

        Require(report.ErrorCount == 0,
            $"Shipped assets/data should validate with zero errors; found: " +
            string.Join(" | ", report.Errors.Select(e => e.ToString())));
    }

    [Test]
    public void SpawnGroupWithNullEnemyIsReported()
    {
        var resources = new List<(string Path, Resource Resource)>
        {
            ("synthetic://spawn_group_null_enemy", new SpawnGroup { Enemy = null, Count = 1 }),
        };

        var report = new DataValidationReport();
        DataValidator.ValidateResources(resources, report);

        Require(report.Errors.Any(e =>
                e.Path == "synthetic://spawn_group_null_enemy" && e.Message.Contains("null Enemy")),
            "A SpawnGroup with a null Enemy reference should be reported as an error.");
    }

    [Test]
    public void DuplicateTowerIdsAreReported()
    {
        var resources = new List<(string Path, Resource Resource)>
        {
            ("synthetic://tower_a", new TowerDefinition
            {
                Id = "dup_tower_id",
                Levels = new[] { new TowerStatBlock { Cost = 100 } },
            }),
            ("synthetic://tower_b", new TowerDefinition
            {
                Id = "dup_tower_id",
                Levels = new[] { new TowerStatBlock { Cost = 120 } },
            }),
        };

        var report = new DataValidationReport();
        DataValidator.ValidateResources(resources, report);

        Require(report.Errors.Any(e => e.Path == "synthetic://tower_b" && e.Message.Contains("Duplicate Id")),
            "Two TowerDefinitions sharing an Id should be reported as an error.");
    }

    [Test]
    public void GroundEnemyWithNoRosterCounterIsReported()
    {
        var resources = new List<(string Path, Resource Resource)>
        {
            ("synthetic://small_arms_only_tower", new TowerDefinition
            {
                Id = "small_arms_only_tower",
                DamageType = DamageType.SmallArms,
                Levels = new[] { new TowerStatBlock { Cost = 100, TargetDomain = TargetDomain.Ground } },
            }),
            ("synthetic://uncounterable_heavy_enemy", new EnemyDefinition
            {
                Id = "uncounterable_heavy_enemy",
                ArmorClass = ArmorClass.Heavy,
                IsAir = false,
                BaseHp = 100f,
                MoveSpeedTilesPerSec = 1f,
                LeakCost = 1,
            }),
        };

        var report = new DataValidationReport();
        DataValidator.ValidateResources(resources, report);

        Require(report.Errors.Any(e =>
                e.Path == "synthetic://uncounterable_heavy_enemy" &&
                e.Message.Contains("no roster tower damage type reaching a 1.0x multiplier")),
            "A Heavy-armor enemy with only a Small Arms tower in the roster should be reported as an error " +
            "(Small Arms vs Heavy is 0.08x per the GDD §5.4 damage table).");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }
}
