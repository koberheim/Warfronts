using System.Collections.Generic;

namespace FrontsOfWar.Platform;

// Used whenever no platform backend is present — the editor, headless test
// runs, and every build until GodotSteam is wired in (GDD §15.5: "Steam is
// never a dependency of the game running"). Tracks unlocked achievements
// in-memory only, purely so callers can observe that a call actually
// happened; nothing here persists to disk or talks to any external service.
public sealed class NullPlatformService : IPlatformService
{
    private readonly HashSet<string> _unlockedAchievements = new();

    public bool IsAvailable => false;

    public void UnlockAchievement(string achievementId)
    {
        if (!string.IsNullOrEmpty(achievementId)) _unlockedAchievements.Add(achievementId);
    }

    public bool IsAchievementUnlocked(string achievementId) => _unlockedAchievements.Contains(achievementId);

    public void SyncCloudSaves()
    {
        // No-op: there is no cloud without a platform backend.
    }
}
