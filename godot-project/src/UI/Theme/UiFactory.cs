using Godot;
using System;

namespace FrontsOfWar.UI.Theme;

// Small constructors for the theme's component library (docs/UI_DESIGN_SPEC
// .md §7). Screens compose these instead of building ad-hoc styles, so every
// label, button and panel resolves its look from fow_theme.tres by type
// variation name. Nothing here holds state or touches gameplay.
public static class UiFactory
{
    public static Label Label(string variation, string text = "", HorizontalAlignment align = HorizontalAlignment.Left, bool uppercase = false)
    {
        var label = new Label { Text = text, HorizontalAlignment = align, Uppercase = uppercase };
        if (!string.IsNullOrEmpty(variation)) label.ThemeTypeVariation = variation;
        return label;
    }

    public static Label Wrapped(string variation, string text, float minWidth = 0f)
    {
        var label = Label(variation, text);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        if (minWidth > 0f) label.CustomMinimumSize = new Vector2(minWidth, 0f);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        return label;
    }

    // Null when the glyph file does not exist yet, so callers can fall back
    // to text (the registry's contract - see UiIcons).
    public static TextureRect Icon(string id, int size, Color? tint = null)
    {
        var texture = UiIcons.Get(id);
        if (texture == null) return null;
        return new TextureRect
        {
            Texture = texture,
            CustomMinimumSize = new Vector2(size, size),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter = CanvasItem.TextureFilterEnum.LinearWithMipmaps,
            Modulate = tint ?? UiPalette.Cream,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
        };
    }

    // Icon + label pair; the icon is skipped silently when missing.
    public static HBoxContainer IconLabel(string iconId, int iconSize, Color tint, Label label, int separation = 6)
    {
        var row = HBox(separation);
        var icon = Icon(iconId, iconSize, tint);
        if (icon != null) row.AddChild(icon);
        row.AddChild(label);
        return row;
    }

    public static Button Button(string variation, string text, Action onPressed = null, string iconId = null)
    {
        var button = new Button { Text = text };
        if (!string.IsNullOrEmpty(variation)) button.ThemeTypeVariation = variation;
        if (iconId != null) button.Icon = UiIcons.Get(iconId);
        if (onPressed != null) button.Pressed += onPressed;
        return button;
    }

    public static PanelContainer Panel(string variation)
    {
        var panel = new PanelContainer();
        if (!string.IsNullOrEmpty(variation)) panel.ThemeTypeVariation = variation;
        return panel;
    }

    public static VBoxContainer VBox(int separation = 8)
    {
        var box = new VBoxContainer();
        box.AddThemeConstantOverride("separation", separation);
        return box;
    }

    public static HBoxContainer HBox(int separation = 8)
    {
        var box = new HBoxContainer();
        box.AddThemeConstantOverride("separation", separation);
        return box;
    }

    public static MarginContainer Margin(int left, int top, int right, int bottom)
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", left);
        margin.AddThemeConstantOverride("margin_top", top);
        margin.AddThemeConstantOverride("margin_right", right);
        margin.AddThemeConstantOverride("margin_bottom", bottom);
        return margin;
    }

    // Hotkey plate for cards (spec §8.4): Oswald caption on a slate chip,
    // derived from the theme's SlotPanel so the chip follows any theme swap.
    public static Label Chip(string text, Control themeSource)
    {
        var chip = Label("CaptionLabel", text, HorizontalAlignment.Center);
        chip.AddThemeColorOverride("font_color", UiPalette.Cream);
        if (themeSource.GetThemeStylebox("panel", "SlotPanel") is StyleBoxFlat slot)
        {
            var style = (StyleBoxFlat)slot.Duplicate();
            style.ContentMarginLeft = 6f;
            style.ContentMarginRight = 6f;
            style.ContentMarginTop = 1f;
            style.ContentMarginBottom = 1f;
            chip.AddThemeStyleboxOverride("normal", style);
        }
        chip.MouseFilter = Control.MouseFilterEnum.Ignore;
        chip.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
        return chip;
    }

    public static Control Spacer(float width = 0f, float height = 0f, bool expand = false)
    {
        var spacer = new Control { CustomMinimumSize = new Vector2(width, height), MouseFilter = Control.MouseFilterEnum.Ignore };
        if (expand)
        {
            spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            spacer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        }
        return spacer;
    }

    public static HSeparator Rule(bool paper)
    {
        var rule = new HSeparator();
        if (paper) rule.ThemeTypeVariation = "PaperSeparator";
        return rule;
    }

    // Anchors a control to a screen edge/corner using the layout presets so
    // it survives a wider-than-16:9 window (spec §2: never absolute
    // coordinates for chrome). Margins are in 1080p pixels.
    public static void Anchor(Control control, Control.LayoutPreset preset, int marginX = 24, int marginY = 16)
    {
        bool left = preset is Control.LayoutPreset.TopLeft or Control.LayoutPreset.BottomLeft or Control.LayoutPreset.CenterLeft;
        bool right = preset is Control.LayoutPreset.TopRight or Control.LayoutPreset.BottomRight or Control.LayoutPreset.CenterRight;
        bool top = preset is Control.LayoutPreset.TopLeft or Control.LayoutPreset.TopRight or Control.LayoutPreset.CenterTop;
        bool bottom = preset is Control.LayoutPreset.BottomLeft or Control.LayoutPreset.BottomRight or Control.LayoutPreset.CenterBottom;

        control.SetAnchorsPreset(preset);
        float x = left ? marginX : right ? -marginX : 0f;
        float y = top ? marginY : bottom ? -marginY : 0f;
        control.OffsetLeft = x;
        control.OffsetRight = x;
        control.OffsetTop = y;
        control.OffsetBottom = y;
        control.GrowHorizontal = right ? Control.GrowDirection.Begin : left ? Control.GrowDirection.End : Control.GrowDirection.Both;
        control.GrowVertical = bottom ? Control.GrowDirection.Begin : top ? Control.GrowDirection.End : Control.GrowDirection.Both;
    }

    // Motion per spec §10: panels fade in over 150 ms, values pulse once
    // over 180 ms. Tweens run on the render clock, so they animate while the
    // simulation is paused (the pause menu fades in while the game is held).
    public static void FadeIn(CanvasItem item, float seconds = 0.15f)
    {
        item.Modulate = item.Modulate with { A = 0f };
        item.CreateTween().TweenProperty(item, "modulate:a", 1f, seconds);
    }

    public static void Pulse(CanvasItem item, float seconds = 0.18f)
    {
        var tween = item.CreateTween();
        tween.TweenProperty(item, "modulate:a", 0.6f, seconds * 0.5f);
        tween.TweenProperty(item, "modulate:a", 1f, seconds * 0.5f);
    }

    public static string Roman(int value) => value switch
    {
        1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V", 6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", 10 => "X",
        _ => value.ToString(),
    };
}
