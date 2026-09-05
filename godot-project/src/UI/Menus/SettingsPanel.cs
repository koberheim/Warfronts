using System;
using System.Collections.Generic;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Menus;

// Settings content (GDD §13.8-13.9), shared by two hosts: SettingsController
// (its own flow screen, reachable from the main menu) and PauseMenu (swapped
// in over the pause card, so opening Settings mid-mission never touches
// scene/GameLoop state - see PauseMenu.OpenSettings). fow_theme.tres only
// styles Label/PanelContainer/Button (docs/UI_DESIGN_SPEC.md §7); volumes use
// the custom-drawn PaperSlider (a real continuous drag bar), while every
// other choice here (Display, UI Scale, Colorblind Palette, Visual Effects)
// is a themed toggle-button row - those are categorical choices, not
// continuous ranges, so a slider would not fit them. Not covered here
// because nothing in the game backs them yet: VSync/resolution/frame cap,
// screen shake, subtitles, default game speed, auto-pause-on-wave-complete,
// confirm-before-sell, targeting-priority defaults, tutorial-hints/damage-
// number toggles.
public partial class SettingsPanel : VBoxContainer
{
    private Action _onBack;
    private string _awaitingRebindAction;
    private Button _awaitingRebindButton;
    private Label _conflictLabel;

    public void Setup(Action onBack)
    {
        _onBack = onBack;
        FocusMode = FocusModeEnum.None;
        Populate();
    }

    // Reset Controls needs to refresh every binding row's displayed label;
    // rebuilding is simpler than keeping a live Label reference per row.
    // QueueFree defers removal to end-of-frame, so the repopulate itself
    // must wait a frame too (matches CampaignSelectionController.Rebuild) -
    // otherwise the old and new rows would briefly coexist.
    private void Rebuild()
    {
        foreach (var child in GetChildren()) child.QueueFree();
        Callable.From(Populate).CallDeferred();
    }

    private void Populate()
    {
        AddThemeConstantOverride("separation", 10);
        var settings = UserSettings.Current;

        AddChild(UiFactory.Label("PaperSubheadingLabel", "VIDEO"));
        AddChild(SteppedRow("Display", new[] { "Windowed", "Fullscreen" }, new[] { false, true },
            settings.Fullscreen, value => { UserSettings.Current.Fullscreen = value; ApplyAndSave(); }));
        AddChild(SteppedRow("UI Scale", new[] { "75%", "100%", "125%", "150%" }, new[] { 0.75f, 1f, 1.25f, 1.5f },
            settings.UiScale, value => { UserSettings.Current.UiScale = value; ApplyAndSave(); }));
        AddChild(UiFactory.Rule(true));

        AddChild(UiFactory.Label("PaperSubheadingLabel", "AUDIO"));
        AddChild(VolumeRow("Master", settings.MasterVolume, v => UserSettings.Current.MasterVolume = v));
        AddChild(VolumeRow("Music", settings.MusicVolume, v => UserSettings.Current.MusicVolume = v));
        AddChild(VolumeRow("SFX", settings.SfxVolume, v => UserSettings.Current.SfxVolume = v));
        AddChild(VolumeRow("UI", settings.UiVolume, v => UserSettings.Current.UiVolume = v));
        AddChild(VolumeRow("Radio Chatter", settings.RadioVolume, v => UserSettings.Current.RadioVolume = v));
        AddChild(UiFactory.Rule(true));

        AddChild(UiFactory.Label("PaperSubheadingLabel", "ACCESSIBILITY"));
        AddChild(SteppedRow("Colorblind Palette",
            new[] { "Default", "Protanopia", "Deuteranopia", "Tritanopia" },
            new[] { ColorblindPalette.Default, ColorblindPalette.Protanopia, ColorblindPalette.Deuteranopia, ColorblindPalette.Tritanopia },
            settings.ColorblindPalette, value => { UserSettings.Current.ColorblindPalette = value; ApplyAndSave(); }));
        AddChild(SteppedRow("Visual Effects",
            new[] { "Full", "Reduced", "Minimal" },
            new[] { EffectsIntensity.Full, EffectsIntensity.Reduced, EffectsIntensity.Minimal },
            settings.EffectsIntensity, value => { UserSettings.Current.EffectsIntensity = value; ApplyAndSave(); }));
        AddChild(UiFactory.Rule(true));

        AddChild(UiFactory.Label("PaperSubheadingLabel", "CONTROLS"));
        foreach (var definition in UserSettings.BindingDefinitions)
            AddChild(BindingRow(definition));
        _conflictLabel = UiFactory.Label("PaperSmallLabel", "");
        _conflictLabel.Visible = false;
        AddChild(_conflictLabel);
        AddChild(UiFactory.Button("PaperButton", "Reset Controls to Default", () =>
        {
            UserSettings.ResetBindings();
            ApplyAndSave();
            Rebuild();
        }));

        AddChild(UiFactory.Spacer(0f, 12f));
        var back = UiFactory.Button("PrimaryButton", "Back", () => { ApplyAndSave(); _onBack(); });
        FlowScreen.ActionRow(this, back);
    }

    // Every mutation applies immediately (so Video/Audio/Accessibility
    // changes are visible without leaving the screen) and saves immediately
    // (so a crash or Alt+F4 right after doesn't lose the change).
    private void ApplyAndSave()
    {
        UserSettings.Apply(GetTree());
        ProfileStore.TrySave(out _);
    }

    private Control SteppedRow<T>(string label, string[] optionLabels, T[] values, T current, Action<T> onSelect)
    {
        var row = UiFactory.HBox(8);
        row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2(190f, 0f), VerticalAlignment = VerticalAlignment.Center });
        var group = new ButtonGroup();
        var comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < optionLabels.Length; i++)
        {
            var value = values[i];
            var button = UiFactory.Button("PaperButton", optionLabels[i], () => onSelect(value));
            button.ToggleMode = true;
            button.ButtonGroup = group;
            button.CustomMinimumSize = new Vector2(0f, 36f);
            button.SetPressedNoSignal(comparer.Equals(value, current));
            row.AddChild(button);
        }
        return row;
    }

    // A real draggable slider (0-100%, continuous) rather than the stepped
    // rows above - volumes are the one setting on this screen the User
    // specifically asked to be a slider bar. Applies live while dragging
    // (cheap: just an in-memory AudioServer bus volume) but only writes the
    // profile to disk once the drag ends, so scrubbing the bar doesn't spam
    // the save file on every mouse-motion event.
    private Control VolumeRow(string label, float current, Action<float> onChange)
    {
        var row = UiFactory.HBox(10);
        row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2(190f, 0f), VerticalAlignment = VerticalAlignment.Center });
        var slider = new PaperSlider { Value = current, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        var valueLabel = new Label
        {
            Text = $"{Mathf.RoundToInt(current * 100f)}%",
            CustomMinimumSize = new Vector2(52f, 0f),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };
        slider.ValueChanged += value =>
        {
            valueLabel.Text = $"{Mathf.RoundToInt(value * 100f)}%";
            onChange(value);
            UserSettings.Apply(GetTree());
        };
        slider.DragEnded += () => ProfileStore.TrySave(out _);
        row.AddChild(slider);
        row.AddChild(valueLabel);
        return row;
    }

    private Control BindingRow(UserSettings.BindingDefinition definition)
    {
        var row = UiFactory.HBox(8);
        row.AddChild(new Label { Text = definition.Label, CustomMinimumSize = new Vector2(190f, 0f), VerticalAlignment = VerticalAlignment.Center });
        var rebind = UiFactory.Button("PaperButton", UserSettings.BindingLabel(definition.Action));
        rebind.CustomMinimumSize = new Vector2(150f, 36f);
        rebind.Pressed += () => BeginRebind(definition.Action, rebind);
        row.AddChild(rebind);
        return row;
    }

    private void BeginRebind(string action, Button button)
    {
        _awaitingRebindAction = action;
        _awaitingRebindButton = button;
        button.Text = "Press a key...";
        _conflictLabel.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_awaitingRebindAction == null) return;

        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            _awaitingRebindButton.Text = UserSettings.BindingLabel(_awaitingRebindAction);
            _awaitingRebindAction = null;
            _awaitingRebindButton = null;
            GetViewport().SetInputAsHandled();
            return;
        }

        InputBindingData binding = @event switch
        {
            InputEventKey { Pressed: true, Echo: false } key => new InputBindingData { Device = "key", Code = (int)key.Keycode },
            InputEventMouseButton { Pressed: true } mouse => new InputBindingData { Device = "mouse", Code = (int)mouse.ButtonIndex },
            _ => null,
        };
        if (binding == null) return;

        string action = _awaitingRebindAction;
        var button = _awaitingRebindButton;
        _awaitingRebindAction = null;
        _awaitingRebindButton = null;

        if (UserSettings.TrySetBinding(action, binding, out string conflict))
        {
            button.Text = UserSettings.BindingLabel(action);
            ApplyAndSave();
        }
        else
        {
            button.Text = UserSettings.BindingLabel(action);
            _conflictLabel.Text = string.IsNullOrEmpty(conflict)
                ? "That input can't be bound here."
                : $"Already bound to: {conflict}";
            _conflictLabel.Visible = true;
        }
        GetViewport().SetInputAsHandled();
    }
}
