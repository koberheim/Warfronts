using FrontsOfWar.Core;

namespace FrontsOfWar.Meta;

// GDD §12.3 — Faction Mastery is a ten-rank, cosmetic-only track per nation.
// §12.2 is absolute: nothing here ever touches a gameplay stat.
public enum MasteryRewardTier
{
    ColorScheme,    // ranks 1-3: alternate tower color schemes
    Banner,         // ranks 4-6: profile banners and insignia
    TableDressing,  // ranks 7-9: alternate map-table dressing
    Title,          // rank 10: nation title + "Veteran Command" table frame
}

public static class MasteryService
{
    // Base XP * difficulty multiplier * (1 + bonus * extra stars), where
    // "extra stars" is stars earned beyond star 1 (0, 1, or 2). A failed
    // mission (no stars) earns no XP.
    public static float XpForCompletion(Difficulty difficulty, bool[] starsEarned, GameBalanceConfig config)
    {
        if (starsEarned == null || starsEarned.Length == 0 || !starsEarned[0]) return 0f;

        int extraStars = 0;
        for (int i = 1; i < starsEarned.Length; i++)
            if (starsEarned[i]) extraStars++;

        return config.MasteryXpBase * DifficultyMultiplier(difficulty, config)
            * (1f + config.MasteryXpExtraStarBonus * extraStars);
    }

    private static float DifficultyMultiplier(Difficulty difficulty, GameBalanceConfig config) => difficulty switch
    {
        Difficulty.Recruit => config.MasteryXpDifficultyMultiplierRecruit,
        Difficulty.Regular => config.MasteryXpDifficultyMultiplierRegular,
        Difficulty.Veteran => config.MasteryXpDifficultyMultiplierVeteran,
        Difficulty.Elite => config.MasteryXpDifficultyMultiplierElite,
        _ => config.MasteryXpDifficultyMultiplierRegular,
    };

    // Highest rank (1-10) whose threshold the given XP total meets or beats.
    public static int RankFor(float xp, GameBalanceConfig config)
    {
        var thresholds = config.MasteryRankXpThresholds;
        int rank = 1;
        for (int i = 0; i < thresholds.Length; i++)
            if (xp >= thresholds[i]) rank = i + 1;
        return rank;
    }

    public static MasteryRewardTier RewardTierFor(int rank) => rank switch
    {
        <= 3 => MasteryRewardTier.ColorScheme,
        <= 6 => MasteryRewardTier.Banner,
        <= 9 => MasteryRewardTier.TableDressing,
        _ => MasteryRewardTier.Title,
    };
}
