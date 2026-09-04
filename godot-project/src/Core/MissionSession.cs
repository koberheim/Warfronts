using System.Collections.Generic;
using FrontsOfWar.Meta;

namespace FrontsOfWar.Core;

// Small process-local handoff between the mission scenes. Persistent
// campaign progress (GDD §19 prompt 41) is read from/written to
// ProfileStore.Current around this — MissionSession itself stays the
// per-run scratch state for the M3 briefing/loadout/results loop and
// one-click retry.
public static class MissionSession
{
    public static bool TutorialCompleted { get; set; }
    public static bool LastMissionWon { get; set; }
    public static string LastMissionTitle { get; set; } = "Bocage Crossroads";
    public static int LastWaveReached { get; set; }

    // The mission currently being briefed/played (GDD §19 prompt 41). Only
    // one campaign mission is authored so far; this is a resource path
    // rather than a nation id since MissionDefinition itself is
    // nation-neutral (§10.4).
    public static string CurrentMissionPath { get; set; } = "res://assets/data/missions/m01_bocage_crossroads.tres";

    // The nation played this run. United States only until §13.3's full
    // nation-selection UI exists, matching LoadoutController's current
    // United-States-only doctrine picker.
    public static string CurrentNationId { get; set; } = "united_states";

    // Filled in by MissionStatsCollector when MissionCompletedEvent fires
    // (GDD §19 prompt 41: "its snapshot is stored in MissionSession.LastResult
    // at mission end"). ResultsController reads this to call
    // ProgressionService.RecordResult.
    public static MissionStatsSnapshot LastResult { get; set; }

    // The doctrine picked in the loadout screen (GDD §8.3, §19 prompt 39) —
    // a bare doctrine id (e.g. "lend_lease"), not a resource path, resolved
    // against the current nation by DoctrineSystem.LoadDoctrine. Defaults to
    // the United States' first doctrine, matching Loadout's default US kit.
    public static string SelectedDoctrineId { get; set; } = "lend_lease";

    // The six build-bar towers for the next mission (GDD §13.3's loadout
    // screen picks these; §13.4's build bar shows exactly these six with
    // hotkeys Q/W/E/R/T/Y in list order). Stored as resource paths rather
    // than live TowerDefinition references since this is a static field that
    // outlives scene changes (Claude decision — see docs/DECISIONS.md).
    // Defaults to the GDD's recommended United States Mission 1 loadout;
    // there is no loadout-selection UI yet (§13.3 is deferred), so this is
    // currently the only loadout the game offers.
    public static List<string> Loadout { get; set; } = new()
    {
        "res://assets/data/towers/t1_automatic_gun.tres",
        "res://assets/data/towers/t3_field_mortar.tres",
        "res://assets/data/towers/t4_anti_tank_gun.tres",
        "res://assets/data/towers/t9_command_post.tres",
        "res://assets/data/towers/t2_marksman_post.tres",
        "res://assets/data/towers/t5_flak_battery.tres",
    };

    public static void ResetMission()
    {
        LastMissionWon = false;
        LastWaveReached = 0;
        LastResult = null;
    }
}
