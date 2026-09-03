using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.UI.Flow;

public partial class BriefingController : Node2D
{
    public override void _Ready()
    {
        var box = new VBoxContainer { Position = new Vector2(130, 90), Size = new Vector2(700, 420) };
        AddChild(box);
        box.AddChild(new Label { Text = "FRONTS OF WAR  /  MISSION BRIEFING", Theme = null });
        box.AddChild(new Label { Text = "BOCAGE CROSSROADS\n\nEnemy armor is probing the crossroads. Hold the Defense Line through twelve waves.\n\nKnown finale threat: B1 Breakthrough Panzer. Its armor skirt yields fastest to Explosive damage;\nafter it breaks, switch to Armor-Piercing fire.\n\nSignature available: Arsenal of Democracy Factory — continuous friendly-unit production." });
        var button = new Button { Text = "Continue to Loadout", CustomMinimumSize = new Vector2(260, 48) };
        button.Pressed += () => GetTree().ChangeSceneToFile("res://scenes_root/loadout.tscn");
        box.AddChild(button);
    }
}
