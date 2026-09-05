using Godot;
using FrontsOfWar.Debug;
using FrontsOfWar.Meta;
#if DEBUG
using System.Reflection;
using Chickensoft.GoDotTest;
#endif

namespace FrontsOfWar.Core;

// The project entry point. Normal play enters the main menu or mission;
// developer tooling, validation, and tests use explicit command-line routes.
public partial class Boot : Node2D
{
    public const string MainMenuScene = "res://scenes_root/main_menu.tscn";
    public const string MissionScene = "res://scenes_root/mission.tscn";
    public const string MapEditorScene = "res://scenes_root/map_editor.tscn";

#if DEBUG
    private TestEnvironment _testEnvironment;
#endif

    public override void _Ready()
    {
#if DEBUG
        if (HasCmdlineArg("--validate-data"))
        {
            CallDeferred(nameof(RunDataValidation));
            return;
        }
        var environment = TestEnvironment.From(OS.GetCmdlineArgs());
        if (environment.ShouldRunTests)
        {
            _testEnvironment = environment;
            CallDeferred(nameof(RunTests));
            return;
        }
#endif
        CallDeferred(nameof(StartMission));
    }

    private static bool HasCmdlineArg(string flag)
    {
        return HasCmdlineArg(OS.GetCmdlineArgs(), flag);
    }

    private static bool HasCmdlineArg(string[] args, string flag)
    {
        foreach (string arg in args)
            if (arg == flag)
                return true;
        return false;
    }

    // Pure launch resolution keeps the developer boundary directly testable.
    // --screen is deliberately allowlisted: it is a screenshot convenience,
    // not a general scene loader and cannot be used to enter map_editor.
    public static string ResolveLaunchScene(string[] args, bool isHeadless, bool developerToolsAvailable)
    {
        if (!developerToolsAvailable) return MainMenuScene;
        if (HasCmdlineArg(args, "--smoke-test")) return "res://tests/fixtures/combat_smoke.tscn";
        if (developerToolsAvailable && HasCmdlineArg(args, "--map-editor"))
            return MapEditorScene;

        string requestedScreen = ScreenshotCapture.ArgValue(args, "--screen");
        string screenScene = requestedScreen switch
        {
            "main_menu" => MainMenuScene,
            "campaign_selection" => "res://scenes_root/campaign_selection.tscn",
            "settings" => "res://scenes_root/settings.tscn",
            "briefing" => "res://scenes_root/briefing.tscn",
            "loadout" => "res://scenes_root/loadout.tscn",
            "mission" => MissionScene,
            "results" => "res://scenes_root/results.tscn",
            _ => null,
        };
        if (screenScene != null) return screenScene;

        if (isHeadless || HasCmdlineArg(args, "--mission")) return MissionScene;
        return MainMenuScene;
    }

    private static bool DeveloperToolsAvailable()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

#if DEBUG
    private void RunDataValidation()
    {
        var report = DataValidator.ValidateProjectData();
        GD.Print(report.BuildReportText());
        GetTree().Quit(report.ExitCode);
    }

    private async void RunTests()
    {
        await GoTest.RunTests(Assembly.GetExecutingAssembly(), this, _testEnvironment);
    }
#endif

    private void StartMission()
    {
        string[] args = OS.GetCmdlineArgs();
        bool developerToolsAvailable = DeveloperToolsAvailable();
        string requestedScreen = ScreenshotCapture.ArgValue(args, "--screen");
        string launchScene = ResolveLaunchScene(
            args,
            DisplayServer.GetName() == "headless",
            developerToolsAvailable);

        if (HasCmdlineArg(args, "--map-editor") && !developerToolsAvailable)
            GD.PushWarning("--map-editor is unavailable in player/release builds; starting the normal game.");
        if (!string.IsNullOrEmpty(requestedScreen) && !IsAllowedScreen(requestedScreen))
            GD.PushWarning($"Ignoring unsupported --screen value '{requestedScreen}'.");

        // Dev-only screenshot capture (D54): the capture node rides on the
        // tree root across the scene change. Screen values are allowlisted
        // above so this cannot bypass the editor's launch boundary.
        var capture = developerToolsAvailable ? ScreenshotCapture.FromCmdline(args) : null;
        if (capture != null)
        {
            capture.ScreenName = launchScene.GetFile().GetBaseName();
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, capture);
        }

        // The editor is isolated from player profile and MissionSession
        // initialization. Phase 2 will add its own document state.
        if (launchScene == MapEditorScene)
        {
            GetTree().ChangeSceneToFile(launchScene);
            return;
        }

        // Loads the profile once (GDD section 19 prompt 41) and mirrors its
        // tutorial flag into per-run MissionSession state.
        MissionSession.TutorialCompleted = ProfileStore.Current.TutorialCompleted;
        if (developerToolsAvailable && HasCmdlineArg(args, "--skip-tutorial")) MissionSession.TutorialCompleted = true;

        // Bindings/audio/fullscreen/UI-scale/colorblind theme (GDD §13.8-13.9)
        // must apply from the first frame, not only once the player opens
        // Settings. Safe in headless runs - UserSettings.Apply no-ops the
        // window-specific calls when DisplayServer.GetName() == "headless".
        UserSettings.Apply(GetTree());

        // Normal play still starts at the main menu (GDD section 13.1; UI
        // spec section 8.1); mission/headless/screenshot routes are unchanged.
        GetTree().ChangeSceneToFile(launchScene);
    }

    private static bool IsAllowedScreen(string screen)
    {
        return screen is "main_menu" or "campaign_selection" or "settings" or "briefing" or "loadout" or "mission" or "results";
    }
}
