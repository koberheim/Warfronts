using System;
using System.Collections.Generic;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;

namespace FrontsOfWar.Tests;

// R10 (docs/RELEASE_COMPLETION.md): B2 Convoy, B3 Formation, B4 multi-phase,
// and Elite Medium Armor's Frontal Plate (GDD §10.3) - see docs/DECISIONS.md
// D83 for what this deliberately simplifies (B4's single add-archetype and
// the phase-3 air escort, neither of which any prior boss needed).
public class BossTests : TestClass
{
    public BossTests(Node testScene) : base(testScene) { }

    [Test]
    public void ConvoyCommanderProjectsResistanceAndSuppressionImmunityWithinItsAura()
    {
        var path = MakePath();
        var commander = MakeEnemy(path, "res://assets/data/enemies/e_b2_armored_column_command.tres");
        var nearEscort = MakeEnemy(path, "res://assets/data/enemies/e6_medium_armor.tres");
        var farEscort = MakeEnemy(path, "res://assets/data/enemies/e6_medium_armor.tres");
        var roster = new List<EnemyController> { commander, nearEscort, farEscort };
        foreach (var e in roster) e.SetEnemyProvider(() => roster);

        nearEscort.GlobalPosition = commander.GlobalPosition + new Vector2(50f, 0f); // inside a 3-tile (192px) aura
        farEscort.GlobalPosition = commander.GlobalPosition + new Vector2(500f, 0f); // well outside it

        float nearBefore = nearEscort.CurrentHp;
        float farBefore = farEscort.CurrentHp;
        nearEscort.ApplyDamage(100f, DamageType.SmallArms);
        farEscort.ApplyDamage(100f, DamageType.SmallArms);

        Require(nearBefore - nearEscort.CurrentHp < farBefore - farEscort.CurrentHp,
            "the escort inside the Convoy aura takes less damage than the identical hit outside it");

        farEscort.ApplySuppressed(4f, 4f);
        Require(farEscort.Status.IsSuppressed, "outside the aura, Suppression applies normally");
        nearEscort.ApplySuppressed(4f, 4f);
        Require(!nearEscort.Status.IsSuppressed, "the Convoy aura grants Suppression immunity");
    }

    [Test]
    public void KillingTheConvoyCommanderCollapsesNearbyEscortsToHalfHealth()
    {
        var path = MakePath();
        var commander = MakeEnemy(path, "res://assets/data/enemies/e_b2_armored_column_command.tres");
        var escort = MakeEnemy(path, "res://assets/data/enemies/e6_medium_armor.tres");
        var roster = new List<EnemyController> { commander, escort };
        foreach (var e in roster) e.SetEnemyProvider(() => roster);
        escort.GlobalPosition = commander.GlobalPosition + new Vector2(50f, 0f);

        Require(escort.CurrentHp == escort.MaxHp, "the escort starts at full health");
        commander.CapHealth(0f); // kills the commander directly, bypassing resistance/shields

        Require(escort.CurrentHp <= escort.MaxHp * 0.5f + 0.01f, "the escort collapses to (at most) 50% HP the instant the commander dies");
    }

    [Test]
    public void FormationBonusHoldsOnlyAtFullStrengthAndEachLossSlowsTheSurvivors()
    {
        var path = MakePath();
        var a = MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres");
        var b = MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres");
        var c = MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres");
        var roster = new List<EnemyController> { a, b, c };
        foreach (var e in roster) e.SetEnemyProvider(() => roster);

        float fullStrengthHit = 100f;
        a.ApplyDamage(fullStrengthHit, DamageType.AntiAir);
        float damageAtFullStrength = fullStrengthHit == 0 ? 0 : a.MaxHp - a.CurrentHp;

        c.CapHealth(0f); // one bomber down - formation is now broken

        float beforeSecondHit = b.CurrentHp;
        b.ApplyDamage(fullStrengthHit, DamageType.AntiAir);
        float damageBroken = beforeSecondHit - b.CurrentHp;

        Require(damageBroken > damageAtFullStrength, "losing a wingman removes the shared 30% damage reduction");

        // A survivor's own speed multiplier scales with how many wingmen are gone.
        var provider = typeof(EnemyController).GetMethod("FormationState",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var (damageMultiplier, speedMultiplier) = ((float, float))provider.Invoke(b, Array.Empty<object>());
        Require(damageMultiplier == 1f, "no reduction once the formation is broken");
        RequireApproximately(0.8f, speedMultiplier, "one lost wingman applies exactly one 20% speed penalty stack");
    }

    [Test]
    public void FormationBreakSlowsALiveBomberInFlightNotJustTheRawCalculation()
    {
        var path = MakePath();
        var pair = new List<EnemyController>
        {
            MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres"),
            MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres"),
        };
        // Only 2 of the authored FormationSize (3) are alive - also broken,
        // to the same degree (1 lost member) as `lone` above.
        foreach (var e in pair) e.SetEnemyProvider(() => pair);

        var full = new List<EnemyController>
        {
            MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres"),
            MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres"),
            MakeEnemy(path, "res://assets/data/enemies/e_b3_bomber_wing.tres"),
        };
        foreach (var e in full) e.SetEnemyProvider(() => full);

        pair[0].SimTick(1f);
        full[0].SimTick(1f);

        Require(pair[0].PathProgress < full[0].PathProgress,
            "a two-of-three (broken) formation flies slower over the same real tick than a full three-of-three formation");
    }

    [Test]
    public void MultiPhaseBossAdvancesOnceAtEachThresholdAndHaltsForATelegraph()
    {
        var definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e_b4_fortress_assault_group.tres");
        var boss = new MultiPhaseBossController(definition);

        boss.UpdatePhase(currentHp: definition.BaseHp, maxHp: definition.BaseHp);
        Require(boss.PhaseIndex == 0, "starts in phase 0 at full health");

        boss.UpdatePhase(currentHp: definition.BaseHp * 0.59f, maxHp: definition.BaseHp);
        Require(boss.PhaseIndex == 1 && boss.IsHalted, "crossing 60% HP advances to phase 1 (Siege) and halts for the telegraph");

        boss.Tick(3f); // exactly the telegraph window
        Require(!boss.IsHalted, "the halt lifts after its 3-second telegraph");

        boss.UpdatePhase(currentHp: definition.BaseHp * 0.7f, maxHp: definition.BaseHp); // healed back above the phase-1 threshold
        Require(boss.PhaseIndex == 1, "phases are one-way - healing does not revert a transition");

        boss.UpdatePhase(currentHp: definition.BaseHp * 0.24f, maxHp: definition.BaseHp);
        Require(boss.PhaseIndex == 2 && boss.IsHalted, "crossing 25% HP advances to phase 2 and halts again");
        Require(boss.IsSuppressionImmune, "phase 2 (index 2) is Suppression immune");
        Require(boss.SpeedMultiplier == definition.Phase3SpeedMultiplier, "phase 2 applies the authored speed multiplier");
    }

    [Test]
    public void MultiPhaseBossOnlyBombardsDuringItsSiegePhaseAndOnTheAuthoredInterval()
    {
        var definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e_b4_fortress_assault_group.tres");
        var boss = new MultiPhaseBossController(definition);

        Require(!boss.ConsumeBombardReady(out _, out _), "phase 0 never bombards");

        boss.UpdatePhase(definition.BaseHp * 0.5f, definition.BaseHp); // -> phase 1 (Siege), 3s halt begins
        boss.Tick(3f); // clears the halt
        Require(!boss.ConsumeBombardReady(out _, out _), "not ready before the authored interval elapses");

        boss.Tick(definition.SiegeBombardIntervalSeconds);
        Require(boss.ConsumeBombardReady(out float range, out float duration), "ready once the interval elapses in the Siege phase");
        Require(range == definition.SiegeBombardRangeTiles && duration == definition.SiegeSuppressionDurationSeconds,
            "reuses the Siege archetype's own authored range/duration, not new fields");
    }

    [Test]
    public void FrontalPlateOnlyReducesDamageFromAheadAndOnlyOnceDamaged()
    {
        var path = MakePath();
        var enemy = MakeEnemy(path, "res://assets/data/enemies/e6_medium_armor_elite.tres");
        var source = new StubDamageSource();
        enemy.SimTick(0.05f); // establishes a real +X heading (Velocity) - a stationary enemy gets no Frontal Plate

        source.GlobalPosition = enemy.GlobalPosition + new Vector2(60f, 0f); // ahead, along the heading
        enemy.ApplyDamage(50f, DamageType.SmallArms, source); // still above the 35% activation threshold - full damage
        float aboveThresholdDamage = enemy.MaxHp - enemy.CurrentHp;

        enemy.CapHealth(enemy.MaxHp * 0.3f); // now below the 35% activation threshold
        enemy.SimTick(0.05f);
        float beforeSecondHit = enemy.CurrentHp;
        source.GlobalPosition = enemy.GlobalPosition + new Vector2(60f, 0f);
        enemy.ApplyDamage(50f, DamageType.SmallArms, source);
        float belowThresholdFrontDamage = beforeSecondHit - enemy.CurrentHp;

        Require(belowThresholdFrontDamage < 50f && belowThresholdFrontDamage < aboveThresholdDamage,
            "once damaged past the activation threshold, a frontal hit is reduced");

        enemy.SimTick(0.05f);
        source.GlobalPosition = enemy.GlobalPosition + new Vector2(-60f, 0f); // now behind the heading
        float beforeThirdHit = enemy.CurrentHp;
        enemy.ApplyDamage(50f, DamageType.SmallArms, source);
        float belowThresholdRearDamage = beforeThirdHit - enemy.CurrentHp;

        Require(belowThresholdRearDamage > belowThresholdFrontDamage, "a hit from behind takes full damage even below the activation threshold");
    }

    [Test]
    public void EliteVariantsMatchTheirGddOverrides()
    {
        var baseline = GD.Load<EnemyDefinition>("res://assets/data/enemies/e6_medium_armor.tres");
        var elite = GD.Load<EnemyDefinition>("res://assets/data/enemies/e6_medium_armor_elite.tres");
        Require(elite.BaseHp == baseline.BaseHp * 1.5f, "Elite Medium Armor has +50% HP");
        Require(elite.HasFrontalPlate && elite.FrontalPlateActivateHpFraction == 0.35f, "Elite Medium Armor gains Frontal Plate at 35% HP");

        var eliteSiege = GD.Load<EnemyDefinition>("res://assets/data/enemies/e12_siege_artillery_elite.tres");
        Require(eliteSiege.SiegeBombardRangeTiles == 13f, "Elite Siege has 13-tile range");
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

    private EnemyController MakeEnemy(PathNetwork path, string resourcePath)
    {
        var enemy = new EnemyController { Definition = GD.Load<EnemyDefinition>(resourcePath) };
        TestScene.AddChild(enemy);
        enemy.Initialize(path);
        return enemy;
    }

    private sealed class StubDamageSource : IDamageSource
    {
        public string SourceId => "stub";
        public Vector2 GlobalPosition { get; set; }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }

    private static void RequireApproximately(float expected, float actual, string message)
    {
        if (MathF.Abs(expected - actual) > 0.001f)
            throw new InvalidOperationException($"Assertion failed: {message}; expected {expected}, got {actual}");
    }
}
