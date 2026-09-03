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

    public static void ResetMission()
    {
        LastMissionWon = false;
        LastWaveReached = 0;
    }
}
