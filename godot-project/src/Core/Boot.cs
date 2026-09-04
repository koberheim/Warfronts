using Godot;
using FrontsOfWar.Debug;
using FrontsOfWar.Meta;
#if DEBUG
using System.Reflection;
using Chickensoft.GoDotTest;
#endif

namespace FrontsOfWar.Core;

// The project entry point. Normal play enters the reusable mission scene;
// --run-tests routes the same Godot Mono process into the headless test
// suite, and --validate-data routes it into the Data Validator (GDD §19
// prompt 45) instead — both exit the process rather than falling through to
// StartMission.
public partial class Boot : Node2D
{
#if DEBUG
    private TestEnvironment _testEnvironment;
#endif

    public override void _Ready()
    {
        if (HasCmdlineArg("--validate-data"))
        {
            CallDeferred(nameof(RunDataValidation));
            return;
        }
#if DEBUG
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
        foreach (string arg in OS.GetCmdlineArgs())
            if (arg == flag)
                return true;
        return false;
    }

    private void RunDataValidation()
    {
        var report = DataValidator.ValidateProjectData();
        GD.Print(report.BuildReportText());
        GetTree().Quit(report.ExitCode);
    }

#if DEBUG
    private async void RunTests()
    {
        await GoTest.RunTests(Assembly.GetExecutingAssembly(), this, _testEnvironment);
    }
#endif

    private void StartMission()
    {
        // Loads the profile once (GDD §19 prompt 41) and mirrors its
        // tutorial flag into the per-run MissionSession, matching how the
        // rest of the M3 briefing/loadout/results flow reads MissionSession
        // rather than the profile directly.
        MissionSession.TutorialCompleted = ProfileStore.Current.TutorialCompleted;
        string[] args = OS.GetCmdlineArgs();
        if (HasCmdlineArg("--skip-tutorial")) MissionSession.TutorialCompleted = true;

        // Dev-only screenshot capture (D54): --screen picks the root scene,
        // the capture node rides on the tree root across the scene change.
        string screen = ScreenshotCapture.ArgValue(args, "--screen");
        var capture = ScreenshotCapture.FromCmdline(args);
        if (capture != null)
        {
            capture.ScreenName = string.IsNullOrEmpty(screen) ? (HasCmdlineArg("--mission") ? "mission" : "briefing") : screen;
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, capture);
        }

        if (!string.IsNullOrEmpty(screen))
        {
            GetTree().ChangeSceneToFile($"res://scenes_root/{screen}.tscn");
            return;
        }
        if (DisplayServer.GetName() == "headless" || HasCmdlineArg("--mission"))
        {
            GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn");
            return;
        }
        // Normal play starts at the main menu (GDD §13.1; UI spec §8.1).
        GetTree().ChangeSceneToFile("res://scenes_root/main_menu.tscn");
    }
}
