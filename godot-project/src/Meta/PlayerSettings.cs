using System;
using System.Collections.Generic;

namespace FrontsOfWar.Meta;

public enum ColorblindPalette { Default, Protanopia, Deuteranopia, Tritanopia }
public enum EffectsIntensity { Full, Reduced, Minimal }

// JSON-friendly representation of a single Godot key or mouse binding.
public sealed class InputBindingData
{
    public string Device { get; set; } = "key";
    public int Code { get; set; }

    public bool SameAs(InputBindingData other)
        => other != null && Device == other.Device && Code == other.Code;
}

// Stored inside PlayerProfile so settings travel with the game's existing
// versioned, atomic profile save instead of becoming a second fragile file.
public sealed class PlayerSettings
{
    // Defaults true to match project.godot's window/size/mode=3 default
    // (D-"Launch fullscreen at 1080p by default") - UserSettings.Apply now
    // runs at boot, so a false default here would silently flip every fresh
    // profile to windowed mode the moment this field started being read.
    public bool Fullscreen { get; set; } = true;
    public float UiScale { get; set; } = 1f;
    public float MasterVolume { get; set; } = 1f;
    public float MusicVolume { get; set; } = 1f;
    public float SfxVolume { get; set; } = 1f;
    public float UiVolume { get; set; } = 1f;
    public float RadioVolume { get; set; } = 1f;
    public bool ReducedMotion { get; set; }
    public EffectsIntensity EffectsIntensity { get; set; } = EffectsIntensity.Full;
    public ColorblindPalette ColorblindPalette { get; set; } = ColorblindPalette.Default;
    public Dictionary<string, InputBindingData> InputBindings { get; set; } = new();

    public void Normalize()
    {
        UiScale = Math.Clamp(UiScale, 0.75f, 1.5f);
        MasterVolume = Math.Clamp(MasterVolume, 0f, 1f);
        MusicVolume = Math.Clamp(MusicVolume, 0f, 1f);
        SfxVolume = Math.Clamp(SfxVolume, 0f, 1f);
        UiVolume = Math.Clamp(UiVolume, 0f, 1f);
        RadioVolume = Math.Clamp(RadioVolume, 0f, 1f);
        InputBindings ??= new Dictionary<string, InputBindingData>();
    }
}
