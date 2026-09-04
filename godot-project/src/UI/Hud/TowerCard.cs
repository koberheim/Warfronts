using Godot;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Hud;

// One build-bar card (docs/UI_DESIGN_SPEC.md §8.4 E): hotkey plate, tower
// glyph, name, cost - or "Need +NN" when unaffordable - on a CardButton
// whose toggled state is the amber "selected for placement" look. The
// overlay never takes the mouse, so hover/press styling stays the button's.
public partial class TowerCard : Button
{
    public TowerDefinition Definition { get; private set; }

    private Control _overlay;
    private HBoxContainer _costRow;
    private Label _costLabel;
    private Label _needLabel;

    public void Setup(TowerDefinition definition, Key hotkey)
    {
        Definition = definition;
        ThemeTypeVariation = "CardButton";
        ToggleMode = true;
        FocusMode = FocusModeEnum.None;
        CustomMinimumSize = new Vector2(120f, 128f);
        TooltipText = BuildTooltip(definition, hotkey);

        var margin = UiFactory.Margin(6, 6, 6, 6);
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(margin);
        _overlay = margin;

        var column = UiFactory.VBox(2);
        margin.AddChild(column);

        column.AddChild(UiFactory.Chip(hotkey.ToString(), this));

        var icon = UiFactory.Icon(UiIcons.ForTower(definition.Id), 40);
        if (icon != null)
        {
            icon.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            column.AddChild(icon);
        }
        else
        {
            column.AddChild(UiFactory.Label("SubheadingLabel", Initials(definition.DisplayName), HorizontalAlignment.Center));
        }

        var name = UiFactory.Wrapped("SmallLabel", definition.DisplayName);
        name.HorizontalAlignment = HorizontalAlignment.Center;
        name.AddThemeColorOverride("font_color", UiPalette.Cream);
        column.AddChild(name);

        _costRow = UiFactory.HBox(4);
        _costRow.Alignment = BoxContainer.AlignmentMode.Center;
        var supplyIcon = UiFactory.Icon("resource_supply", 14);
        if (supplyIcon != null) _costRow.AddChild(supplyIcon);
        _costLabel = UiFactory.Label("BodyLabel", "0");
        _costRow.AddChild(_costLabel);
        column.AddChild(_costRow);

        _needLabel = UiFactory.Label("SmallLabel", "", HorizontalAlignment.Center);
        _needLabel.Visible = false;
        column.AddChild(_needLabel);

        IgnoreMouse(margin);
    }

    public void SetState(int cost, int shortfall, bool selected)
    {
        bool unaffordable = shortfall > 0;
        Disabled = unaffordable;
        _costLabel.Text = cost.ToString();
        _costRow.Visible = !unaffordable;
        _needLabel.Visible = unaffordable;
        if (unaffordable) _needLabel.Text = $"Need +{shortfall}";
        if (ButtonPressed != selected) SetPressedNoSignal(selected);
        _overlay.Modulate = _overlay.Modulate with { A = unaffordable ? 0.55f : 1f };
    }

    private static string BuildTooltip(TowerDefinition definition, Key hotkey)
    {
        var stats = definition.PreForkStatsForLevel(1);
        return $"{definition.DisplayName}  [{hotkey}]\n" +
               $"{MatchupRules.DamageTypeName(definition.DamageType)} · range {stats.RangeTiles:0.0} tiles\n" +
               $"Strong vs {MatchupRules.StrongVsText(definition.DamageType)}\n" +
               $"Weak vs {MatchupRules.WeakVsText(definition.DamageType)}";
    }

    private static string Initials(string name)
    {
        var parts = name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}" : name.Length > 0 ? name[..1] : "?";
    }

    // Everything drawn over the button must let the mouse through, or the
    // button never sees hover/press.
    internal static void IgnoreMouse(Control root)
    {
        root.MouseFilter = MouseFilterEnum.Ignore;
        foreach (var child in root.GetChildren())
            if (child is Control control) IgnoreMouse(control);
    }
}
