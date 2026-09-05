using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Menus;

// The pause menu (docs/UI_DESIGN_SPEC.md §8.7; GDD §13.7): a paper card
// with Resume (focused), Restart, Settings (D80: a second embedded card,
// not a scene change), Abandon (inline confirm) and Quit to Menu. Holds the
// simulation while open and gives the
// previous pause state back on Resume, so a menu opened over the tutorial's
// pause does not un-pause the tutorial. Only the card blocks the mouse: the
// HUD underneath keeps working, matching "building stays available".
public partial class PauseMenu : Control
{
    private PanelContainer _card;
    private PanelContainer _settingsCard;
    private Button _resume;
    private HBoxContainer _confirmRow;
    private bool _wasPaused;

    public bool IsOpen => _card.Visible || _settingsCard.Visible;

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

        column.AddChild(UiFactory.Button("PaperButton", "Settings", OpenSettings));

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

        // A second card swapped in over the first (never a scene change) -
        // Settings mid-mission must not touch GameLoop/MissionSession state.
        _settingsCard = UiFactory.Panel("PaperPanel");
        _settingsCard.CustomMinimumSize = new Vector2(640f, 760f);
        _settingsCard.Visible = false;
        AddChild(_settingsCard);
        UiFactory.Anchor(_settingsCard, LayoutPreset.Center, 0, 0);
        var settingsColumn = UiFactory.VBox(10);
        _settingsCard.AddChild(settingsColumn);
        settingsColumn.AddChild(UiFactory.Label("StampLabel", "SETTINGS", uppercase: true));
        settingsColumn.AddChild(UiFactory.Rule(true));
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        settingsColumn.AddChild(scroll);
        var settingsPanel = new SettingsPanel { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scroll.AddChild(settingsPanel);
        settingsPanel.Setup(CloseSettings);

        if (ScreenshotCapture.UiStateIs("pause")) Callable.From(Open).CallDeferred();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape }) return;
        if (_settingsCard.Visible) { CloseSettings(); GetViewport().SetInputAsHandled(); return; }
        if (IsOpen) Close(); else Open();
        GetViewport().SetInputAsHandled();
    }

    private void OpenSettings()
    {
        _card.Visible = false;
        _settingsCard.Visible = true;
        UiFactory.FadeIn(_settingsCard);
    }

    private void CloseSettings()
    {
        _settingsCard.Visible = false;
        _card.Visible = true;
        _resume.GrabFocus();
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
