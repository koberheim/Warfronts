using Godot;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Hud;

// One ability card (docs/UI_DESIGN_SPEC.md §8.4 G; GDD §7.6): key plate,
// glyph with a radial cooldown sweep and the seconds left drawn over it,
// name, and the CP cost badge. Shared by the three universal abilities and
// the doctrine's fourth slot so all four read the same way.
public partial class AbilityCard : Button
{
    private Control _overlay;
    private CooldownRing _sweep;
    private Label _cooldownLabel;
    private Label _cpLabel;
    private TextureRect _cpIcon;
    private int _cpCost;

    // The sim that knows the cost is built after the HUD (child-before-parent
    // _Ready), so the badge is corrected on the first refresh.
    public void SetCpCost(int cpCost)
    {
        if (cpCost == _cpCost) return;
        _cpCost = cpCost;
        _cpLabel.Text = $"{cpCost} CP";
    }

    public void Setup(string hotkey, string iconId, string name, int cpCost)
    {
        ThemeTypeVariation = "CardButton";
        ToggleMode = true;
        FocusMode = FocusModeEnum.None;
        CustomMinimumSize = new Vector2(104f, 140f);
        TooltipText = $"{name}  [{hotkey}]";

        var margin = UiFactory.Margin(6, 6, 6, 6);
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(margin);
        _overlay = margin;

        var column = UiFactory.VBox(2);
        margin.AddChild(column);
        column.AddChild(UiFactory.Chip(hotkey, this));

        var iconBox = new Control { CustomMinimumSize = new Vector2(52f, 52f), SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        column.AddChild(iconBox);
        var icon = UiFactory.Icon(iconId, 52);
        if (icon != null)
        {
            icon.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            iconBox.AddChild(icon);
        }
        _sweep = new CooldownRing { Mode = CooldownRing.Style.Pie, SweepColor = UiPalette.Slate with { A = 0.7f } };
        _sweep.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        iconBox.AddChild(_sweep);
        _cooldownLabel = UiFactory.Label("BodyLabel", "", HorizontalAlignment.Center);
        _cooldownLabel.VerticalAlignment = VerticalAlignment.Center;
        _cooldownLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _cooldownLabel.AddThemeColorOverride("font_outline_color", UiPalette.Slate);
        _cooldownLabel.AddThemeConstantOverride("outline_size", 4);
        iconBox.AddChild(_cooldownLabel);

        var nameLabel = UiFactory.Wrapped("SmallLabel", name);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeColorOverride("font_color", UiPalette.Cream);
        column.AddChild(nameLabel);

        var cpRow = UiFactory.HBox(4);
        cpRow.Alignment = BoxContainer.AlignmentMode.Center;
        _cpIcon = UiFactory.Icon("resource_cp", 12);
        if (_cpIcon != null) cpRow.AddChild(_cpIcon);
        _cpLabel = UiFactory.Label("SmallLabel", $"{cpCost} CP");
        _cpCost = cpCost;
        cpRow.AddChild(_cpLabel);
        column.AddChild(cpRow);

        TowerCard.IgnoreMouse(margin);
    }

    public void SetState(float cooldownRemaining, float cooldownTotal, bool affordable, bool selected, bool exhausted)
    {
        bool cooling = cooldownRemaining > 0f;
        Disabled = cooling || exhausted;
        _sweep.SetFraction(cooling && cooldownTotal > 0f ? cooldownRemaining / cooldownTotal : 0f);

        string text = exhausted ? "USED" : cooling ? Mathf.CeilToInt(cooldownRemaining).ToString() : "";
        if (_cooldownLabel.Text != text) _cooldownLabel.Text = text;

        if (ButtonPressed != selected) SetPressedNoSignal(selected);

        // Shortfall is shown as words in the status line on click (GDD §7.6);
        // the badge only tints so the state is visible before clicking.
        var badge = affordable ? UiPalette.Cream : UiPalette.Red;
        _cpLabel.AddThemeColorOverride("font_color", badge);
        if (_cpIcon != null) _cpIcon.Modulate = badge;
        _overlay.Modulate = _overlay.Modulate with { A = Disabled ? 0.55f : 1f };
    }
}
