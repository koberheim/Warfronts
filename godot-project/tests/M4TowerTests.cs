using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Map.Planning;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

public class M4TowerTests : TestClass
{
    public M4TowerTests(Node testScene) : base(testScene) { }

    [Test]
    public void M4ArchetypesLoadWithBothUpgradeBranches()
    {
        foreach (var id in new[] { "t2_marksman_post", "t5_flak_battery", "t6_armored_emplacement", "t7_heavy_artillery", "t8_minefield" })
        {
            var definition = GD.Load<TowerDefinition>($"res://assets/data/towers/{id}.tres");
            Require(definition != null, $"{id} resource loads");
            Require(definition.Levels.Length == 2, $"{id} has shared L1/L2 data");
            Require(definition.BranchA?.Levels.Length == 2 && definition.BranchB?.Levels.Length == 2,
                $"{id} has two L3/L4 branches");
            Require(definition.BranchA.BehaviorId != definition.BranchB.BehaviorId, $"{id} branches have distinct behavior IDs");
        }
    }

    [Test]
    public void M4MechanicProfilesMatchTheGdd()
    {
        var marksman = Load("t2_marksman_post");
        RequireApproximately(0.75f, marksman.Levels[0].DamageMultiplier, "Marksman AP-light scale");
        Require(marksman.BranchA.Levels[0].StatusEffectId == "Spotted", "Overwatch applies Spotted");
        Require(marksman.BranchB.Levels[0].BlastRadiusTiles > 0f, "Anti-Materiel gains splash");

        var flak = Load("t5_flak_battery");
        Require(flak.Levels[0].TargetDomain == TargetDomain.Air, "Flak is air-only before the fork");
        Require(flak.BranchA.Levels[0].TargetDomain == TargetDomain.Air &&
            flak.BranchA.Levels[0].SecondaryTargetDomain == TargetDomain.Ground, "Dual Purpose gains ground fire");
        Require(flak.BranchB.Levels[0].StatusEffectId == "Spotted", "Predictive Fire marks air targets");

        var emplacement = Load("t6_armored_emplacement");
        Require(emplacement.Levels[0].SecondaryRateOfFirePerSec == 4f, "Emplacement has coaxial fire");
        Require(emplacement.BranchA.Levels[0].SecondaryRateOfFirePerSec == 0f, "Heavy Turret loses coaxial fire");
        Require(emplacement.BranchB.Levels[0].UsesDamageTypeOverride &&
            emplacement.BranchB.Levels[0].DamageTypeOverride == Combat.DamageType.Explosive, "Assault Gun becomes explosive");

        var artillery = Load("t7_heavy_artillery");
        Require(artillery.Levels[0].SalvoCount == 3 && artillery.BranchA.Levels[0].SalvoCount == 1 &&
            artillery.BranchB.Levels[0].SalvoCount == 9, "Artillery branches have distinct salvo sizes");
        Require(artillery.BranchA.Levels[0].FireDelaySeconds >= 1.4f && artillery.Levels[0].FireDelaySeconds >= 1.4f,
            "Artillery fire delay hard floor");

        var minefield = Load("t8_minefield");
        Require(minefield.Levels[0].MaxCharges == 6 && minefield.Levels[0].ChargeRegenSeconds == 12f,
            "Minefield charges and regeneration");
        Require(minefield.BranchA.Levels[0].DamageTypeOverride == Combat.DamageType.ArmorPiercing,
            "Anti-Tank Mines use AP");
        Require(minefield.BranchB.Levels[0].StatusEffectId == "Suppressed", "Concussion Charges suppress");
    }

    [Test]
    public void MinefieldPlacementAndChargesRespectLimits()
    {
        var fields = new System.Collections.Generic.List<MinefieldField>
        {
            new() { Position = new PlanPoint(10, 10), Charges = 2 },
        };
        Require(!MinefieldRules.CanPlace(fields, new PlanPoint(11, 10), 6, 2.5f), "minimum field spacing");
        Require(MinefieldRules.CanPlace(fields, new PlanPoint(20, 10), 6, 2.5f), "valid field placement");
        var field = fields[0];
        Require(!MinefieldRules.TryTrigger(field, 1f, 1.2f, 0.4f), "arming delay prevents trigger");
        Require(MinefieldRules.TryTrigger(field, 1f, 1.2f, 0f) && field.Charges == 1, "trigger consumes one charge");
    }

    private static TowerDefinition Load(string id) => GD.Load<TowerDefinition>($"res://assets/data/towers/{id}.tres");

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
