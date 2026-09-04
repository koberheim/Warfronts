using System.Collections.Generic;
using FrontsOfWar.Core;

namespace FrontsOfWar.Meta;

// The persisted meta-progression state (GDD §12.8). Plain POCO serialized
// with System.Text.Json by SaveSystem — no Godot Resource/Node types here so
// it can be constructed and asserted against in a plain unit test without a
// scene tree. Field initializers double as "fresh profile" defaults.
public sealed class PlayerProfile
{
    // Must match SaveSystem.CurrentSchemaVersion; SaveSystem stamps this on
    // every Load()/Save() so a freshly-constructed profile that never went
    // through SaveSystem still self-reports correctly.
    public int SchemaVersion { get; set; } = 2;

    public bool TutorialCompleted { get; set; }

    // nationId -> accumulated Faction Mastery XP (GDD §12.3). Cosmetic-only
    // reward track; never touches gameplay stats (§12.2's absolute rule).
    public Dictionary<string, double> MasteryXp { get; set; } = new();

    // missionId -> best-of record across every attempt.
    public Dictionary<string, MissionRecord> MissionResults { get; set; } = new();

    // Distinct campaign mission ids ever completed — drives the "campaign
    // Mission 3" / "Mission 8" gates in GDD §9.5 (Skirmish/Endless unlocks).
    // Skirmish/Endless mode completions themselves (§19 prompt 42, not yet
    // implemented) are a different mode and deliberately do not feed this.
    public HashSet<string> CampaignMissionsCompleted { get; set; } = new();

    // Highest difficulty ever completed, on any mission, with any nation —
    // GDD §9.5's "complete any mission on Regular/Veteran" difficulty gates.
    public Difficulty? BestDifficultyCompleted { get; set; }

    public HashSet<string> UnlockedAchievements { get; set; } = new();
}

// One mission's best-ever outcome (GDD §11.3 stars, §9.5 unlock gates).
public sealed class MissionRecord
{
    // Index 0 = star 1 (completed), 1 = star 2 (>=75% Defense Line),
    // 2 = star 3 (the mission's StarObjectiveDefinition). Best-of merged
    // across every attempt by ProgressionService.RecordResult.
    public bool[] BestStars { get; set; } = new bool[3];
    public Difficulty? BestDifficulty { get; set; }

    // nationId -> number of times this specific mission has been completed
    // with that nation. Feeds UnlockService's signature/doctrine gates.
    public Dictionary<string, int> CompletionsByNation { get; set; } = new();
}
