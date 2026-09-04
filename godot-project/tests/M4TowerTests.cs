using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Map.Planning;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

public class M4TowerTests : TestClass
{
    public M4TowerTests(Node testScene) : base(testScene) { }

    [Test]
    public void M4ArchetypesLoadWithBothUpgradeBranches()
    {
        foreach (var id in new[]
        {
            "t1_automatic_gun", "t2_marksman_post", "t3_field_mortar", "t4_anti_tank_gun",
            "t5_flak_battery", "t6_armored_emplacement", "t7_heavy_artillery", "t8_minefield",
            "t9_command_post",
        })
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

    // GDD §7.4's cost table plus §6's T1/T3/T4/T9 branch forks (the four
    // "VS" scope towers that only had L1/L2 authored before this pass — see
    // docs/DECISIONS.md). Confirms the fix for the NullReferenceException
    // TowerUpgradeController.CurrentStats() used to throw at L3/L4 on these
    // four archetypes (branch.Levels[Level - 3] on a null branch).
    [Test]
    public void VsTowerBranchesMatchCostCurveAndUpgradeCleanly()
    {
        var config = new GameBalanceConfig();
        var costTable = new (string Id, int L1, int L2, int L3, int L4)[]
        {
            ("t1_automatic_gun", 100, 65, 115, 210),
            ("t3_field_mortar", 150, 98, 173, 315),
            ("t4_anti_tank_gun", 200, 130, 230, 420),
            ("t9_command_post", 225, 146, 259, 473),
        };

        foreach (var entry in costTable)
        {
            var definition = Load(entry.Id);
            Require(definition.Levels.Length == 2, $"{entry.Id} has shared L1/L2 data");
            Require(definition.BranchA?.Levels.Length == 2 && definition.BranchB?.Levels.Length == 2,
                $"{entry.Id} has two L3/L4 branches");
            Require(definition.BranchA.BehaviorId != definition.BranchB.BehaviorId,
                $"{entry.Id} branches have distinct behavior IDs");
            Require(definition.Levels[0].Cost == entry.L1, $"{entry.Id} L1 cost matches GDD §7.4");
            Require(definition.Levels[1].Cost == entry.L2, $"{entry.Id} L2 cost matches GDD §7.4");
            Require(definition.BranchA.Levels[0].Cost == entry.L3 && definition.BranchB.Levels[0].Cost == entry.L3,
                $"{entry.Id} L3 cost matches GDD §7.4 on both branches");
            Require(definition.BranchA.Levels[1].Cost == entry.L4 && definition.BranchB.Levels[1].Cost == entry.L4,
                $"{entry.Id} L4 cost matches GDD §7.4 on both branches");

            foreach (var branchChoice in new[] { TowerBranchChoice.A, TowerBranchChoice.B })
            {
                var upgrade = new TowerUpgradeController(definition, config);
                Require(upgrade.CurrentStats() != null, $"{entry.Id} L1 stats non-null");
                Require(upgrade.UpgradeCost() == entry.L2, $"{entry.Id} L1->L2 upgrade cost matches TowerUpgradeController");
                upgrade.Upgrade();
                Require(upgrade.CurrentStats() != null, $"{entry.Id} L2 stats non-null");
                Require(upgrade.UpgradeCost() == entry.L3, $"{entry.Id} L2->L3 upgrade cost matches TowerUpgradeController");
                upgrade.Upgrade(branchChoice);
                Require(upgrade.Branch == branchChoice, $"{entry.Id} entered branch {branchChoice} at the fork");
                Require(upgrade.CurrentStats() != null, $"{entry.Id} L3 ({branchChoice}) stats non-null — was the NRE this fixes");
                Require(upgrade.UpgradeCost() == entry.L4, $"{entry.Id} L3->L4 upgrade cost matches TowerUpgradeController");
                upgrade.Upgrade();
                Require(upgrade.Level == 4, $"{entry.Id} reaches L4 on branch {branchChoice}");
                Require(!upgrade.CanUpgrade, $"{entry.Id} L4 is max level");
                Require(upgrade.CurrentStats() != null, $"{entry.Id} L4 ({branchChoice}) stats non-null — was the NRE this fixes");
            }
        }
    }

    [Test]
    public void VsTowerBranchMechanicsMatchTheGdd()
    {
        var automaticGun = Load("t1_automatic_gun");
        Require(automaticGun.BranchA.Levels[0].RateOfFirePerSec > automaticGun.Levels[1].RateOfFirePerSec,
            "Sustained Fire ramps rate of fire above the pre-fork baseline");
        Require(automaticGun.BranchB.Levels[0].StatusEffectId == "Suppressed" &&
            automaticGun.BranchB.Levels[0].DamagePerShot < automaticGun.Levels[0].DamagePerShot,
            "Suppressive Fire trades damage for Suppressed");

        var fieldMortar = Load("t3_field_mortar");
        Require(fieldMortar.BranchA.Levels[0].SalvoCount == 3, "Barrage fires a 3-shell salvo");
        Require(fieldMortar.BranchB.Levels[0].StatusEffectId == "Suppressed", "Smoke Rounds applies Suppressed");
        RequireApproximately(2.2f, fieldMortar.BranchB.Levels[0].BlastRadiusTiles, "Smoke Rounds suppression radius matches the GDD's 2.2 tiles");

        var antiTankGun = Load("t4_anti_tank_gun");
        RequireApproximately(105f, antiTankGun.BranchA.Levels[0].DamagePerShot, "Sabot Rounds +40% damage over L1");
        Require(antiTankGun.BranchA.Levels[0].BlastRadiusTiles > 0f, "Sabot Rounds gains an area effect (approximates piercing)");
        RequireApproximately(60f, antiTankGun.BranchB.Levels[0].DamagePerShot, "Rapid Loader -20% damage from L1");
        Require(antiTankGun.BranchB.Levels[0].RateOfFirePerSec > antiTankGun.Levels[0].RateOfFirePerSec * 1.5f,
            "Rapid Loader +70% rate of fire from L1");

        var commandPost = Load("t9_command_post");
        Require(commandPost.BranchA.Levels[0].StatusEffectId == "Spotted", "Forward Observer applies Spotted");
        Require(commandPost.BranchB.Levels[0].SupplyPerWave == 22 && commandPost.BranchB.Levels[0].CommandPointsPerWave == 4,
            "Logistics Depot generates +22 Supply/wave and +2 additional Command Points");
        Require(commandPost.BranchB.Levels[0].AuraRangeBonusPercent < commandPost.Levels[0].AuraRangeBonusPercent,
            "Logistics Depot trades aura strength for economy");
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
