namespace FrontsOfWar.Platform;

// Process-wide access point for the active platform backend (GDD §15.5).
// Defaults to NullPlatformService so every caller works identically with no
// platform present; a future SteamService (the rest of §19 prompt 43) would
// assign this at boot once GodotSteam integration lands.
public static class PlatformServices
{
    public static IPlatformService Current { get; set; } = new NullPlatformService();
}
