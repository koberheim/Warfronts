using System;
using System.Linq;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Map.Planning;

namespace FrontsOfWar.Tests;

public class MapPlanningTests : TestClass
{
    public MapPlanningTests(Node testScene) : base(testScene) { }

    [Test]
    public void CatalogLoadsOneHundredUniqueRecords()
    {
        var catalog = MapLayoutCatalog.LoadFromProject();
        Require(catalog.Records.Count == 100, "100 catalog records load");
        Require(catalog.Records.Select(record => record.Id).Distinct().Count() == 100, "catalog IDs are unique");
        Require(catalog.Records.All(record => record.Topology.Objectives == 1), "each catalog entry has one objective");
        Require(catalog.Records.All(record => record.Topology.GroundEntries is >= 1 and <= 3), "entries are within planner limits");
    }

    [Test]
    public void CandidateGenerationIsDeterministic()
    {
        var template = MapLayoutCatalog.LoadFromProject().Filter("SERPENTINE").First();
        var first = MapCandidateGenerator.GenerateSingle(template, 4421UL);
        var second = MapCandidateGenerator.GenerateSingle(template, 4421UL);
        var different = MapCandidateGenerator.GenerateSingle(template, 4422UL);
        Require(MapPlanSerializer.SaveToJson(first) == MapPlanSerializer.SaveToJson(second), "same seed produces identical plan");
        Require(MapPlanSerializer.SaveToJson(first) != MapPlanSerializer.SaveToJson(different), "different seed changes candidate");
    }

    [Test]
    public void ThousandCandidatesAcrossAllFamiliesValidate()
    {
        var catalog = MapLayoutCatalog.LoadFromProject();
        foreach (var family in catalog.Records.Select(record => record.Family).Distinct())
        {
            var template = catalog.Filter(family).First();
            for (int i = 0; i < 100; i++)
            {
                var plan = MapCandidateGenerator.GenerateSingle(template, (ulong)(10000 + i));
                Require(plan.Validation.IsValid, $"{family} candidate {i} validates: {string.Join("; ", plan.Validation.Errors)}");
                Require(plan.Pads.Count is >= 18 and <= 34, $"{family} candidate {i} pad count");
            }
        }
    }

    [Test]
    public void IntentionalCrossingIsAllowedButAccidentalCrossingIsRejected()
    {
        var plan = new MapPlanDefinition
        {
            Entries = new() { new(5, 5), new(5, 50) }, Objective = new(94, 28),
            Paths = new()
            {
                new() { Id = "path_0", StartEntryId = "entry_0", IntentionalCrossing = true, Points = new() { new(5, 5), new(55, 50), new(94, 28) } },
                new() { Id = "path_1", StartEntryId = "entry_1", IntentionalCrossing = true, Points = new() { new(5, 50), new(55, 5), new(94, 28) } },
            },
        };
        Require(MapPlanValidator.Validate(plan).IsValid, "intentional crossing validates");
        plan.Paths[0].IntentionalCrossing = false;
        plan.Paths[1].IntentionalCrossing = false;
        Require(!MapPlanValidator.Validate(plan).IsValid, "unflagged crossing is rejected");
    }

    [Test]
    public void AdvancedFamiliesKeepBranchConsistencyAndDiversity()
    {
        var catalog = MapLayoutCatalog.LoadFromProject();
        foreach (var family in new[] { "MERGE", "SPLIT_MERGE", "DUAL_LANE", "CROSSING", "HUB", "ASYMMETRIC" })
        {
            var template = catalog.Filter(family).First();
            var candidates = MapCandidateGenerator.Generate(template, 7000UL, 12);
            Require(candidates.Count > 1, $"{family} produces diverse candidates");
            Require(candidates.All(candidate => candidate.Validation.IsValid), $"{family} candidates validate");
        }
    }

    [Test]
    public void ScoreAndSaveLoadAreStable()
    {
        var template = MapLayoutCatalog.LoadFromProject().Filter("HAIRPIN").First();
        var plan = MapCandidateGenerator.GenerateSingle(template, 8100UL);
        float firstScore = MapPlanScorer.Score(plan, template).Total;
        float secondScore = MapPlanScorer.Score(plan, template).Total;
        var roundTrip = MapPlanSerializer.LoadFromJson(MapPlanSerializer.SaveToJson(plan));
        RequireApproximately(firstScore, secondScore, "score is stable");
        Require(roundTrip.Id == plan.Id && roundTrip.Paths.Count == plan.Paths.Count && roundTrip.Pads.Count == plan.Pads.Count,
            "plan save/load preserves structure");
        RequireApproximately(plan.Paths[0].Points[2].X, roundTrip.Paths[0].Points[2].X, "plan save/load preserves geometry");
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
