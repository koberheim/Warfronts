using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;

namespace FrontsOfWar.UI.Panels;

// The tower inspection panel (GDD §13.5, §19 prompt 20). Opens on
// TowerClickedEvent, anchored near the tower. Uses prototype glyph rows for
// Strong vs / Weak vs and live lifetime damage-per-Supply attribution while
// the project still uses placeholder art.
public partial class TowerInspectionPanel : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private PanelContainer _panel;
    private Label _titleLabel;
    private Label _statsLabel;
    private Button _upgradeButton;
    private Button _upgradeBranchAButton;
    private Button _upgradeBranchBButton;
    private Button _sellButton;
    private TowerController _selected;

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);
        BuildLayout();
        Hide();

        EventBus.Instance?.Subscribe<TowerClickedEvent>(OnTowerClicked);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<TowerClickedEvent>(OnTowerClicked);
    }

    private void BuildLayout()
    {
        _panel = new PanelContainer { Position = new Vector2(16, 500), CustomMinimumSize = new Vector2(260, 0) };
        AddChild(_panel);

        var vbox = new VBoxContainer();
        _panel.AddChild(vbox);

        _titleLabel = new Label();
        vbox.AddChild(_titleLabel);
        _statsLabel = new Label();
        vbox.AddChild(_statsLabel);

        var buttonRow = new HBoxContainer();
        vbox.AddChild(buttonRow);

        _upgradeButton = new Button();
        _upgradeButton.Pressed += OnUpgradePressed;
        buttonRow.AddChild(_upgradeButton);

        // At the fork level (GDD §6: "the fork happens when purchasing
        // level 3"), the single upgrade button is replaced by one button per
        // branch so the player can see both names/costs before committing —
        // the choice is permanent unless the tower is sold.
        _upgradeBranchAButton = new Button();
        _upgradeBranchAButton.Pressed += () => OnUpgradeBranchPressed(TowerBranchChoice.A);
        buttonRow.AddChild(_upgradeBranchAButton);

        _upgradeBranchBButton = new Button();
        _upgradeBranchBButton.Pressed += () => OnUpgradeBranchPressed(TowerBranchChoice.B);
        buttonRow.AddChild(_upgradeBranchBButton);

        _sellButton = new Button();
        _sellButton.Pressed += OnSellPressed;
        buttonRow.AddChild(_sellButton);

        var closeButton = new Button { Text = "Close" };
        closeButton.Pressed += Close;
        buttonRow.AddChild(closeButton);
    }

    private void OnTowerClicked(TowerClickedEvent evt)
    {
        _selected = evt.Tower;
        _panel.Position = evt.Tower.GlobalPosition + new Vector2(20, -20);
        Show();
        Refresh();
    }

    private void Close()
    {
        _selected = null;
        Hide();
    }

    private void Refresh()
    {
        if (_selected == null) { Hide(); return; }

        var stats = _selected.Upgrade.CurrentStats();
        _titleLabel.Text = $"{_selected.Definition.DisplayName}  (L{_selected.Upgrade.Level})";
        _statsLabel.Text =
            $"Damage: {stats.DamagePerShot:F0} {_selected.Definition.DamageType}\n" +
            $"Rate of fire: {stats.RateOfFirePerSec:F2}/s\n" +
            $"Range: {stats.RangeTiles:F1} tiles\n" +
            $"{MatchupRows(_selected.Definition.DamageType)}\n" +
            $"Lifetime damage: {_selected.LifetimeDamage:F0}\n" +
            $"Damage / Supply: {DamagePerSupply(_selected):F2}";

        // GDD §6: "the fork happens when purchasing level 3" — at that one
        // level, offer both branches side by side instead of a single
        // button; every other level (including "no upgrades left") keeps
        // the single-button layout.
        bool atFork = _selected.Upgrade.CanUpgrade && _selected.Upgrade.Level == TowerUpgradeController.ForkLevel - 1;
        _upgradeButton.Visible = !atFork;
        _upgradeBranchAButton.Visible = atFork;
        _upgradeBranchBButton.Visible = atFork;

        if (atFork)
        {
            var definition = _selected.Definition;
            int costA = _selected.Upgrade.UpgradeCost(TowerBranchChoice.A);
            int costB = _selected.Upgrade.UpgradeCost(TowerBranchChoice.B);
            _upgradeBranchAButton.Text = $"{definition.BranchA?.Name ?? "Branch A"} ({costA})";
            _upgradeBranchAButton.Disabled = _mission.Supply.Balance < costA;
            _upgradeBranchBButton.Text = $"{definition.BranchB?.Name ?? "Branch B"} ({costB})";
            _upgradeBranchBButton.Disabled = _mission.Supply.Balance < costB;
        }
        else if (_selected.Upgrade.CanUpgrade)
        {
            int cost = _selected.Upgrade.UpgradeCost();
            _upgradeButton.Text = $"Upgrade ({cost})";
            _upgradeButton.Disabled = _mission.Supply.Balance < cost;
        }
        else
        {
            _upgradeButton.Text = "Max level";
            _upgradeButton.Disabled = true;
        }

        // Universal sell rule (GDD §6): towers may be sold any time except
        // while Suppressed.
        _sellButton.Text = _selected.IsSuppressed
            ? "Sell (suppressed)"
            : $"Sell ({_selected.Upgrade.SellRefund()})";
        _sellButton.Disabled = _selected.IsSuppressed;
    }

    private void OnUpgradePressed()
    {
        if (_selected == null || !_selected.Upgrade.CanUpgrade) return;

        int cost = _selected.Upgrade.UpgradeCost();
        if (!_mission.Supply.TrySpend(cost)) return;

        _selected.Upgrade.Upgrade();
        Refresh();
    }

    private void OnUpgradeBranchPressed(TowerBranchChoice branch)
    {
        if (_selected == null || !_selected.Upgrade.CanUpgrade) return;

        int cost = _selected.Upgrade.UpgradeCost(branch);
        if (!_mission.Supply.TrySpend(cost)) return;

        _selected.Upgrade.Upgrade(branch);
        Refresh();
    }

    private void OnSellPressed()
    {
        if (_selected == null || _selected.IsSuppressed) return;

        int refund = _selected.Sell();
        _mission.Supply.Credit(refund);
        _mission.Towers.Unregister(_selected);
        _mission.Placement?.ReleasePad(_selected);
        _selected.QueueFree();
        Close();
    }

    private static float DamagePerSupply(TowerController tower)
        => tower.Upgrade.TotalInvested > 0
            ? tower.LifetimeDamage / tower.Upgrade.TotalInvested
            : 0f;

    private static string MatchupRows(DamageType damageType) => damageType switch
    {
        DamageType.SmallArms => "Strong vs: [cloth] Soft\nWeak vs: [shield] Armored, Heavy",
        DamageType.Explosive => "Strong vs: [half-shield] Hardened\nWeak vs: [double-shield] Heavy",
        DamageType.ArmorPiercing => "Strong vs: [shield] Armored, Heavy\nWeak vs: [cloth] Soft",
        DamageType.AntiAir => "Strong vs: [wing] Air\nWeak vs: [ground] Ground",
        _ => "Strong vs: —\nWeak vs: —",
    };
}
