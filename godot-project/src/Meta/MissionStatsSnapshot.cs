using System.Collections.Generic;
using FrontsOfWar.Core;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Meta;

// A finished mission's raw facts (GDD §11.3 star evaluation, §12.3 mastery
// XP). Built once by MissionStatsCollector at mission end and handed to
// StarEvaluator/ProgressionService — deliberately dumb data, no behavior.
public sealed class MissionStatsSnapshot
{
    public bool Victory;
    public int TowersBuilt;
    public HashSet<TowerArchetype> ArchetypesUsed = new();
    public bool BossKilled;
    public int FinalIntegrity;
    public int MaxIntegrity;
    public Difficulty Difficulty;
    public int WaveReached;
}
