using Godot;
using System.Linq;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Panels;

// The tower inspection panel (GDD §13.5, §19 prompt 20; docs/UI_DESIGN_SPEC
// .md §8.5). Opens on TowerClickedEvent / CommandPostClickedEvent, anchors
// the paper card 24 px beside the tower in screen space (flipping left at
// the screen edge), drives the world-space selection overlay, and closes on
// Esc or a click elsewhere. Command Posts share the same upgrade bookkeeping.
public partial class TowerInspectionPanel : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private TowerInspectionCard _card;
    private TowerSelectionOverlay _overlay;
    private TowerController _selected;
    private CommandPostController _selectedPost;

    private TowerUpgradeController SelectedUpgrade => _selectedPost?.Upgrade ?? _selected?.Upgrade;
    private TowerDefinition SelectedDefinition => _selectedPost?.Definition ?? _selected?.Definition;
    private Node2D SelectedNode => (Node2D)_selectedPost ?? _selected;

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);
        _card = new TowerInspectionCard();
        AddChild(_card);
        _card.Visible = false;
        _card.UpgradeRequested += OnUpgradePressed;
        _card.BranchRequested += OnUpgradeBranchPressed;
        _card.SellRequested += OnSellPressed;
        _card.CloseRequested += Close;

        _overlay = new TowerSelectionOverlay { Visible = false, ZIndex = 50 };
        Callable.From(() => _mission?.AddChild(_overlay)).CallDeferred();

        EventBus.Instance?.Subscribe<TowerClickedEvent>(OnTowerClicked);
        EventBus.Instance?.Subscribe<CommandPostClickedEvent>(OnCommandPostClicked);

        if (ScreenshotCapture.UiStateIs("inspect"))
            Callable.From(() => { var first = _mission?.Towers.Towers.FirstOrDefault(); if (first != null) OnTowerClicked(new TowerClickedEvent(first)); }).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<TowerClickedEvent>(OnTowerClicked);
        EventBus.Instance?.Unsubscribe<CommandPostClickedEvent>(OnCommandPostClicked);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_card.Visible) return;
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            Close();
            GetViewport().SetInputAsHandled();
            return;
        }
        // A click that reached here missed the card. Close without marking
        // the event handled so a tower under the cursor still gets picked
        // (physics picking runs after unhandled input) and reopens the card.
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) Close();
    }

    private void OnTowerClicked(TowerClickedEvent evt)
    {
        _selectedPost = null;
        _selected = evt.Tower;
        _overlay?.Select(evt.Tower);
        Open();
    }

    private void OnCommandPostClicked(CommandPostClickedEvent evt)
    {
        _selected = null;
        _selectedPost = evt.Post;
        _overlay?.Select(evt.Post);
        Open();
    }

    private void Open()
    {
        bool wasVisible = _card.Visible;
        Refresh();
        _card.Visible = true;
        if (!wasVisible) UiFactory.FadeIn(_card);
        Callable.From(PositionCard).CallDeferred();
    }

    private void Close()
    {
        _selected = null;
        _selectedPost = null;
        _overlay?.Clear();
        _card.Visible = false;
    }

    private void PositionCard()
    {
        var node = SelectedNode;
        if (node == null || !IsInstanceValid(node) || !_card.Visible) return;
        var screen = node.GetGlobalTransformWithCanvas().Origin;
        var viewport = GetViewport().GetVisibleRect().Size;
        var size = _card.GetCombinedMinimumSize();
        float x = screen.X + 24f;
        if (x + size.X > viewport.X - 24f) x = screen.X - 24f - size.X;
        float y = Mathf.Clamp(screen.Y - size.Y / 2f, 24f, Mathf.Max(24f, viewport.Y - size.Y - 24f));
        _card.Position = new Vector2(x, y);
    }

    private void Refresh()
    {
        var upgrade = SelectedUpgrade;
        var definition = SelectedDefinition;
        if (upgrade == null || definition == null) { Close(); return; }

        // GDD §6: "the fork happens when purchasing level 3" - at that one
        // level both branches are offered side by side.
        bool atFork = upgrade.CanUpgrade && upgrade.Level == TowerUpgradeController.ForkLevel - 1;
        var view = new TowerInspectionCard.InspectionView
        {
            Name = definition.DisplayName,
            IconId = UiIcons.ForTower(definition.Id),
            Level = upgrade.Level,
            BranchName = upgrade.Branch switch
            {
                TowerBranchChoice.A => definition.BranchA?.Name,
                TowerBranchChoice.B => definition.BranchB?.Name,
                _ => null,
            },
            IsCommandPost = _selectedPost != null,
            DamageType = definition.DamageType,
            Stats = upgrade.CurrentStats(),
            LifetimeDamage = _selected?.LifetimeDamage ?? 0f,
            DamagePerSupply = _selected != null && upgrade.TotalInvested > 0 ? _selected.LifetimeDamage / upgrade.TotalInvested : 0f,
            Suppressed = _selected?.IsSuppressed == true,
            CanUpgrade = upgrade.CanUpgrade,
            AtFork = atFork,
            UpgradeCost = upgrade.CanUpgrade ? upgrade.UpgradeCost() : 0,
            UpgradePreview = upgrade.PreviewStats(),
            BranchAName = definition.BranchA?.Name ?? "Branch A",
            BranchACost = atFork ? upgrade.UpgradeCost(TowerBranchChoice.A) : 0,
            BranchAPreview = atFork ? upgrade.PreviewStats(TowerBranchChoice.A) : null,
            BranchBName = definition.BranchB?.Name ?? "Branch B",
            BranchBCost = atFork ? upgrade.UpgradeCost(TowerBranchChoice.B) : 0,
            BranchBPreview = atFork ? upgrade.PreviewStats(TowerBranchChoice.B) : null,
            SellRefund = upgrade.SellRefund(),
            SupplyBalance = _mission.Supply.Balance,
        };
        _card.Refresh(view);
    }

    private void OnUpgradePressed()
    {
        var upgrade = SelectedUpgrade;
        if (upgrade == null || !upgrade.CanUpgrade) return;
        if (!_mission.Supply.TrySpend(upgrade.UpgradeCost())) return;
        upgrade.Upgrade();
        AfterChange();
    }

    private void OnUpgradeBranchPressed(TowerBranchChoice branch)
    {
        var upgrade = SelectedUpgrade;
        if (upgrade == null || !upgrade.CanUpgrade) return;
        if (!_mission.Supply.TrySpend(upgrade.UpgradeCost(branch))) return;
        upgrade.Upgrade(branch);
        AfterChange();
    }

    private void AfterChange()
    {
        Refresh();
        _overlay?.QueueRedraw();
        Callable.From(PositionCard).CallDeferred();
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

        // Universal sell rule (GDD §6): any time except while Suppressed.
        if (_selected == null || _selected.IsSuppressed) return;
        int refund = _selected.Sell();
        _mission.Supply.Credit(refund);
        _mission.Towers.Unregister(_selected);
        _mission.Placement?.ReleasePad(_selected);
        _selected.QueueFree();
        Close();
    }
}
