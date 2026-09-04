using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Menus;

// The pause menu (docs/UI_DESIGN_SPEC.md §8.7; GDD §13.7): a paper card
// with Resume (focused), Restart, Settings (later), Abandon (inline
// confirm) and Quit to Menu. Holds the simulation while open and gives the
// previous pause state back on Resume, so a menu opened over the tutorial's
// pause does not un-pause the tutorial. Only the card blocks the mouse: the
// HUD underneath keeps working, matching "building stays available".
public partial class PauseMenu : Control
{
    private PanelContainer _card;
    private Button _resume;
    private HBoxContainer _confirmRow;
    private bool _wasPaused;

    public bool IsOpen => _card.Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        ProcessMode = ProcessModeEnum.Always;

        _card = UiFactory.Panel("PaperPanel");
        _card.CustomMinimumSize = new Vector2(420f, 0f);
        _card.Visible = false;
        AddChild(_card);
        UiFactory.Anchor(_card, LayoutPreset.Center, 0, 0);

        var column = UiFactory.VBox(10);
        _card.AddChild(column);
        column.AddChild(UiFactory.Label("StampLabel", "PAUSED", uppercase: true));
        column.AddChild(UiFactory.Wrapped("PaperSmallLabel", "Planning mode — building and upgrading stay available while the front is held."));
        column.AddChild(UiFactory.Rule(true));

        _resume = UiFactory.Button("PrimaryButton", "Resume", Close);
        column.AddChild(_resume);
        column.AddChild(UiFactory.Button("PaperButton", "Restart Mission", Restart));

        var settings = UiFactory.Button("PaperButton", "Settings", null, "lock");
        settings.Disabled = true;
        settings.TooltipText = "Later in development";
        column.AddChild(settings);
        column.AddChild(UiFactory.Label("PaperSmallLabel", "Settings arrive later in development"));

        column.AddChild(UiFactory.Button("PaperButton", "Abandon Mission", () => _confirmRow.Visible = true));
        _confirmRow = UiFactory.HBox(8);
        _confirmRow.Visible = false;
        var prompt = UiFactory.Label("PaperBodyLabel", "Abandon this mission?");
        prompt.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        prompt.VerticalAlignment = VerticalAlignment.Center;
        _confirmRow.AddChild(prompt);
        _confirmRow.AddChild(UiFactory.Button("PaperButton", "Yes", Abandon));
        _confirmRow.AddChild(UiFactory.Button("PaperButton", "No", () => _confirmRow.Visible = false));
        column.AddChild(_confirmRow);

        column.AddChild(UiFactory.Button("PaperButton", "Quit to Menu", QuitToMenu));

        if (ScreenshotCapture.UiStateIs("pause")) Callable.From(Open).CallDeferred();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape }) return;
        if (IsOpen) Close(); else Open();
        GetViewport().SetInputAsHandled();
    }

    public void Open()
    {
        if (IsOpen) return;
        var time = GameLoop.Instance?.Time;
        _wasPaused = time?.IsPaused ?? false;
        time?.Pause();
        _confirmRow.Visible = false;
        _card.Visible = true;
        UiFactory.FadeIn(_card);
        _resume.GrabFocus();
    }

    public void Close()
    {
        if (!IsOpen) return;
        _card.Visible = false;
        if (!_wasPaused) GameLoop.Instance?.Time.Resume();
    }

    // The TimeController lives on the GameLoop autoload and outlives the
    // mission scene, so every exit resumes it before changing scenes.
    private void Restart() => LeaveTo("res://scenes_root/mission.tscn");
    private void Abandon() => LeaveTo("res://scenes_root/briefing.tscn");
    private void QuitToMenu() => LeaveTo("res://scenes_root/main_menu.tscn");

    private void LeaveTo(string scenePath)
    {
        GameLoop.Instance?.Time.Resume();
        MissionSession.ResetMission();
        GetTree().ChangeSceneToFile(scenePath);
    }
}
