using Godot;
using System;
using FrontsOfWar.Combat;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Panels;

// The paper card itself (docs/UI_DESIGN_SPEC.md §8.5; GDD §13.5): header
// with level pips, stats, matchup glyph rows, attribution, and the upgrade
// (with diff preview) / branch / sell actions. Pure presentation: the panel
// builds an InspectionView from the sim and wires the action events.
public partial class TowerInspectionCard : PanelContainer
{
    public sealed class InspectionView
    {
        public string Name;
        public string IconId;
        public int Level;
        public string BranchName;
        public bool IsCommandPost;
        public DamageType DamageType;
        public TowerStatBlock Stats;
        public float LifetimeDamage;
        public float DamagePerSupply;
        public bool Suppressed;
        public bool CanUpgrade;
        public bool AtFork;
        public int UpgradeCost;
        public TowerStatBlock UpgradePreview;
        public string BranchAName;
        public int BranchACost;
        public TowerStatBlock BranchAPreview;
        public string BranchBName;
        public int BranchBCost;
        public TowerStatBlock BranchBPreview;
        public int SellRefund;
        public int SupplyBalance;
    }

    public event Action UpgradeRequested;
    public event Action<TowerBranchChoice> BranchRequested;
    public event Action SellRequested;
    public event Action CloseRequested;

    private Control _iconSlot;
    private Label _name;
    private readonly TextureRect[] _pips = new TextureRect[TowerUpgradeController.MaxLevel];
    private Label _branch;
    private GridContainer _stats;
    private HBoxContainer _strongRow;
    private HBoxContainer _weakRow;
    private Label _lifetime;
    private Label _perSupply;
    private Label _suppressed;
    private Button _upgrade;
    private Label _diff;
    private HBoxContainer _branchRow;
    private Button _branchA;
    private Label _branchADiff;
    private Button _branchB;
    private Label _branchBDiff;
    private Button _sell;

    public override void _Ready()
    {
        ThemeTypeVariation = "PaperPanel";
        CustomMinimumSize = new Vector2(380f, 0f);
        var column = UiFactory.VBox(8);
        AddChild(column);

        var header = UiFactory.HBox(8);
        column.AddChild(header);
        _iconSlot = new Control { CustomMinimumSize = new Vector2(32f, 32f), MouseFilter = MouseFilterEnum.Ignore };
        header.AddChild(_iconSlot);
        var titles = UiFactory.VBox(0);
        titles.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(titles);
        _name = UiFactory.Label("PaperCardHeadingLabel", "");
        titles.AddChild(_name);
        var pipRow = UiFactory.HBox(3);
        titles.AddChild(pipRow);
        for (int i = 0; i < _pips.Length; i++)
        {
            _pips[i] = UiFactory.Icon("level_pip_off", 10, UiPalette.Ink) ?? new TextureRect();
            pipRow.AddChild(_pips[i]);
        }
        _branch = UiFactory.Label("PaperSmallLabel", "");
        pipRow.AddChild(_branch);
        var close = UiFactory.Button("PaperButton", "", () => CloseRequested?.Invoke(), "close");
        close.CustomMinimumSize = new Vector2(32f, 32f);
        close.TooltipText = "Close (Esc)";
        if (close.Icon == null) close.Text = "×";
        header.AddChild(close);

        column.AddChild(UiFactory.Rule(true));
        _stats = new GridContainer { Columns = 2 };
        _stats.AddThemeConstantOverride("h_separation", 16);
        _stats.AddThemeConstantOverride("v_separation", 2);
        column.AddChild(_stats);

        _strongRow = UiFactory.HBox(6);
        column.AddChild(_strongRow);
        _weakRow = UiFactory.HBox(6);
        column.AddChild(_weakRow);

        column.AddChild(UiFactory.Rule(true));
        var attribution = new GridContainer { Columns = 2 };
        attribution.AddThemeConstantOverride("h_separation", 16);
        column.AddChild(attribution);
        attribution.AddChild(UiFactory.Label("PaperBodyLabel", "Lifetime damage"));
        _lifetime = UiFactory.Label("PaperNumberLabel", "0", HorizontalAlignment.Right);
        _lifetime.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        attribution.AddChild(_lifetime);
        attribution.AddChild(UiFactory.Label("PaperBodyLabel", "Damage / Supply"));
        _perSupply = UiFactory.Label("PaperNumberLabel", "0.00", HorizontalAlignment.Right);
        _perSupply.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        attribution.AddChild(_perSupply);

        column.AddChild(UiFactory.Rule(true));
        _suppressed = UiFactory.Label("StampLabel", "SUPPRESSED", HorizontalAlignment.Center);
        _suppressed.Visible = false;
        column.AddChild(_suppressed);

        _upgrade = UiFactory.Button("PaperButton", "Upgrade", () => UpgradeRequested?.Invoke(), "upgrade_arrow");
        column.AddChild(_upgrade);
        _diff = UiFactory.Wrapped("PaperMonoLabel", "");
        column.AddChild(_diff);

        _branchRow = UiFactory.HBox(8);
        column.AddChild(_branchRow);
        _branchA = BranchColumn(_branchRow, "branch_a", TowerBranchChoice.A, out _branchADiff);
        _branchB = BranchColumn(_branchRow, "branch_b", TowerBranchChoice.B, out _branchBDiff);

        _sell = UiFactory.Button("PaperButton", "Sell", () => SellRequested?.Invoke(), "sell");
        column.AddChild(_sell);
    }

    private Button BranchColumn(Container row, string iconId, TowerBranchChoice choice, out Label diff)
    {
        var box = UiFactory.VBox(4);
        box.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(box);
        var button = UiFactory.Button("PaperButton", "", () => BranchRequested?.Invoke(choice), iconId);
        box.AddChild(button);
        diff = UiFactory.Wrapped("PaperSmallLabel", "");
        box.AddChild(diff);
        return button;
    }

    public void Refresh(InspectionView view)
    {
        _name.Text = view.Name;
        foreach (var child in _iconSlot.GetChildren()) child.QueueFree();
        var icon = UiFactory.Icon(view.IconId, 32, UiPalette.Ink);
        if (icon != null) _iconSlot.AddChild(icon);
        for (int i = 0; i < _pips.Length; i++)
            _pips[i].Texture = UiIcons.Get(i < view.Level ? "level_pip_on" : "level_pip_off");
        _branch.Text = string.IsNullOrEmpty(view.BranchName) ? "" : $"· {view.BranchName}";

        FillStats(view);
        FillMatchups(view);
        _lifetime.Text = $"{view.LifetimeDamage:F0}";
        _perSupply.Text = $"{view.DamagePerSupply:F2}";
        _suppressed.Visible = view.Suppressed;

        _upgrade.Visible = !view.AtFork;
        _diff.Visible = !view.AtFork && view.CanUpgrade;
        _branchRow.Visible = view.AtFork;
        if (view.AtFork)
        {
            _branchA.Text = $"{view.BranchAName} · {view.BranchACost}";
            _branchA.Disabled = view.SupplyBalance < view.BranchACost;
            _branchADiff.Text = Diff(view.Stats, view.BranchAPreview, view.IsCommandPost);
            _branchB.Text = $"{view.BranchBName} · {view.BranchBCost}";
            _branchB.Disabled = view.SupplyBalance < view.BranchBCost;
            _branchBDiff.Text = Diff(view.Stats, view.BranchBPreview, view.IsCommandPost);
        }
        else if (view.CanUpgrade)
        {
            _upgrade.Text = $"Upgrade · {view.UpgradeCost}";
            _upgrade.Disabled = view.SupplyBalance < view.UpgradeCost;
            _diff.Text = Diff(view.Stats, view.UpgradePreview, view.IsCommandPost);
        }
        else
        {
            _upgrade.Text = "Max level";
            _upgrade.Disabled = true;
        }

        _sell.Text = view.Suppressed ? "Sell (suppressed)" : $"Sell · refund {view.SellRefund}";
        _sell.Disabled = view.Suppressed;
    }

    private void FillStats(InspectionView view)
    {
        foreach (var child in _stats.GetChildren()) child.QueueFree();
        var s = view.Stats;
        if (view.IsCommandPost)
        {
            StatRow("Aura radius", $"{s.AuraRadiusTiles:0.0} tiles");
            StatRow("Aura bonus", $"+{s.AuraRangeBonusPercent * 100f:F0}% range · +{s.AuraRateOfFireBonusPercent * 100f:F0}% rate");
            StatRow("Command Points", $"+{s.CommandPointsPerWave} / wave");
            StatRow("Supply", $"+{s.SupplyPerWave} / wave");
            return;
        }
        StatRow("Damage", $"{s.DamagePerShot:F0}", UiIcons.ForDamageType(view.DamageType), MatchupRules.DamageTypeName(view.DamageType));
        StatRow("Rate of fire", $"{s.RateOfFirePerSec:0.00} /s");
        StatRow("Range", $"{s.RangeTiles:0.0} tiles");
        StatRow("DPS", $"{s.DamagePerShot * s.RateOfFirePerSec:F0}");
    }

    private void StatRow(string label, string value, string iconId = null, string iconLabel = null)
    {
        _stats.AddChild(UiFactory.Label("PaperBodyLabel", label));
        var right = UiFactory.HBox(6);
        right.Alignment = BoxContainer.AlignmentMode.End;
        right.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        right.AddChild(UiFactory.Label("PaperNumberLabel", value));
        if (iconId != null)
        {
            var glyph = UiFactory.Icon(iconId, 18, UiPalette.Ink);
            if (glyph != null) { glyph.TooltipText = iconLabel; right.AddChild(glyph); }
            right.AddChild(UiFactory.Label("PaperSmallLabel", iconLabel));
        }
        _stats.AddChild(right);
    }

    private void FillMatchups(InspectionView view)
    {
        foreach (var child in _strongRow.GetChildren()) child.QueueFree();
        foreach (var child in _weakRow.GetChildren()) child.QueueFree();
        _strongRow.Visible = !view.IsCommandPost;
        _weakRow.Visible = !view.IsCommandPost;
        if (view.IsCommandPost) return;
        MatchupRow(_strongRow, "Strong vs", MatchupRules.StrongVs(view.DamageType));
        MatchupRow(_weakRow, "Weak vs", MatchupRules.WeakVs(view.DamageType));
    }

    private static void MatchupRow(Container row, string caption, System.Collections.Generic.IReadOnlyList<(string IconId, string Label)> entries)
    {
        var label = UiFactory.Label("PaperBodyLabel", caption);
        label.CustomMinimumSize = new Vector2(92f, 0f);
        row.AddChild(label);
        foreach (var (iconId, text) in entries)
        {
            var glyph = UiFactory.Icon(iconId, 24, UiPalette.Ink);
            if (glyph != null) { glyph.TooltipText = text; row.AddChild(glyph); }
            row.AddChild(UiFactory.Label("PaperSmallLabel", text));
        }
    }

    // "Damage 45 → 62 · Range 5.0 → 5.5": only the stats that change.
    private static string Diff(TowerStatBlock from, TowerStatBlock to, bool commandPost)
    {
        if (from == null || to == null) return "";
        var parts = new System.Collections.Generic.List<string>();
        void Add(string name, float a, float b, string format = "0")
        {
            if (!Mathf.IsEqualApprox(a, b)) parts.Add($"{name} {a.ToString(format)} → {b.ToString(format)}");
        }
        if (commandPost)
        {
            Add("Aura", from.AuraRadiusTiles, to.AuraRadiusTiles, "0.0");
            Add("CP/wave", from.CommandPointsPerWave, to.CommandPointsPerWave);
            Add("Supply/wave", from.SupplyPerWave, to.SupplyPerWave);
        }
        else
        {
            Add("Damage", from.DamagePerShot, to.DamagePerShot);
            Add("Rate", from.RateOfFirePerSec, to.RateOfFirePerSec, "0.00");
            Add("Range", from.RangeTiles, to.RangeTiles, "0.0");
            Add("Blast", from.BlastRadiusTiles, to.BlastRadiusTiles, "0.0");
        }
        return parts.Count == 0 ? "No stat change" : string.Join(" · ", parts);
    }
}
