using Godot;

namespace FrontsOfWar.UI.Flow;

public partial class LoadoutController : Node2D
{
    public override void _Ready()
    {
        var box = new VBoxContainer { Position = new Vector2(130, 80), Size = new Vector2(760, 470) };
        AddChild(box);
        box.AddChild(new Label { Text = "LOADOUT  /  UNITED STATES" });
        box.AddChild(new Label { Text = "Recommended Mission 1 loadout\n\n[Q] Automatic Gun     [W] Field Mortar     [E] Anti-Tank Gun\n[R] Command Post       [T] Marksman Post      [Y] Flak Battery\n\nDoctrine: none selected (doctrines arrive with later content).\nDifficulty: Regular\n\nBuild these six towers on open build pads from the in-mission build bar.\nThe Arsenal of Democracy signature factory is pre-placed on the map." });
        var button = new Button { Text = "Deploy to Bocage Crossroads", CustomMinimumSize = new Vector2(300, 48) };
        button.Pressed += () => GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn");
        box.AddChild(button);
    }
}
