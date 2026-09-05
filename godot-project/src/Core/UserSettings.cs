using System;
using System.Collections.Generic;
using Godot;
using FrontsOfWar.Meta;

namespace FrontsOfWar.Core;

// Runtime application boundary for the profile-backed settings. Input actions
// live here so all consumers share the same remappable names instead of
// comparing hard-coded keys independently.
public static class UserSettings
{
    public readonly record struct BindingDefinition(string Action, string Label, InputBindingData DefaultBinding);

    public const string PauseAction = "fow_pause";
    public const string CancelAction = "fow_cancel";
    public const string CycleSpeedAction = "fow_cycle_speed";
    public const string IncreaseSpeedAction = "fow_increase_speed";
    public const string DecreaseSpeedAction = "fow_decrease_speed";

    public static string BuildAction(int slot) => $"fow_build_{slot}";
    public static string AbilityAction(int slot) => $"fow_ability_{slot}";

    public static readonly IReadOnlyList<BindingDefinition> BindingDefinitions = new[]
    {
        new BindingDefinition(PauseAction, "Pause", KeyBinding(Key.P)),
        new BindingDefinition(CancelAction, "Cancel targeting", KeyBinding(Key.Escape)),
        new BindingDefinition(CycleSpeedAction, "Cycle speed", KeyBinding(Key.Space)),
        new BindingDefinition(IncreaseSpeedAction, "Increase speed", KeyBinding(Key.Equal)),
        new BindingDefinition(DecreaseSpeedAction, "Decrease speed", KeyBinding(Key.Minus)),
        new BindingDefinition(BuildAction(1), "Build slot 1", KeyBinding(Key.Q)),
        new BindingDefinition(BuildAction(2), "Build slot 2", KeyBinding(Key.W)),
        new BindingDefinition(BuildAction(3), "Build slot 3", KeyBinding(Key.E)),
        new BindingDefinition(BuildAction(4), "Build slot 4", KeyBinding(Key.R)),
        new BindingDefinition(BuildAction(5), "Build slot 5", KeyBinding(Key.T)),
        new BindingDefinition(BuildAction(6), "Build slot 6", KeyBinding(Key.Y)),
        new BindingDefinition(AbilityAction(1), "Ability 1", KeyBinding(Key.Key1)),
        new BindingDefinition(AbilityAction(2), "Ability 2", KeyBinding(Key.Key2)),
        new BindingDefinition(AbilityAction(3), "Ability 3", KeyBinding(Key.Key3)),
        new BindingDefinition(AbilityAction(4), "Ability 4", KeyBinding(Key.Key4)),
    };

    public static PlayerSettings Current
    {
        get
        {
            var profile = ProfileStore.Current;
            profile.Settings ??= new PlayerSettings();
            profile.Settings.Normalize();
            EnsureDefaultBindings(profile.Settings);
            return profile.Settings;
        }
    }

    public static void Apply(SceneTree tree = null)
    {
        var settings = Current;
        ApplyInputMap(settings);
        ApplyAudio(settings);
        if (tree?.Root == null || DisplayServer.GetName() == "headless") return;

        DisplayServer.WindowSetMode(settings.Fullscreen
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
        tree.Root.ContentScaleFactor = settings.UiScale;
        ApplyColorblindTheme(tree, settings.ColorblindPalette);
    }

    public static bool TrySetBinding(string action, InputBindingData binding, out string conflictingAction)
    {
        conflictingAction = "";
        if (binding == null || (binding.Device != "key" && binding.Device != "mouse")) return false;
        foreach (var definition in BindingDefinitions)
        {
            if (definition.Action == action) continue;
            if (Current.InputBindings.TryGetValue(definition.Action, out var existing) && existing.SameAs(binding))
            {
                conflictingAction = definition.Label;
                return false;
            }
        }
        Current.InputBindings[action] = binding;
        ApplyInputMap(Current);
        return true;
    }

    public static void ResetBindings()
    {
        Current.InputBindings.Clear();
        EnsureDefaultBindings(Current);
        ApplyInputMap(Current);
    }

    public static string BindingLabel(string action)
    {
        if (!Current.InputBindings.TryGetValue(action, out var binding)) return "Unbound";
        return binding.Device == "mouse" ? ((MouseButton)binding.Code).ToString() : ((Key)binding.Code).ToString();
    }

    public static bool IsActionPressed(InputEvent input, string action)
        => input.IsActionPressed(action, allowEcho: false, exactMatch: true);

    public static bool IsReducedMotion => Current.ReducedMotion || Current.EffectsIntensity != EffectsIntensity.Full;

    private static InputBindingData KeyBinding(Key key) => new() { Device = "key", Code = (int)key };

    private static void EnsureDefaultBindings(PlayerSettings settings)
    {
        foreach (var definition in BindingDefinitions)
            settings.InputBindings.TryAdd(definition.Action, new InputBindingData
            {
                Device = definition.DefaultBinding.Device,
                Code = definition.DefaultBinding.Code,
            });
    }

    private static void ApplyInputMap(PlayerSettings settings)
    {
        foreach (var definition in BindingDefinitions)
        {
            if (!InputMap.HasAction(definition.Action)) InputMap.AddAction(definition.Action);
            InputMap.ActionEraseEvents(definition.Action);
            if (!settings.InputBindings.TryGetValue(definition.Action, out var binding)) continue;
            InputMap.ActionAddEvent(definition.Action, ToInputEvent(binding));
        }
    }

    private static InputEvent ToInputEvent(InputBindingData binding) => binding.Device == "mouse"
        ? new InputEventMouseButton { ButtonIndex = (MouseButton)binding.Code }
        : new InputEventKey { Keycode = (Key)binding.Code };

    private static void ApplyAudio(PlayerSettings settings)
    {
        SetBusVolume("Master", settings.MasterVolume);
        SetBusVolume("Music", settings.MusicVolume);
        SetBusVolume("SFX", settings.SfxVolume);
        SetBusVolume("UI", settings.UiVolume);
        SetBusVolume("Radio", settings.RadioVolume);
    }

    public static bool HasAudioBus(string bus) => AudioServer.GetBusIndex(bus) >= 0;

    private static void SetBusVolume(string bus, float level)
    {
        int index = AudioServer.GetBusIndex(bus);
        if (index >= 0) AudioServer.SetBusVolumeDb(index, Mathf.LinearToDb(level));
    }

    private static void ApplyColorblindTheme(SceneTree tree, ColorblindPalette palette)
    {
        var source = GD.Load<Theme>("res://assets/ui/theme/fow_theme.tres");
        if (source == null) return;
        var theme = (Theme)source.Duplicate();
        Color accent = palette switch
        {
            ColorblindPalette.Protanopia => new Color("#4AA3DF"),
            ColorblindPalette.Deuteranopia => new Color("#E69F00"),
            ColorblindPalette.Tritanopia => new Color("#CC79A7"),
            _ => new Color("#E0A83A"),
        };
        theme.SetColor("font_pressed_color", "Button", accent);
        theme.SetColor("font_pressed_color", "PrimaryButton", accent);
        theme.SetColor("font_pressed_color", "PaperButton", accent);
        theme.SetColor("font_color", "StampLabel", accent);
        tree.Root.Set("theme", theme);
    }
}
