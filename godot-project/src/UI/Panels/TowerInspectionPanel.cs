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
    private CommandPostController _selectedPost;

    // T9 Command Post is a different node type but shares the same
    // level/branch/cost bookkeeping, so the panel works through these.
    private TowerUpgradeController SelectedUpgrade => _selectedPost?.Upgrade ?? _selected?.Upgrade;
    private TowerDefinition SelectedDefinition => _selectedPost?.Definition ?? _selected?.Definition;

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);
        BuildLayout();
        Hide();

        EventBus.Instance?.Subscribe<TowerClickedEvent>(OnTowerClicked);
        EventBus.Instance?.Subscribe<CommandPostClickedEvent>(OnCommandPostClicked);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<TowerClickedEvent>(OnTowerClicked);
        EventBus.Instance?.Unsubscribe<CommandPostClickedEvent>(OnCommandPostClicked);
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
        _selectedPost = null;
        _selected = evt.Tower;
        _panel.Position = evt.Tower.GlobalPosition + new Vector2(20, -20);
        Show();
        Refresh();
    }

    private void OnCommandPostClicked(CommandPostClickedEvent evt)
    {
        _selected = null;
        _selectedPost = evt.Post;
        _panel.Position = evt.Post.GlobalPosition + new Vector2(20, -20);
        Show();
        Refresh();
    }

    private void Close()
    {
        _selected = null;
        _selectedPost = null;
        Hide();
    }

    private void Refresh()
    {
        var upgrade = SelectedUpgrade;
        var definition = SelectedDefinition;
        if (upgrade == null || definition == null) { Hide(); return; }

        var stats = upgrade.CurrentStats();
        _titleLabel.Text = $"{definition.DisplayName}  (L{upgrade.Level})";
        _statsLabel.Text = _selectedPost != null ? CommandPostStats(stats) : CombatStats(stats, _selected);

        // GDD §6: "the fork happens when purchasing level 3" — at that one
        // level, offer both branches side by side instead of a single
        // button; every other level (including "no upgrades left") keeps
        // the single-button layout.
        bool atFork = upgrade.CanUpgrade && upgrade.Level == TowerUpgradeController.ForkLevel - 1;
        _upgradeButton.Visible = !atFork;
        _upgradeBranchAButton.Visible = atFork;
        _upgradeBranchBButton.Visible = atFork;

        if (atFork)
        {
            int costA = upgrade.UpgradeCost(TowerBranchChoice.A);
            int costB = upgrade.UpgradeCost(TowerBranchChoice.B);
            _upgradeBranchAButton.Text = $"{definition.BranchA?.Name ?? "Branch A"} ({costA})";
            _upgradeBranchAButton.Disabled = _mission.Supply.Balance < costA;
            _upgradeBranchBButton.Text = $"{definition.BranchB?.Name ?? "Branch B"} ({costB})";
            _upgradeBranchBButton.Disabled = _mission.Supply.Balance < costB;
        }
        else if (upgrade.CanUpgrade)
        {
            int cost = upgrade.UpgradeCost();
            _upgradeButton.Text = $"Upgrade ({cost})";
            _upgradeButton.Disabled = _mission.Supply.Balance < cost;
        }
        else
        {
            _upgradeButton.Text = "Max level";
            _upgradeButton.Disabled = true;
        }

        // Universal sell rule (GDD §6): towers may be sold any time except
        // while Suppressed. Command Posts never fire and are never suppressed.
        bool suppressed = _selected?.IsSuppressed == true;
        _sellButton.Text = suppressed ? "Sell (suppressed)" : $"Sell ({upgrade.SellRefund()})";
        _sellButton.Disabled = suppressed;
    }

    private static string CombatStats(TowerStatBlock stats, TowerController tower) =>
        $"Damage: {stats.DamagePerShot:F0} {tower.Definition.DamageType}\n" +
        $"Rate of fire: {stats.RateOfFirePerSec:F2}/s\n" +
        $"Range: {stats.RangeTiles:F1} tiles\n" +
        $"{MatchupRows(tower.Definition.DamageType)}\n" +
        $"Lifetime damage: {tower.LifetimeDamage:F0}\n" +
        $"Damage / Supply: {DamagePerSupply(tower):F2}";

    private static string CommandPostStats(TowerStatBlock stats) =>
        $"Aura radius: {stats.AuraRadiusTiles:F1} tiles\n" +
        $"Aura: +{stats.AuraRangeBonusPercent * 100f:F0}% range, +{stats.AuraRateOfFireBonusPercent * 100f:F0}% rate of fire\n" +
        $"Command Points / wave: +{stats.CommandPointsPerWave}\n" +
        $"Supply / wave: +{stats.SupplyPerWave}";

    private void OnUpgradePressed()
    {
        var upgrade = SelectedUpgrade;
        if (upgrade == null || !upgrade.CanUpgrade) return;

        int cost = upgrade.UpgradeCost();
        if (!_mission.Supply.TrySpend(cost)) return;

        upgrade.Upgrade();
        Refresh();
    }

    private void OnUpgradeBranchPressed(TowerBranchChoice branch)
    {
        var upgrade = SelectedUpgrade;
        if (upgrade == null || !upgrade.CanUpgrade) return;

        int cost = upgrade.UpgradeCost(branch);
        if (!_mission.Supply.TrySpend(cost)) return;

        upgrade.Upgrade(branch);
        Refresh();
    }

    private void OnSellPressed()
    {
        if (_selectedPost != null)
        {
            _mission.Supply.Credit(_selectedPost.Upgrade.SellRefund());
            _mission.CommandPosts.Unregister(_selectedPost);
            _mission.Placement?.ReleasePad(_selectedPost);
            _selectedPost.QueueFree();
            Close();
            return;
        }

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
