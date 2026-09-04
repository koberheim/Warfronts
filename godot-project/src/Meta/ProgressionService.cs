using System.Collections.Generic;
using FrontsOfWar.Core;
using FrontsOfWar.Platform;

namespace FrontsOfWar.Meta;

// GDD §19 prompt 41. Orchestrates one mission result into the player's
// persistent profile: stars (best-of merge), Faction Mastery XP, unlock
// gate deltas, and two of GDD §12.5's example achievements. Pure aside from
// the PlatformServices.Current.UnlockAchievement call, itself a no-op
// against NullPlatformService until Steam lands (§19 prompt 43).
public static class ProgressionService
{
    public static ProgressionSummary RecordResult(
        PlayerProfile profile, MissionDefinition mission, string nationId,
        MissionStatsSnapshot stats, GameBalanceConfig config)
    {
        var before = UnlockGateSnapshot.Capture(profile, nationId);
        string missionId = mission?.Id ?? "";

        bool[] starsEarned = StarEvaluator.Evaluate(stats, mission?.StarObjective, config);
        var record = RecordMissionResult(profile, missionId, nationId, stats, starsEarned);

        float xpGained = stats.Victory ? MasteryService.XpForCompletion(stats.Difficulty, starsEarned, config) : 0f;
        profile.MasteryXp.TryGetValue(nationId, out double xpBefore);
        int rankBefore = MasteryService.RankFor((float)xpBefore, config);
        double xpAfter = xpBefore + xpGained;
        profile.MasteryXp[nationId] = xpAfter;
        int rankAfter = MasteryService.RankFor((float)xpAfter, config);

        var achievementsUnlocked = new List<string>();
        if (stats.Victory)
        {
            TryUnlockAchievement(profile, AchievementIds.FirstMissionWon, achievementsUnlocked);
            if (stats.MaxIntegrity > 0 && stats.FinalIntegrity >= stats.MaxIntegrity)
                TryUnlockAchievement(profile, AchievementIds.WonWithoutLosingDefenseLine, achievementsUnlocked);
        }

        var after = UnlockGateSnapshot.Capture(profile, nationId);

        return new ProgressionSummary
        {
            StarsEarnedThisRun = starsEarned,
            BestStars = (bool[])record.BestStars.Clone(),
            MasteryXpGained = xpGained,
            MasteryRankBefore = rankBefore,
            MasteryRankAfter = rankAfter,
            AchievementsUnlocked = achievementsUnlocked,
            NewUnlockMessages = before.DiffMessages(after, nationId),
        };
    }

    private static MissionRecord RecordMissionResult(
        PlayerProfile profile, string missionId, string nationId, MissionStatsSnapshot stats, bool[] starsEarned)
    {
        if (!profile.MissionResults.TryGetValue(missionId, out var record))
        {
            record = new MissionRecord();
            profile.MissionResults[missionId] = record;
        }

        for (int i = 0; i < 3 && i < starsEarned.Length; i++)
            record.BestStars[i] = record.BestStars[i] || starsEarned[i];

        if (stats.Victory)
        {
            record.CompletionsByNation.TryGetValue(nationId, out int completions);
            record.CompletionsByNation[nationId] = completions + 1;
            if (record.BestDifficulty is null || stats.Difficulty > record.BestDifficulty)
                record.BestDifficulty = stats.Difficulty;

            profile.CampaignMissionsCompleted.Add(missionId);
            if (profile.BestDifficultyCompleted is null || stats.Difficulty > profile.BestDifficultyCompleted)
                profile.BestDifficultyCompleted = stats.Difficulty;
        }

        return record;
    }

    private static void TryUnlockAchievement(PlayerProfile profile, string id, List<string> unlockedThisRun)
    {
        if (!profile.UnlockedAchievements.Add(id)) return;
        PlatformServices.Current.UnlockAchievement(id);
        unlockedThisRun.Add(id);
    }

    // Captures the boolean state of every UnlockService gate relevant to one
    // nation, before and after a RecordResult call, so the results screen
    // can announce exactly what just newly unlocked.
    private readonly struct UnlockGateSnapshot
    {
        private readonly bool _signature;
        private readonly bool _doctrine2;
        private readonly bool _doctrine3;
        private readonly bool _skirmish;
        private readonly bool _endless;
        private readonly bool _veteran;
        private readonly bool _elite;

        private UnlockGateSnapshot(bool signature, bool doctrine2, bool doctrine3,
            bool skirmish, bool endless, bool veteran, bool elite)
        {
            _signature = signature;
            _doctrine2 = doctrine2;
            _doctrine3 = doctrine3;
            _skirmish = skirmish;
            _endless = endless;
            _veteran = veteran;
            _elite = elite;
        }

        public static UnlockGateSnapshot Capture(PlayerProfile profile, string nationId) => new(
            UnlockService.IsSignatureUnlocked(profile, nationId),
            UnlockService.IsDoctrineUnlocked(profile, nationId, 1),
            UnlockService.IsDoctrineUnlocked(profile, nationId, 2),
            UnlockService.IsSkirmishUnlocked(profile),
            UnlockService.IsEndlessUnlocked(profile),
            UnlockService.IsDifficultyUnlocked(profile, Difficulty.Veteran),
            UnlockService.IsDifficultyUnlocked(profile, Difficulty.Elite));

        public List<string> DiffMessages(UnlockGateSnapshot after, string nationId)
        {
            var messages = new List<string>();
            if (!_signature && after._signature) messages.Add($"Signature tower unlocked ({nationId})");
            if (!_doctrine2 && after._doctrine2) messages.Add($"Doctrine 2 unlocked ({nationId})");
            if (!_doctrine3 && after._doctrine3) messages.Add($"Doctrine 3 unlocked ({nationId})");
            if (!_skirmish && after._skirmish) messages.Add("Skirmish mode unlocked");
            if (!_endless && after._endless) messages.Add("Endless mode unlocked");
            if (!_veteran && after._veteran) messages.Add("Veteran difficulty unlocked");
            if (!_elite && after._elite) messages.Add("Elite difficulty unlocked");
            return messages;
        }
    }
}
