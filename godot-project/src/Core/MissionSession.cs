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

    // The nation played this run. Campaign selection writes this before the
    // briefing/loadout flow; runtime reads it when applying nation variants.
    public static string CurrentNationId { get; set; } = "united_states";

    // Campaign selection is process-local until the runtime consumes it.
    // Alliance remains explicit even though NationProfile also carries it: the
    // GDD flow makes this the player's first fiction-facing decision.
    public static string SelectedAllianceId { get; set; } = "Allies";
    public static Difficulty SelectedDifficulty { get; set; } = Difficulty.Regular;

    // The six ordered build-bar slots. Each path resolves to either a
    // TowerDefinition, SignatureDefinition, or ArsenalDefinition. A selected
    // national signature occupies one of these six slots; it never grants a
    // seventh build choice. Consumers must type-dispatch the Resource while
    // preserving this Q/W/E/R/T/Y order.
    public static List<string> BuildSlotResourcePaths { get; set; } = new()
    {
        "res://assets/data/towers/t1_automatic_gun.tres",
        "res://assets/data/towers/t3_field_mortar.tres",
        "res://assets/data/towers/t4_anti_tank_gun.tres",
        "res://assets/data/towers/t9_command_post.tres",
        "res://assets/data/towers/t2_marksman_post.tres",
        "res://assets/data/towers/t5_flak_battery.tres",
    };

    // Filled in by MissionStatsCollector when MissionCompletedEvent fires
    // (GDD §19 prompt 41: "its snapshot is stored in MissionSession.LastResult
    // at mission end"). ResultsController must claim it before persisting so
    // recreating results.tscn cannot award the same completion twice.
    public static MissionStatsSnapshot LastResult { get; private set; }
    private static bool _lastResultPersistenceClaimed;

    // The doctrine picked in the loadout screen (GDD §8.3, §19 prompt 39) —
    // a bare doctrine id (e.g. "lend_lease"), not a resource path, resolved
    // against the current nation by DoctrineSystem.LoadDoctrine. Defaults to
    // the United States' first doctrine for a fresh process; the campaign
    // selection and loadout screens replace it with the chosen nation's id.
    public static string SelectedDoctrineId { get; set; } = "lend_lease";

    // Temporary TowerDefinition-only projection for the current build bar.
    // BuildSlotResourcePaths is authoritative; this list will be removed
    // when the build bar consumes generic paths and dispatches signatures.
    // A signature selection therefore leaves five entries here until that
    // runtime migration lands, rather than miscasting a signature as a tower.
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
        _lastResultPersistenceClaimed = false;
    }

    public static void StoreCompletedResult(MissionStatsSnapshot result)
    {
        LastResult = result;
        _lastResultPersistenceClaimed = false;
    }

    // Results can be reached only once for a completed run. This in-memory
    // claim is deliberately reset only when a new mission result is stored or
    // the player starts/retries/abandons a mission via ResetMission.
    public static bool TryClaimResultForPersistence(out MissionStatsSnapshot result)
    {
        result = LastResult;
        if (result == null || _lastResultPersistenceClaimed) return false;
        _lastResultPersistenceClaimed = true;
        return true;
    }
}
