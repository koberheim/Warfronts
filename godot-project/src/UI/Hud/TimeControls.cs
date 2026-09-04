using Godot;
using System;
using System.Collections.Generic;
using FrontsOfWar.Core;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Hud;

// HUD zone C (docs/UI_DESIGN_SPEC.md §8.4; GDD §7.7): the brass speed lever
// as three toggle segments, a pause button and the menu button. Owns the
// time hotkeys - Space cycles speed, +/- step it, P pauses - and never
// touches Engine.TimeScale (the TimeController does the real work).
public partial class TimeControls : HBoxContainer
{
    private readonly Dictionary<TimeController.Speed, Button> _segments = new();
    private Button _pauseButton;
    private bool _shownPaused;

    public event Action MenuRequested;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 8);

        var plate = new PanelContainer();
        if (GetThemeStylebox("normal", "PrimaryButton") is StyleBoxTexture brass)
        {
            var style = (StyleBoxTexture)brass.Duplicate();
            style.ContentMarginLeft = 6f;
            style.ContentMarginRight = 6f;
            style.ContentMarginTop = 5f;
            style.ContentMarginBottom = 5f;
            plate.AddThemeStyleboxOverride("panel", style);
        }
        plate.TooltipText = "Game speed (Space cycles, + / - step)";
        AddChild(plate);

        var segments = UiFactory.HBox(4);
        plate.AddChild(segments);
        var group = new ButtonGroup();
        foreach (var speed in new[] { TimeController.Speed.Normal, TimeController.Speed.Fast, TimeController.Speed.Fastest })
        {
            var button = UiFactory.Button("CardButton", $"{(int)speed}×");
            button.ToggleMode = true;
            button.ButtonGroup = group;
            button.FocusMode = FocusModeEnum.None;
            button.CustomMinimumSize = new Vector2(44f, 36f);
            var captured = speed;
            button.Pressed += () => SetSpeed(captured);
            segments.AddChild(button);
            _segments[speed] = button;
        }

        _pauseButton = IconButton("pause", "Pause (P)", TogglePause);
        AddChild(_pauseButton);
        AddChild(IconButton("menu", "Menu (Esc)", () => MenuRequested?.Invoke()));
        Refresh();
    }

    private static Button IconButton(string iconId, string tooltip, Action onPressed)
    {
        var button = UiFactory.Button(null, "", onPressed, iconId);
        button.CustomMinimumSize = new Vector2(44f, 40f);
        button.FocusMode = FocusModeEnum.None;
        button.IconAlignment = HorizontalAlignment.Center;
        button.TooltipText = tooltip;
        if (button.Icon == null) button.Text = tooltip.Split(' ')[0];
        return button;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        var time = GameLoop.Instance?.Time;
        if (time == null) return;

        switch (key.Keycode)
        {
            case Key.Space: time.CycleSpeed(); break;
            case Key.P: time.TogglePause(); break;
            case Key.Plus or Key.Equal or Key.KpAdd: StepSpeed(time, +1); break;
            case Key.Minus or Key.KpSubtract: StepSpeed(time, -1); break;
            default: return;
        }
        Refresh();
        GetViewport().SetInputAsHandled();
    }

    private static void StepSpeed(TimeController time, int delta)
        => time.SetSpeed((TimeController.Speed)Mathf.Clamp((int)time.CurrentSpeed + delta, 1, 3));

    private void SetSpeed(TimeController.Speed speed)
    {
        GameLoop.Instance?.Time.SetSpeed(speed);
        Refresh();
    }

    private void TogglePause()
    {
        GameLoop.Instance?.Time.TogglePause();
        Refresh();
    }

    // Cheap enough to call every frame: only writes when the state changed,
    // so the HUD follows pauses/speed changes made by keys or the tutorial.
    public void Refresh()
    {
        var time = GameLoop.Instance?.Time;
        if (time == null) return;

        foreach (var (speed, button) in _segments)
        {
            bool active = speed == time.CurrentSpeed;
            if (button.ButtonPressed != active) button.SetPressedNoSignal(active);
        }

        if (_shownPaused == time.IsPaused && _pauseButton.Icon != null) return;
        _shownPaused = time.IsPaused;
        var icon = UiIcons.Get(time.IsPaused ? "play" : "pause");
        if (icon != null) _pauseButton.Icon = icon;
        else _pauseButton.Text = time.IsPaused ? "Play" : "Pause";
        _pauseButton.TooltipText = time.IsPaused ? "Resume (P)" : "Pause (P)";
    }
}
