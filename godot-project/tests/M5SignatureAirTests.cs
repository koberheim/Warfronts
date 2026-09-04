using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Tests;

public class M5SignatureAirTests : TestClass
{
    public M5SignatureAirTests(Node testScene) : base(testScene) { }

    [Test]
    public void M5SignatureResourcesMatchTheGdd()
    {
        var raf = LoadSignature("signature_raf_scramble_command");
        Require(raf.NationId == "britain" && raf.ChargeCaps[0] == 2 && raf.ChargeRegenSeconds[2] == 14f, "RAF charge data");
        Require(raf.RafCorridorWidthTiles == 3f && raf.RafCorridorLengthTiles == 8f && raf.RafPassCount == 3, "RAF corridor data");
        var katyusha = LoadSignature("signature_katyusha_storm_battery");
        Require(katyusha.KatyushaFullCharge[0] == 240f && katyusha.KatyushaRocketCounts[2] == 36, "Katyusha charge and salvo data");
        var blitz = LoadSignature("signature_blitzkrieg_command_post");
        Require(blitz.BlitzRateOfFireMultiplier == 1.45f && blitz.BlitzProjectileVelocityMultiplier == 1.6f, "Blitzkrieg active modifiers");
        var bersaglieri = LoadSignature("signature_bersaglieri_charge_post");
        Require(bersaglieri.BersaglieriDeploymentIntervals[0] == 18f && bersaglieri.BersaglieriSquadSizes[1] == 5, "Bersaglieri deployment data");
        var airfield = LoadSignature("signature_special_attack_airfield");
        Require(airfield.AirfieldStrikeDamage == 420f && airfield.AirfieldStoredChargeCaps[2] == 3, "Airfield strike data");
    }

    [Test]
    public void RafScrambleUsesThreePassCorridorAndAirIntercept()
    {
        var path = CreatePath();
        var ground = new FakeTargetable(new Vector2(320f, 0f), false);
        var raf = new RafScrambleController { Definition = LoadSignature("signature_raf_scramble_command") };
        TestScene.AddChild(raf);
        raf.Initialize(path, () => new List<ITargetable> { ground });
        Require(raf.TryActivateAtPoint(new Vector2(320f, 0f)), "RAF spends a charge");
        raf.SimTick(4f);
        Require(ground.TotalDamage > 224f && ground.TotalDamage < 226f, "RAF applies three SA+HE passes");
        Require(raf.Charges == 1, "RAF charge is finite");

        var air = new FakeTargetable(new Vector2(320f, 0f), true);
        var airRaf = new RafScrambleController { Definition = LoadSignature("signature_raf_scramble_command") };
        TestScene.AddChild(airRaf);
        airRaf.Initialize(path, () => new List<ITargetable> { air });
        Require(airRaf.TryActivateAtPoint(new Vector2(320f, 0f)), "RAF air sortie activates");
        airRaf.SimTick(4f);
        Require(air.AntiAirHits == 3 && air.TotalDamage == 600f, "RAF air sortie intercepts three passes");
    }

    [Test]
    public void RemainingSignaturesExposeTheirNamedActivationRules()
    {
        var path = CreatePath();
        var target = new FakeTargetable(new Vector2(320f, 0f), false);
        var katyusha = new KatyushaStormController { Definition = LoadSignature("signature_katyusha_storm_battery") };
        TestScene.AddChild(katyusha);
        katyusha.Initialize(path, () => new List<ITargetable> { target });
        katyusha.SimTick(240f);
        Require(katyusha.ChargePoints == 240f && katyusha.TryRelease(), "Katyusha reaches and releases full charge");
        katyusha.SimTick(3.5f);
        Require(!katyusha.IsBarrageActive, "Katyusha barrage completes");

        var bersaglieri = new BersaglieriChargeController { Definition = LoadSignature("signature_bersaglieri_charge_post") };
        TestScene.AddChild(bersaglieri);
        bersaglieri.Initialize(path, () => new List<ITargetable> { target });
        Require(bersaglieri.TryDeploy() && bersaglieri.LivingUnitCount == 4, "Bersaglieri deploys a four-unit burst");

        var airfield = new SpecialAttackAirfieldController { Definition = LoadSignature("signature_special_attack_airfield") };
        TestScene.AddChild(airfield);
        airfield.Initialize(path, () => new List<ITargetable> { target });
        Require(airfield.TryActivateAtPoint(new Vector2(320f, 0f)), "Airfield spends a stored strike");
        airfield.SimTick(2f);
        Require(target.TotalDamage > 0f, "Airfield resolves its delayed strike");
    }

    [Test]
    public void AirCorridorSupportAndMinefieldRulesWorkTogether()
    {
        var path = CreatePath();
        var corridor = new AirCorridorDefinition { EntryPosition = new Vector2(0f, 100f), ObjectivePosition = new Vector2(640f, 100f) };
        var air = NewEnemy("e8_air_unit", path, corridor);
        air.SimTick(1f);
        Require(air.IsAir && air.GlobalPosition.Y == 100f && air.PathProgress > 0f && !air.ReachedEnd, "E8 uses the authored air corridor");

        var manager = new EnemyManager();
        var vehicle = NewEnemy("e6_medium_armor", path);
        var support = NewEnemy("e9_support_repair_vehicle", path);
        manager.Register(vehicle);
        manager.Register(support);
        vehicle.ApplyDamage(100f, DamageType.ArmorPiercing);
        float damaged = vehicle.CurrentHp;
        support.SimTick(1f);
        Require(vehicle.CurrentHp > damaged && support.RepairTarget == vehicle, "E9 repairs a damaged vehicle and draws a target");

        var escort = NewEnemy("e10_escort_shield_vehicle", path);
        var shielded = NewEnemy("e6_medium_armor", path);
        manager.Register(escort);
        manager.Register(shielded);
        float shieldedHp = shielded.CurrentHp;
        shielded.ApplyDamage(200f, DamageType.ArmorPiercing);
        Require(shielded.CurrentHp == shieldedHp && escort.ShieldRemaining < 400f, "E10 shield absorbs allied damage");

        // A separate manager keeps the earlier Escort's shield pool from
        // absorbing the blast; this step only proves the mine/concealment rule.
        var recon = NewEnemy("e11_recon_concealed", path);
        new EnemyManager().Register(recon);
        Require(recon.IsConcealed && !recon.IsRevealed, "E11 starts concealed to towers");
        var minefield = new MinefieldController { Definition = GD.Load<TowerDefinition>("res://assets/data/towers/t8_minefield.tres") };
        TestScene.AddChild(minefield);
        minefield.Initialize(() => new List<ITargetable> { recon });
        float reconHp = recon.CurrentHp;
        Require(minefield.TriggerNow() && recon.CurrentHp < reconHp, "Minefield triggers on concealed E11");
    }

    [Test]
    public void GroundTowersDoNotAcquireAirButFlakCan()
    {
        var t1 = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
        var flak = GD.Load<TowerDefinition>("res://assets/data/towers/t5_flak_battery.tres");
        Require(t1.Levels[0].TargetDomain == TargetDomain.Ground, "ground tower domain is ground");
        Require(flak.Levels[0].TargetDomain == TargetDomain.Air, "Flak primary domain is air");
        Require(flak.BranchA.Levels[1].TargetDomain == TargetDomain.GroundAndAir, "Flak dual-purpose branch can acquire air and ground");
    }

    private EnemyController NewEnemy(string id, PathNetwork path, AirCorridorDefinition corridor = null)
    {
        var enemy = new EnemyController { Definition = GD.Load<EnemyDefinition>($"res://assets/data/enemies/{id}.tres") };
        TestScene.AddChild(enemy);
        enemy.Initialize(path, 1f, corridor);
        return enemy;
    }

    private PathNetwork CreatePath()
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

    private static SignatureDefinition LoadSignature(string id)
        => GD.Load<SignatureDefinition>($"res://assets/data/towers/{id}.tres");

    private sealed class FakeTargetable : ITargetable
    {
        public Vector2 GlobalPosition { get; }
        public float PathProgress => 0.5f;
        public float CurrentHp { get; private set; } = 10000f;
        public bool IsAir { get; }
        public bool IsConcealed => false;
        public bool IsRevealed => true;
        public bool IsAlive => CurrentHp > 0f;
        public Vector2 Velocity => Vector2.Zero;
        public float TotalDamage { get; private set; }
        public int AntiAirHits { get; private set; }

        public FakeTargetable(Vector2 position, bool isAir) { GlobalPosition = position; IsAir = isAir; }
        public void ApplyDamage(float amount, DamageType type)
        {
            CurrentHp -= amount;
            TotalDamage += amount;
            if (type == DamageType.AntiAir) AntiAirHits++;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }
}
