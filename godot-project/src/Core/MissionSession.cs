using System.Collections.Generic;

namespace FrontsOfWar.Core;

// Small process-local handoff between the mission scenes. Persistent campaign
// saves are intentionally later milestone work; this only supports the M3
// briefing/loadout/results loop and one-click retry.
public static class MissionSession
{
    public static bool TutorialCompleted { get; set; }
    public static bool LastMissionWon { get; set; }
    public static string LastMissionTitle { get; set; } = "Bocage Crossroads";
    public static int LastWaveReached { get; set; }

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
    }
}
