using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Tests;

// Selection flow is data-driven: these tests protect the boundary between
// campaign-facing resources and nearby authoring assets such as waves.
public class CampaignSelectionTests : TestClass
{
    public CampaignSelectionTests(Node testScene) : base(testScene) { }

    [Test]
    public void CatalogGroupsAllSixAuthoredNationsByAlliance()
    {
        var nations = MissionCatalog.DiscoverNations();

        Require(nations.Count == 6, "all six NationProfile resources are discoverable");
        Require(MissionCatalog.NationsForAlliance("Allies").Count == 3, "three Allied nations are selectable");
        Require(MissionCatalog.NationsForAlliance("Axis").Count == 3, "three Axis nations are selectable");
        Require(nations.All(entry => !string.IsNullOrWhiteSpace(entry.Resource.SignatureId)),
            "each selected nation declares a signature id");
    }

    [Test]
    public void CatalogIncludesMissionsButExcludesWaveAssets()
    {
        var missions = MissionCatalog.DiscoverMissions();

        Require(missions.Any(entry => entry.Resource.Id == "m01_bocage_crossroads"),
            "the authored MissionDefinition is selectable");
        Require(!missions.Any(entry => entry.Path.EndsWith("m2_wave_sequence.tres", StringComparison.Ordinal)
            || entry.Path.EndsWith("test_wave_01.tres", StringComparison.Ordinal)),
            "WaveSequence assets beside missions never become campaign entries");
    }

    [Test]
    public void CampaignMissionRequiresItsPrecedingCompletion()
    {
        var first = new MissionCatalog.Entry<MissionDefinition>("res://test/m01.tres", new MissionDefinition { Id = "m01", Act = 1 });
        var second = new MissionCatalog.Entry<MissionDefinition>("res://test/m02.tres", new MissionDefinition { Id = "m02", Act = 1 });
        IReadOnlyList<MissionCatalog.Entry<MissionDefinition>> ordered = new[] { first, second };
        var profile = new PlayerProfile();

        Require(MissionCatalog.IsCampaignMissionUnlocked(profile, first, ordered), "the first mission starts unlocked");
        Require(!MissionCatalog.IsCampaignMissionUnlocked(profile, second, ordered), "the next mission starts locked");
        profile.CampaignMissionsCompleted.Add("m01");
        Require(MissionCatalog.IsCampaignMissionUnlocked(profile, second, ordered), "the next mission unlocks after its predecessor");
    }

    [Test]
    public void SignatureConsumesOneOfTheSixGenericBuildSlots()
    {
        var paths = MissionCatalog.DiscoverNations()
            .Select(nation => MissionCatalog.ResolveSignatureResourcePath(nation.Resource.SignatureId))
            .ToList();

        Require(paths.Count == 6 && paths.All(path => !string.IsNullOrEmpty(path)),
            "every nation resolves a signature resource path");
        Require(paths.All(path => ResourceLoader.Load(path) is SignatureDefinition or ArsenalDefinition),
            "signature resources may be a SignatureDefinition or ArsenalDefinition");

        var originalSlots = MissionSession.BuildSlotResourcePaths;
        try
        {
            MissionSession.BuildSlotResourcePaths = MissionCatalog.DiscoverBuildTowers().Take(5)
                .Select(entry => entry.Path).Append(paths[0]).ToList();
            Require(MissionSession.BuildSlotResourcePaths.Count == 6, "build selection has exactly six ordered slots");
            Require(MissionSession.BuildSlotResourcePaths.Contains(paths[0]),
                "the signature occupies a real build slot rather than a seventh field");
            Require(MissionSession.BuildSlotResourcePaths.Count(path => ResourceLoader.Load(path) is TowerDefinition) == 5,
                "the signature replaces one ordinary tower choice");
            Require(MissionSession.BuildSlotResourcePaths.Count(path => ResourceLoader.Load(path) is SignatureDefinition or ArsenalDefinition) == 1,
                "at most one generic signature resource occupies the six slots");
        }
        finally
        {
            MissionSession.BuildSlotResourcePaths = originalSlots;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }
}
