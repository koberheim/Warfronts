using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.UI.Menus;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Flow;

// GDD §13.8-13.9. Reachable from the main menu; PauseMenu embeds
// SettingsPanel directly instead of using this scene, so opening Settings
// mid-mission never changes scenes.
public partial class SettingsController : Node2D
{
    public override void _Ready()
    {
        GameLoop.Instance?.Time.Resume();
        var content = FlowScreen.Build(this);
        var column = FlowScreen.PaperSheet(content, 900f, 840f, Control.LayoutPreset.Center, out _, 32, 24, 12);
        column.AddChild(UiFactory.Label("StampLabel", "SETTINGS", uppercase: true));
        column.AddChild(UiFactory.Label("PaperTitleLabel", "OPTIONS", uppercase: true));
        column.AddChild(UiFactory.Rule(true));

        var scroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        column.AddChild(scroll);
        var panel = new SettingsPanel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        scroll.AddChild(panel);
        panel.Setup(() => GetTree().ChangeSceneToFile("res://scenes_root/main_menu.tscn"));
    }
}
