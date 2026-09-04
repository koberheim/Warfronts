using System.Collections.Generic;

namespace FrontsOfWar.Meta;

// What ResultsController shows after a mission (GDD §19 prompt 41 —
// ProgressionService.RecordResult "returns a summary the results screen can
// show").
public sealed class ProgressionSummary
{
    public bool[] StarsEarnedThisRun { get; init; } = new bool[3];
    public bool[] BestStars { get; init; } = new bool[3];
    public float MasteryXpGained { get; init; }
    public int MasteryRankBefore { get; init; }
    public int MasteryRankAfter { get; init; }
    public bool RankedUp => MasteryRankAfter > MasteryRankBefore;
    public List<string> AchievementsUnlocked { get; init; } = new();
    public List<string> NewUnlockMessages { get; init; } = new();
}
