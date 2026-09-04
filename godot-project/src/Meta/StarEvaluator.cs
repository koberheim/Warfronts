using FrontsOfWar.Core;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Meta;

// Pure (GDD §11.3): three stars per mission — completion, Defense Line
// health, and a mission-specific objective. Takes GameBalanceConfig so the
// 75% threshold stays the one tuning surface (CLAUDE.md principle 2)
// instead of a magic number here.
public static class StarEvaluator
{
    public static bool[] Evaluate(MissionStatsSnapshot stats, StarObjectiveDefinition objective, GameBalanceConfig config)
    {
        bool star1 = stats?.Victory ?? false;
        bool star2 = star1 && stats.MaxIntegrity > 0
            && (float)stats.FinalIntegrity / stats.MaxIntegrity >= config.Star2DefenseLineThreshold;
        bool star3 = star1 && EvaluateObjective(stats, objective);
        return new[] { star1, star2, star3 };
    }

    private static bool EvaluateObjective(MissionStatsSnapshot stats, StarObjectiveDefinition objective)
    {
        if (objective == null) return false;
        return objective.Kind switch
        {
            StarObjectiveKind.MaxTowersBuilt => stats.TowersBuilt <= objective.IntParameter,
            StarObjectiveKind.NoArchetype => !stats.ArchetypesUsed.Contains((TowerArchetype)objective.IntParameter),
            StarObjectiveKind.MinDifficulty => (int)stats.Difficulty >= objective.IntParameter,
            StarObjectiveKind.BossKilled => stats.BossKilled,
            _ => false,
        };
    }
}
