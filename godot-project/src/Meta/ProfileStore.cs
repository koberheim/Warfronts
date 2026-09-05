using FrontsOfWar.Core;

namespace FrontsOfWar.Meta;

// Process-wide handle on the loaded PlayerProfile (GDD §19 prompt 41). Boot
// loads it once via SaveSystem.Load(); everything else (ResultsController's
// save, future loadout unlock gating) reads/writes this one instance rather
// than re-loading from disk.
public static class ProfileStore
{
    private static PlayerProfile _current;

    public static PlayerProfile Current => _current ??= SaveSystem.Load();

    public static void Save() => SaveSystem.Save(Current);

    // Callers that are part of player flow use this boundary so a filesystem
    // failure can be presented as retryable state instead of escaping from a
    // UI callback. The in-memory profile intentionally remains intact.
    public static bool TrySave(out string error)
    {
        try
        {
            Save();
            error = "";
            return true;
        }
        catch (System.Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    public static bool TryCompleteTutorial(out string error)
    {
        Current.TutorialCompleted = true;
        return TrySave(out error);
    }

    // Test-only: forces a specific (or fresh) in-memory profile without
    // touching disk, so successive tests don't see each other's mutations to
    // this static.
    public static void ResetForTests(PlayerProfile profile = null) => _current = profile ?? new PlayerProfile();
}
