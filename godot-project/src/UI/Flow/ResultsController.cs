using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.UI.Flow;

public partial class ResultsController : Node2D
{
    public override void _Ready()
    {
        var box = new VBoxContainer { Position = new Vector2(180, 120), Size = new Vector2(620, 360) };
        AddChild(box);
        string result = MissionSession.LastMissionWon ? "MISSION COMPLETE" : "MISSION FAILED";
        box.AddChild(new Label { Text = $"{result}\n\n{MissionSession.LastMissionTitle}\nWave reached: {MissionSession.LastWaveReached}" });
        var retry = new Button { Text = "Retry Mission", CustomMinimumSize = new Vector2(240, 44) };
        retry.Pressed += () => { MissionSession.ResetMission(); GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn"); };
        box.AddChild(retry);
        var menu = new Button { Text = "Back to Briefing", CustomMinimumSize = new Vector2(240, 44) };
        menu.Pressed += () => GetTree().ChangeSceneToFile("res://scenes_root/briefing.tscn");
        box.AddChild(menu);
    }
}
