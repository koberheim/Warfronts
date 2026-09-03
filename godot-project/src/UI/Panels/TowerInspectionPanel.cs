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

        if (_selected.Upgrade.CanUpgrade)
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

        _sellButton.Text = $"Sell ({_selected.Upgrade.SellRefund()})";
    }

    private void OnUpgradePressed()
    {
        if (_selected == null || !_selected.Upgrade.CanUpgrade) return;

        int cost = _selected.Upgrade.UpgradeCost();
        if (!_mission.Supply.TrySpend(cost)) return;

        _selected.Upgrade.Upgrade();
        Refresh();
    }

    private void OnSellPressed()
    {
        if (_selected == null) return;

        int refund = _selected.Sell();
        _mission.Supply.Credit(refund);
        _mission.Towers.Unregister(_selected);
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
