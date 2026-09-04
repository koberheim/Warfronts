using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;
using FrontsOfWar.Platform;

namespace FrontsOfWar.Tests;

// GDD §19 prompt 41 (ProgressionService, UnlockService, MasteryService,
// stars, the versioned-JSON save system) and the Null half of prompt 43
// (IPlatformService). Builds real StarObjectiveDefinition/MissionDefinition/
// PlayerProfile instances rather than fakes — these are plain
// Resource/POCO data, so there's no scene-tree cost to using the real thing.
public class ProgressionTests : TestClass
{
    public ProgressionTests(Node testScene) : base(testScene) { }

    private static GameBalanceConfig Config() => new();

    // --- (a) stars from scripted stats -----------------------------------

    [Test]
    public void AllThreeStarsEarnedWhenObjectiveMetAndDefenseLineFull()
    {
        var objective = new StarObjectiveDefinition { Kind = StarObjectiveKind.MaxTowersBuilt, IntParameter = 8 };
        var stats = new MissionStatsSnapshot
            { Victory = true, TowersBuilt = 5, FinalIntegrity = 20, MaxIntegrity = 20, Difficulty = Difficulty.Regular };

        var stars = StarEvaluator.Evaluate(stats, objective, Config());

        Require(stars[0], "star 1 (completed) earned");
        Require(stars[1], "star 2 (>=75% Defense Line) earned");
        Require(stars[2], "star 3 (mission objective) earned");
    }

    [Test]
    public void DefenseLineAt74PercentLosesStarTwoOnly()
    {
        var objective = new StarObjectiveDefinition { Kind = StarObjectiveKind.MaxTowersBuilt, IntParameter = 8 };
        var stats = new MissionStatsSnapshot
            { Victory = true, TowersBuilt = 3, FinalIntegrity = 74, MaxIntegrity = 100, Difficulty = Difficulty.Regular };

        var stars = StarEvaluator.Evaluate(stats, objective, Config());

        Require(stars[0], "star 1 still earned");
        Require(!stars[1], "star 2 lost at 74% Defense Line");
        Require(stars[2], "star 3 still earned (objective unrelated to Defense Line)");
    }

    [Test]
    public void NineTowersLosesStarThreeOnly()
    {
        var objective = new StarObjectiveDefinition { Kind = StarObjectiveKind.MaxTowersBuilt, IntParameter = 8 };
        var stats = new MissionStatsSnapshot
            { Victory = true, TowersBuilt = 9, FinalIntegrity = 20, MaxIntegrity = 20, Difficulty = Difficulty.Regular };

        var stars = StarEvaluator.Evaluate(stats, objective, Config());

        Require(stars[0], "star 1 still earned");
        Require(stars[1], "star 2 still earned (Defense Line unrelated to tower count)");
        Require(!stars[2], "star 3 lost building a 9th tower against an 8-tower cap");
    }

    [Test]
    public void DefeatEarnsNoStars()
    {
        var objective = new StarObjectiveDefinition { Kind = StarObjectiveKind.MaxTowersBuilt, IntParameter = 8 };
        var stats = new MissionStatsSnapshot { Victory = false, TowersBuilt = 2, FinalIntegrity = 0, MaxIntegrity = 20 };

        var stars = StarEvaluator.Evaluate(stats, objective, Config());

        Require(!stars[0] && !stars[1] && !stars[2], "a defeat earns zero stars");
    }

    // --- (b) unlock gates --------------------------------------------------

    private static PlayerProfile ProfileWithNationCompletions(int count)
    {
        var profile = new PlayerProfile();
        var record = new MissionRecord();
        record.CompletionsByNation["united_states"] = count;
        profile.MissionResults["m01_bocage_crossroads"] = record;
        return profile;
    }

    [Test]
    public void SignatureUnlocksAtExactlyOneNationCompletion()
    {
        Require(!UnlockService.IsSignatureUnlocked(ProfileWithNationCompletions(0), "united_states"), "locked at 0 completions");
        Require(UnlockService.IsSignatureUnlocked(ProfileWithNationCompletions(1), "united_states"), "unlocked at 1 completion");
    }

    [Test]
    public void DoctrineTwoUnlocksAtExactlyTwoNationCompletions()
    {
        Require(!UnlockService.IsDoctrineUnlocked(ProfileWithNationCompletions(1), "united_states", 1), "locked at 1 completion");
        Require(UnlockService.IsDoctrineUnlocked(ProfileWithNationCompletions(2), "united_states", 1), "unlocked at 2 completions");
    }

    [Test]
    public void DoctrineThreeUnlocksAtExactlyFourNationCompletions()
    {
        Require(!UnlockService.IsDoctrineUnlocked(ProfileWithNationCompletions(3), "united_states", 2), "locked at 3 completions");
        Require(UnlockService.IsDoctrineUnlocked(ProfileWithNationCompletions(4), "united_states", 2), "unlocked at 4 completions");
    }

    [Test]
    public void SkirmishUnlocksAtExactlyThreeCampaignMissions()
    {
        var locked = new PlayerProfile();
        locked.CampaignMissionsCompleted.UnionWith(new[] { "m01", "m02" });
        var unlocked = new PlayerProfile();
        unlocked.CampaignMissionsCompleted.UnionWith(new[] { "m01", "m02", "m03" });

        Require(!UnlockService.IsSkirmishUnlocked(locked), "locked at 2 campaign missions");
        Require(UnlockService.IsSkirmishUnlocked(unlocked), "unlocked at 3 campaign missions");
    }

    [Test]
    public void EndlessUnlocksAtExactlyEightCampaignMissions()
    {
        var locked = new PlayerProfile();
        for (int i = 1; i <= 7; i++) locked.CampaignMissionsCompleted.Add($"m{i:00}");
        var unlocked = new PlayerProfile();
        for (int i = 1; i <= 8; i++) unlocked.CampaignMissionsCompleted.Add($"m{i:00}");

        Require(!UnlockService.IsEndlessUnlocked(locked), "locked at 7 campaign missions");
        Require(UnlockService.IsEndlessUnlocked(unlocked), "unlocked at 8 campaign missions");
    }

    [Test]
    public void VeteranUnlocksAfterAnyRegularWinAndEliteAfterAnyVeteranWin()
    {
        var none = new PlayerProfile();
        var regular = new PlayerProfile { BestDifficultyCompleted = Difficulty.Regular };
        var veteran = new PlayerProfile { BestDifficultyCompleted = Difficulty.Veteran };

        Require(!UnlockService.IsDifficultyUnlocked(none, Difficulty.Veteran), "Veteran locked with no completions");
        Require(UnlockService.IsDifficultyUnlocked(regular, Difficulty.Veteran), "Veteran unlocked after a Regular win");
        Require(!UnlockService.IsDifficultyUnlocked(regular, Difficulty.Elite), "Elite still locked after only a Regular win");
        Require(UnlockService.IsDifficultyUnlocked(veteran, Difficulty.Elite), "Elite unlocked after a Veteran win");
    }

    // --- (c) mastery rank thresholds and cosmetic-only reward tiers ------

    [Test]
    public void MasteryRankThresholdsMatchConfig()
    {
        var config = Config();
        Require(MasteryService.RankFor(0f, config) == 1, "0 XP is rank 1");
        Require(MasteryService.RankFor(149f, config) == 1, "149 XP is still rank 1");
        Require(MasteryService.RankFor(150f, config) == 2, "150 XP reaches rank 2");
        Require(MasteryService.RankFor(4200f, config) == 10, "4200 XP reaches rank 10");
        Require(MasteryService.RankFor(999999f, config) == 10, "rank never exceeds 10");
    }

    [Test]
    public void RewardTiersAreCosmeticBandsOnly()
    {
        Require(MasteryService.RewardTierFor(1) == MasteryRewardTier.ColorScheme, "rank 1 is a color scheme");
        Require(MasteryService.RewardTierFor(3) == MasteryRewardTier.ColorScheme, "rank 3 is still a color scheme");
        Require(MasteryService.RewardTierFor(4) == MasteryRewardTier.Banner, "rank 4 is a banner");
        Require(MasteryService.RewardTierFor(6) == MasteryRewardTier.Banner, "rank 6 is still a banner");
        Require(MasteryService.RewardTierFor(7) == MasteryRewardTier.TableDressing, "rank 7 is table dressing");
        Require(MasteryService.RewardTierFor(9) == MasteryRewardTier.TableDressing, "rank 9 is still table dressing");
        Require(MasteryService.RewardTierFor(10) == MasteryRewardTier.Title, "rank 10 is the title/frame");
    }

    [Test]
    public void XpFormulaScalesByDifficultyAndExtraStars()
    {
        var config = Config();
        float baseXp = MasteryService.XpForCompletion(Difficulty.Regular, new[] { true, false, false }, config);
        float bothExtraXp = MasteryService.XpForCompletion(Difficulty.Regular, new[] { true, true, true }, config);
        float recruitXp = MasteryService.XpForCompletion(Difficulty.Recruit, new[] { true, false, false }, config);
        float defeatXp = MasteryService.XpForCompletion(Difficulty.Elite, new[] { false, false, false }, config);

        RequireApproximately(config.MasteryXpBase, baseXp, "Regular, 1 star: base XP with no bonus");
        RequireApproximately(config.MasteryXpBase * (1f + config.MasteryXpExtraStarBonus * 2), bothExtraXp,
            "Regular, 3 stars: two extra-star bonuses stack");
        RequireApproximately(config.MasteryXpBase * config.MasteryXpDifficultyMultiplierRecruit, recruitXp,
            "Recruit difficulty multiplier applies");
        Require(defeatXp == 0f, "a defeat (no star 1) earns zero mastery XP");
    }

    // --- (d) save round-trip to a scratch path under user:// -------------

    [Test]
    public void SaveThenLoadRoundTripsAllFields()
    {
        WithScratchSaveDirectory("roundtrip", () =>
        {
            var profile = new PlayerProfile { TutorialCompleted = true };
            profile.MasteryXp["united_states"] = 275.5;
            profile.UnlockedAchievements.Add(AchievementIds.FirstMissionWon);
            profile.CampaignMissionsCompleted.Add("m01_bocage_crossroads");
            profile.BestDifficultyCompleted = Difficulty.Regular;
            var record = new MissionRecord { BestDifficulty = Difficulty.Regular };
            record.BestStars[0] = true;
            record.BestStars[1] = true;
            record.CompletionsByNation["united_states"] = 3;
            profile.MissionResults["m01_bocage_crossroads"] = record;

            SaveSystem.Save(profile);
            var loaded = SaveSystem.Load();

            Require(loaded.SchemaVersion == SaveSystem.CurrentSchemaVersion, "loaded schema version is current");
            Require(loaded.TutorialCompleted, "TutorialCompleted round-trips");
            Require(Math.Abs(loaded.MasteryXp["united_states"] - 275.5) < 0.001, "MasteryXp round-trips");
            Require(loaded.UnlockedAchievements.Contains(AchievementIds.FirstMissionWon), "achievements round-trip");
            Require(loaded.CampaignMissionsCompleted.Contains("m01_bocage_crossroads"), "campaign progress round-trips");
            Require(loaded.BestDifficultyCompleted == Difficulty.Regular, "BestDifficultyCompleted round-trips");
            var loadedRecord = loaded.MissionResults["m01_bocage_crossroads"];
            Require(loadedRecord.BestStars[0] && loadedRecord.BestStars[1] && !loadedRecord.BestStars[2], "BestStars round-trips");
            Require(loadedRecord.CompletionsByNation["united_states"] == 3, "CompletionsByNation round-trips");
        });
    }

    // --- (e) v1 -> v2 migration -------------------------------------------

    [Test]
    public void V1SaveJsonMigratesToV2WithDefaults()
    {
        const string v1Json = @"{
            ""SchemaVersion"": 1,
            ""MissionResults"": {},
            ""CampaignMissionsCompleted"": [""m01_bocage_crossroads""],
            ""BestDifficultyCompleted"": 1,
            ""UnlockedAchievements"": [""first_mission_won""]
        }";

        var profile = SaveSystem.LoadFromJson(v1Json);

        Require(profile.SchemaVersion == SaveSystem.CurrentSchemaVersion, "migrated profile reports the current schema version");
        Require(profile.MasteryXp != null && profile.MasteryXp.Count == 0, "v1's missing MasteryXp defaults to an empty map");
        Require(!profile.TutorialCompleted, "v1's missing TutorialCompleted defaults to false");
        Require(profile.CampaignMissionsCompleted.Contains("m01_bocage_crossroads"), "pre-existing v1 data survives migration");
        Require(profile.UnlockedAchievements.Contains("first_mission_won"), "pre-existing v1 achievements survive migration");
    }

    // --- (f) corrupted file -------------------------------------------------

    [Test]
    public void CorruptedSaveFileYieldsFreshProfileWithoutThrowing()
    {
        WithScratchSaveDirectory("corrupt", () =>
        {
            if (!DirAccess.DirExistsAbsolute(SaveSystem.SaveDirectory))
                DirAccess.MakeDirRecursiveAbsolute(SaveSystem.SaveDirectory);
            string path = $"{SaveSystem.SaveDirectory}/{SaveSystem.SaveFileName}";
            using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Write))
                file.StoreString("{ this is not valid json ][");

            PlayerProfile loaded = null;
            Exception thrown = null;
            try { loaded = SaveSystem.Load(); }
            catch (Exception e) { thrown = e; }

            Require(thrown == null, "loading a corrupted save does not throw");
            Require(loaded != null, "a fresh profile is returned");
            Require(loaded.SchemaVersion == SaveSystem.CurrentSchemaVersion, "the fresh profile reports the current schema version");
            Require(loaded.MissionResults.Count == 0, "the fresh profile has no mission results");
        });
    }

    // --- (g) NullPlatformService --------------------------------------------

    [Test]
    public void NullPlatformServiceRecordsAchievementsAndReportsUnavailable()
    {
        var service = new NullPlatformService();

        Require(!service.IsAvailable, "NullPlatformService reports unavailable");
        Require(!service.IsAchievementUnlocked(AchievementIds.FirstMissionWon), "achievement starts locked");

        service.UnlockAchievement(AchievementIds.FirstMissionWon);

        Require(service.IsAchievementUnlocked(AchievementIds.FirstMissionWon), "achievement is recorded after unlocking");
        Require(!service.IsAchievementUnlocked(AchievementIds.WonWithoutLosingDefenseLine), "an unrelated achievement stays locked");

        service.SyncCloudSaves(); // must not throw with no backend present
    }

    // --- integration: ProgressionService.RecordResult end to end -----------

    [Test]
    public void RecordResultMergesBestStarsAwardsXpAndReportsNewUnlocks()
    {
        var config = Config();
        var objective = new StarObjectiveDefinition
            { Kind = StarObjectiveKind.MaxTowersBuilt, IntParameter = 8, Description = "test objective" };
        var mission = new MissionDefinition { Id = "m01_bocage_crossroads", StarObjective = objective };
        var profile = new PlayerProfile();

        var firstRun = new MissionStatsSnapshot
            { Victory = true, TowersBuilt = 9, FinalIntegrity = 20, MaxIntegrity = 20, Difficulty = Difficulty.Regular };
        var firstSummary = ProgressionService.RecordResult(profile, mission, "united_states", firstRun, config);

        Require(firstSummary.StarsEarnedThisRun[0] && firstSummary.StarsEarnedThisRun[1] && !firstSummary.StarsEarnedThisRun[2],
            "first run earns stars 1 and 2 only (9 towers misses the objective)");
        Require(firstSummary.NewUnlockMessages.Contains("Signature tower unlocked (united_states)"),
            "the first United States completion unlocks the signature tower");
        Require(profile.UnlockedAchievements.Contains(AchievementIds.FirstMissionWon), "first mission won achievement recorded");
        Require(profile.UnlockedAchievements.Contains(AchievementIds.WonWithoutLosingDefenseLine),
            "won-without-losing-Defense-Line achievement recorded (Defense Line stayed at max)");

        var secondRun = new MissionStatsSnapshot
            { Victory = true, TowersBuilt = 5, FinalIntegrity = 20, MaxIntegrity = 20, Difficulty = Difficulty.Regular };
        var secondSummary = ProgressionService.RecordResult(profile, mission, "united_states", secondRun, config);

        Require(secondSummary.BestStars[2], "star 3 is now merged into the best-of record from the second run");
        Require(secondSummary.MasteryXpGained > 0f, "a completion awards mastery XP");
        Require(profile.MissionResults["m01_bocage_crossroads"].CompletionsByNation["united_states"] == 2,
            "two United States completions recorded against this mission");
    }

    private static void WithScratchSaveDirectory(string subfolder, Action action)
    {
        string originalDirectory = SaveSystem.SaveDirectory;
        string originalFileName = SaveSystem.SaveFileName;
        try
        {
            SaveSystem.SaveDirectory = $"user://progression_tests_{subfolder}";
            SaveSystem.SaveFileName = "profile.json";
            action();
        }
        finally
        {
            SaveSystem.SaveDirectory = originalDirectory;
            SaveSystem.SaveFileName = originalFileName;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }

    private static void RequireApproximately(float expected, float actual, string message)
    {
        if (MathF.Abs(expected - actual) > 0.01f)
            throw new InvalidOperationException($"Assertion failed: {message}; expected {expected}, got {actual}");
    }
}
