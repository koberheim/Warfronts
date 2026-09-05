using System;
using System.Linq;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.Towers;
using FrontsOfWar.Meta;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Tests;

public class RuntimeMapIntegrationTests : TestClass
{
    public RuntimeMapIntegrationTests(Node scene) : base(scene) { }

    [Test]
    public void CampaignBocageHasBothEntriesAndTwentyTwoPads()
    {
        var mission = GD.Load<MissionDefinition>("res://assets/data/missions/m01_bocage_crossroads.tres");
        var map = MissionMapResolver.Load(mission);
        MissionMapResolver.ValidateWavePaths(mission, map);
        Require(map.TowerNodes.Length == 22 && map.Paths.Length == 2, "GDD Bocage topology");
        Require(map.Paths[0].Points[^1] == map.Paths[1].Points[^1], "shared objective");
        Require(mission.WaveSequence.Waves.SelectMany(wave => wave.Groups).Select(group => group.PathId).Distinct().Count() == 2,
            "both entry routes are exercised by campaign waves");
        Require(map.TowerNodes.Any(pad => pad.Tag == PadTag.Enclosed), "hedgerow trade-off present");
    }

    [Test]
    public void UnknownWaveRouteFailsBeforeMissionStarts()
    {
        var original = GD.Load<MissionDefinition>("res://assets/data/missions/m01_bocage_crossroads.tres");
        var map = MissionMapResolver.Load(original);
        var mission = new MissionDefinition { WaveSequence = new WaveSequence { Waves = new[]
        {
            new WaveDefinition { Groups = new[] { new SpawnGroup
            {
                Enemy = original.WaveSequence.Waves[0].Groups[0].Enemy, PathId = "missing_route",
            } } },
        } } };
        bool refused = false;
        try { MissionMapResolver.ValidateWavePaths(mission, map); }
        catch (InvalidOperationException) { refused = true; }
        Require(refused, "bad route fails before the simulation can enter an exception loop");
    }

    [Test]
    public void PlayerMissionStartsEmptyWithSelectedDifficultyAndBuildTime()
    {
        var previous = MissionSession.SelectedDifficulty;
        MissionSession.SelectedDifficulty = Difficulty.Elite;
        var mission = GD.Load<PackedScene>(Boot.MissionScene).Instantiate<MapRuntime>();
        // Headless integration exercises runtime without presentation deferred callbacks.
        foreach (var child in mission.GetChildren().Where(child => child is CanvasLayer || child.Name == "DamageNumberSpawner").ToArray())
        {
            mission.RemoveChild(child); child.Free();
        }
        try
        {
            TestScene.AddChild(mission);
            Require(mission.AuthoringMap?.Metadata.Id == "bocage_crossroads", "normal flow resolves mission map");
            Require(mission.GetChildren().OfType<BuildPad>().Count() == 22, "normal flow installs all pads");
            Require(mission.Towers.Towers.Count == 0 && mission.CommandPosts.Posts.Count == 0, "no free prototype defense");
            Require(mission.Enemies.Enemies.Count == 0 && mission.IsBuildPhase, "no surprise opening wave");
            Require(mission.BuildTimeRemaining == GameBalanceConfigAutoload.Config.BuildTimeElite, "selected difficulty controls opening time");
            Require(mission.Supply.Balance == GameBalanceConfigAutoload.Config.StartingSupplyElite, "selected difficulty controls supply");
            int balance = mission.Supply.Balance;
            int bonus = mission.EarlyCallBonusNow;
            mission.CallNextWaveEarly();
            Require(mission.Supply.Balance == balance + bonus && mission.Waves.CurrentWaveNumber == 1, "early call pays exact displayed bonus and starts wave one");
            mission.SimTick(1f);
            Require(mission.Enemies.Enemies.Count > 0, "campaign spawns on an installed authored route");
        }
        finally { mission.Free(); MissionSession.SelectedDifficulty = previous; }
    }

    [Test]
    public void AuthoredLayoutReplacesOldPadsAndPathsAndUsesAuthoredAir()
    {
        var root = new Node2D { Position = new Vector2(80, 40), Rotation = 0.2f };
        TestScene.AddChild(root);
        try
        {
            var oldPath = new PathNetwork { Name = "OldPath" };
            var curve = new Curve2D(); curve.AddPoint(Vector2.Zero); curve.AddPoint(Vector2.One * 64);
            oldPath.AddChild(new Path2D { Name = "Route", Curve = curve });
            root.AddChild(oldPath);
            root.AddChild(GD.Load<PackedScene>("res://scenes/map/build_pad.tscn").Instantiate<BuildPad>());
            var map = MapLoader.Load("editor_smoke_fixture");
            var runtime = MapRuntimeDataFactory.Build(map);
            var installedPath = MapRuntimeAuthoringBuilder.InstallIntoMission(root, map, runtime, new NodePath("OldPath"), root);
            Require(root.GetChildren().OfType<BuildPad>().Count() == 2, "only authored pads remain");
            Require(root.GetChildren().OfType<PathNetwork>().Count() == 1, "only authored routes remain");
            var network = root.GetNode<PathNetwork>(installedPath);
            Require(network.GetPositionAtProgress(0).IsEqualApprox(root.ToGlobal(new Vector2(64, 384))), "path honors mission transform");
            var air = MapRuntimeAuthoringBuilder.BuildAirCorridor(runtime, 64);
            Require(air.EntryPosition == new Vector2(0, 64) && air.ObjectivePosition == new Vector2(1216, 320), "air uses authored endpoints");
        }
        finally { root.Free(); }
    }

    [Test]
    public void ProductionValidationRejectsEmptyMapAndWrongEndpointKinds()
    {
        Require(!MapProductionValidator.Validate(MapDefinition.CreateNew("empty", "Empty", 8, 6)).CanPublish, "empty draft cannot publish");
        var map = MapLoader.Load("editor_smoke_fixture");
        map.Markers.First(marker => marker.Kind == GameplayMarkerKind.GroundEntry).Kind = GameplayMarkerKind.Objective;
        Require(!MapProductionValidator.Validate(map).CanPublish, "wrong entry kind is rejected");
    }

    [Test]
    public void RuntimeSnapshotCannotMutateTheDocumentAndClosedRoutesAreRefused()
    {
        var map = MapLoader.Load("editor_smoke_fixture");
        var data = MapRuntimeDataFactory.Build(map);
        data.Paths[0].Points[0] = Vector2.Zero;
        Require(map.Paths[0].BakedRuntimePoints[0] != Vector2.Zero, "snapshot geometry is copied");
        var network = new PathNetwork { PathId = "gate", ActiveFromWave = 10 };
        try
        {
            var set = new PathNetworkSet(); set.Add(network);
            bool refused = false;
            try { set.ResolveForWave("gate", 9, network); } catch (InvalidOperationException) { refused = true; }
            Require(refused, "closed gate cannot receive a spawn");
            Require(set.ResolveForWave("gate", 10, network) == network, "gate opens on authored wave");
        }
        finally { network.Free(); }
    }

    [Test]
    public void RestrictedPadRefusesWrongArchetypeBeforeSpendingSupply()
    {
        var root = new Node2D(); TestScene.AddChild(root);
        var supply = new FrontsOfWar.Economy.SupplyLedger(Difficulty.Regular, new GameBalanceConfig());
        try
        {
            var pad = new BuildPad { AllowedArchetypeIds = new[] { "AntiTankGun" } };
            // No tree is needed to test placement refusal; no scene or spend should occur.
            var definition = GD.Load<TowerDefinition>("res://assets/data/towers/t1_automatic_gun.tres");
            var service = new TowerPlacementService(root, root, supply, new TowerManager(), new CommandPostManager());
            int before = supply.Balance;
            Require(service.TryPlace(definition, pad).Result == TowerPlacementResult.ArchetypeNotAllowed, "restriction enforced");
            Require(supply.Balance == before, "refused placement spends nothing");
            pad.Free();
        }
        finally { supply.Dispose(); root.Free(); }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
