using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;

namespace FrontsOfWar.UI.Flow;

public partial class BriefingController : Node2D
{
    public override void _Ready()
    {
        var mission = GD.Load<MissionDefinition>(MissionSession.CurrentMissionPath);
        if (!string.IsNullOrEmpty(mission?.Title)) MissionSession.LastMissionTitle = mission.Title;

        var box = new VBoxContainer { Position = new Vector2(130, 90), Size = new Vector2(700, 420) };
        AddChild(box);
        box.AddChild(new Label { Text = "FRONTS OF WAR  /  MISSION BRIEFING", Theme = null });
        string title = mission?.Title?.ToUpperInvariant() ?? MissionSession.LastMissionTitle.ToUpperInvariant();
        string briefingText = mission?.BriefingText ?? "";
        box.AddChild(new Label { Text = $"{title}\n\n{briefingText}" });
        var button = new Button { Text = "Continue to Loadout", CustomMinimumSize = new Vector2(260, 48) };
        button.Pressed += () => GetTree().ChangeSceneToFile("res://scenes_root/loadout.tscn");
        box.AddChild(button);
    }
}
