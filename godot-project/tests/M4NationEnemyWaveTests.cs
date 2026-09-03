using System;
using System.Collections.Generic;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Tests;

public class M4NationEnemyWaveTests : TestClass
{
    public M4NationEnemyWaveTests(Node testScene) : base(testScene) { }

    [Test]
    public void SixNationProfilesStayInsideEnvelopeAndParity()
    {
        var profiles = new List<NationProfile>();
        foreach (var id in new[] { "united_states", "britain", "soviet_union", "germany", "italy", "japan" })
        {
            var profile = GD.Load<NationProfile>($"res://assets/data/nations/{id}.tres");
            Require(profile != null, $"{id} profile loads");
            profiles.Add(profile);
        }

        var roster = new List<TowerDefinition>();
        foreach (var id in new[] { "t1_automatic_gun", "t2_marksman_post", "t3_field_mortar", "t4_anti_tank_gun", "t5_flak_battery", "t6_armored_emplacement", "t7_heavy_artillery", "t8_minefield", "t9_command_post" })
            roster.Add(GD.Load<TowerDefinition>($"res://assets/data/towers/{id}.tres"));

        var report = NationBalanceValidator.Validate(profiles, roster);
        Require(report.IsValid, string.Join("; ", report.Errors));
    }

    [Test]
    public void M4EnemyDefinitionsAndSpecialRulesMatchGdd()
    {
        var swarm = GD.Load<EnemyDefinition>("res://assets/data/enemies/e3_swarm_infantry.tres");
        Require(swarm.BaseHp == 26f && swarm.MoveSpeedTilesPerSec == 1.9f && swarm.CohesionGroupSize == 8,
            "Swarm Infantry baseline and group size");

        var heavy = GD.Load<EnemyDefinition>("res://assets/data/enemies/e7_heavy_armor.tres");
        Require(heavy.BaseHp == 1400f && heavy.SuppressionImmune, "Heavy Armor baseline and immunity");
        var controller = new EnemyController { Definition = heavy };
        controller.ApplySuppressed(6f, 6f);
        Require(!controller.Status.IsSuppressed, "Heavy Armor ignores suppression");

        var siege = GD.Load<EnemyDefinition>("res://assets/data/enemies/e12_siege_artillery.tres");
        Require(siege.BaseHp == 480f && siege.SiegeBombardRangeTiles == 11f && siege.SiegeSuppressionDurationSeconds == 6f,
            "Siege baseline and bombard timing");
        Require(SiegeRules.ShouldSuppress(PadTag.Standard, 11f, 11f), "Siege suppresses an in-range standard pad");
        Require(!SiegeRules.ShouldSuppress(PadTag.Enclosed, 1f, 11f), "Enclosed pads are siege immune");
        Require(!SiegeRules.ShouldSuppress(PadTag.Standard, 11.1f, 11f), "Out-of-range pads are safe");
    }

    [Test]
    public void ThreatValueAndPacingWarningsAreComputed()
    {
        var enemy = new EnemyDefinition { Id = "test", BaseHp = 100f, LeakCost = 1, MoveSpeedTilesPerSec = 1f };
        var previous = new WaveDefinition
        {
            WaveNumber = 6,
            Groups = new[] { new SpawnGroup { Enemy = enemy, Count = 1 } },
        };
        var spike = new WaveDefinition
        {
            WaveNumber = 7,
            Groups = new[] { new SpawnGroup { Enemy = enemy, Count = 10 } },
        };
        var report = WavePacingAnalyzer.Analyze(spike, previous, 6);
        Require(report.ThreatValue > ThreatValueCalculator.Calculate(previous) * 1.5f, "Threat Value detects a spike");
        Require(report.Warnings.Exists(w => w.StartsWith("Spike:")), "Pacing report includes spike warning");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }
}
