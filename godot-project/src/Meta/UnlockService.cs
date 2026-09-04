using System.Linq;
using FrontsOfWar.Core;

namespace FrontsOfWar.Meta;

// Pure functions over PlayerProfile (GDD §9.5). All six nations and all 18
// doctrines' first slot are available from first launch — only a nation's
// second/third doctrine slot and signature tower are gated per nation,
// alongside the mode/difficulty gates below.
public static class UnlockService
{
    public static int NationCompletions(PlayerProfile profile, string nationId)
        => profile.MissionResults.Values.Sum(record =>
            record.CompletionsByNation.TryGetValue(nationId, out int count) ? count : 0);

    public static bool IsSignatureUnlocked(PlayerProfile profile, string nationId)
        => NationCompletions(profile, nationId) >= 1;

    // doctrineIndex 0 = the nation's first doctrine (never gated), 1 = its
    // second (2 completions), 2 = its third (4 completions) — GDD §9.5.
    public static bool IsDoctrineUnlocked(PlayerProfile profile, string nationId, int doctrineIndex) => doctrineIndex switch
    {
        0 => true,
        1 => NationCompletions(profile, nationId) >= 2,
        2 => NationCompletions(profile, nationId) >= 4,
        _ => false,
    };

    public static bool IsSkirmishUnlocked(PlayerProfile profile)
        => profile.CampaignMissionsCompleted.Count >= 3;

    public static bool IsEndlessUnlocked(PlayerProfile profile)
        => profile.CampaignMissionsCompleted.Count >= 8;

    public static bool IsDifficultyUnlocked(PlayerProfile profile, Difficulty difficulty) => difficulty switch
    {
        Difficulty.Recruit => true,
        Difficulty.Regular => true,
        Difficulty.Veteran => profile.BestDifficultyCompleted is >= Difficulty.Regular,
        Difficulty.Elite => profile.BestDifficultyCompleted is >= Difficulty.Veteran,
        _ => false,
    };
}
