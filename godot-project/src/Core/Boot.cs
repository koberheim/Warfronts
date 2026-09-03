using Godot;
#if DEBUG
using System.Reflection;
using Chickensoft.GoDotTest;
#endif

namespace FrontsOfWar.Core;

// The project entry point. Normal play enters the reusable mission scene;
// --run-tests routes the same Godot Mono process into the headless test suite.
public partial class Boot : Node2D
{
#if DEBUG
    private TestEnvironment _testEnvironment;
#endif

    public override void _Ready()
    {
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

#if DEBUG
    private async void RunTests()
    {
        await GoTest.RunTests(Assembly.GetExecutingAssembly(), this, _testEnvironment);
    }
#endif

    private void StartMission()
    {
        if (OS.HasFeature("headless"))
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
