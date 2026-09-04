namespace FrontsOfWar.Platform;

// GDD §15.5 — every platform touchpoint (achievements, cloud saves) is
// wrapped behind this interface so Steam is never a dependency of the game
// running. NullPlatformService is the only implementation this task builds;
// a GodotSteam-backed SteamService is the rest of §19 prompt 43 and is
// intentionally out of scope here.
public interface IPlatformService
{
    bool IsAvailable { get; }
    void UnlockAchievement(string achievementId);
    bool IsAchievementUnlocked(string achievementId);
    void SyncCloudSaves();
}
