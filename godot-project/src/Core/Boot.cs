using Godot;
using FrontsOfWar.Debug;
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
        if (DisplayServer.GetName() == "headless")
        {
            GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn");
            return;
        }
        foreach (string arg in OS.GetCmdlineArgs())
            if (arg == "--mission")
            {
                GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn");
                return;
            }
        GetTree().ChangeSceneToFile("res://scenes_root/briefing.tscn");
    }
}
