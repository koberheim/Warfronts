using Godot;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Hud;

// HUD zone A (docs/UI_DESIGN_SPEC.md §2.1, §8.4): Supply with its projected
// per-wave income, Command Points, and the segmented Defense Line bar. Pulses
// once when any value drops so a hit or a spend registers without a flash.
public partial class ResourcePanel : PanelContainer
{
    private Label _supplyValue;
    private Label _supplyIncome;
    private Label _cpValue;
    private Label _defenseValue;
    private SegmentedBar _defenseBar;
    private int _lastSupply = int.MinValue;
    private int _lastCp = int.MinValue;
    private int _lastDefense = int.MinValue;

    public override void _Ready()
    {
        ThemeTypeVariation = "SlatePanel";
        var column = UiFactory.VBox(6);
        AddChild(column);

        column.AddChild(Row("resource_supply", out _supplyValue, out _supplyIncome, "Supply"));
        column.AddChild(Row("resource_cp", out _cpValue, out _, "Command Points"));

        var defenseRow = UiFactory.HBox(8);
        var defenseIcon = UiFactory.Icon("resource_defense_line", 24);
        if (defenseIcon != null) defenseRow.AddChild(defenseIcon);
        _defenseBar = new SegmentedBar { CustomMinimumSize = new Vector2(240f, 14f), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        defenseRow.AddChild(_defenseBar);
        _defenseValue = UiFactory.Label("SmallLabel", "");
        _defenseValue.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        defenseRow.AddChild(_defenseValue);
        column.AddChild(defenseRow);
    }

    private static HBoxContainer Row(string iconId, out Label value, out Label caption, string captionText)
    {
        var row = UiFactory.HBox(8);
        var icon = UiFactory.Icon(iconId, 24);
        if (icon != null) row.AddChild(icon);
        value = UiFactory.Label("NumberLabel", "0");
        value.CustomMinimumSize = new Vector2(72f, 0f);
        row.AddChild(value);
        caption = UiFactory.Label("SmallLabel", captionText);
        caption.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        row.AddChild(caption);
        return row;
    }

    public void SetSupply(int balance, int projectedIncomePerWave)
    {
        if (_lastSupply != int.MinValue && balance < _lastSupply) UiFactory.Pulse(_supplyValue);
        _lastSupply = balance;
        _supplyValue.Text = balance.ToString();
        _supplyIncome.Text = $"+{projectedIncomePerWave} / wave";
    }

    public void SetCommandPoints(int balance)
    {
        if (_lastCp != int.MinValue && balance < _lastCp) UiFactory.Pulse(_cpValue);
        _lastCp = balance;
        _cpValue.Text = balance.ToString();
    }

    public void SetDefenseLine(int integrity, int max)
    {
        if (_lastDefense != int.MinValue && integrity < _lastDefense) UiFactory.Pulse(this);
        _lastDefense = integrity;
        _defenseBar.SetValue(integrity, max);
        _defenseValue.Text = $"{integrity} / {max}";
    }
}
