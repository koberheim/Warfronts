using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Flow;

// Main menu (GDD §13.1; docs/UI_DESIGN_SPEC.md §8.1): a paper sheet on the
// war table with Campaign as the one primary action. Skirmish, Endless,
// Codex and Settings are shown locked until their ladder prompts land
// (D54). The right two-thirds of the table stays clear for the campaign map.
public partial class MainMenuController : Node2D
{
    public override void _Ready()
    {
        GameLoop.Instance?.Time.Resume();
        var content = FlowScreen.Build(this);
        var column = FlowScreen.PaperSheet(content, 520f, 720f, Control.LayoutPreset.CenterLeft, out _, 40, 0, 10);

        column.AddChild(UiFactory.Label("StampLabel", "COMMANDER'S TABLE", uppercase: true));
        column.AddChild(UiFactory.Label("PaperTitleLabel", "FRONTS OF WAR", uppercase: true));
        column.AddChild(UiFactory.Wrapped("PaperSubheadingLabel", "A commander's table. Six nations. One front."));
        column.AddChild(UiFactory.Rule(true));
        column.AddChild(UiFactory.Spacer(0f, 8f));

        var campaign = UiFactory.Button("PrimaryButton", "Campaign", () => GetTree().ChangeSceneToFile("res://scenes_root/briefing.tscn"));
        campaign.CustomMinimumSize = new Vector2(0f, 52f);
        column.AddChild(campaign);

        foreach (var name in new[] { "Skirmish", "Endless", "Codex", "Settings" })
        {
            var button = UiFactory.Button("PaperButton", name, null, "lock");
            button.Disabled = true;
            button.TooltipText = "Later in development";
            button.CustomMinimumSize = new Vector2(0f, 44f);
            column.AddChild(button);
        }
        column.AddChild(UiFactory.Wrapped("PaperSmallLabel", "Skirmish, Endless, the Codex and Settings arrive later in development."));

        column.AddChild(UiFactory.Spacer(expand: true));
        column.AddChild(UiFactory.Button("PaperButton", "Quit", () => GetTree().Quit()));
        column.AddChild(UiFactory.Label("PaperSmallLabel", "Pre-alpha build · fictionalized theaters, invented markings", HorizontalAlignment.Center));

        campaign.GrabFocus();
    }
}
