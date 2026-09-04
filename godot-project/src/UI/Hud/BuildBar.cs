using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using System.Collections.Generic;

namespace FrontsOfWar.UI.Hud;

// Bottom-center build bar (GDD §13.4, §7.4, §7.5, §19 prompts 18-19). Lets
// the player select one of the mission's six loadout towers and place it on
// an open BuildPad. Works while the simulation is paused — GameLoop.Instance
// .Time.IsPaused only gates GameLoop.SimTick, never Godot's own input
// dispatch — since placing towers during the build phase is the point of the
// build phase.
public partial class BuildBar : Control
{
    private readonly record struct Slot(TowerDefinition Definition, Key Hotkey);

    private static readonly Key[] Hotkeys = { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y };

    private MapRuntime _mission;
    private readonly List<Slot> _slots = new();
    private readonly Dictionary<TowerDefinition, Button> _buttons = new();
    private TowerDefinition _selected;
    private RangePreview _rangePreview;

    public MapRuntime Mission { get => _mission; set => _mission = value; }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Ignore;
        LoadLoadout();
        BuildLayout();

        // Deferred: BuildBar._Ready() runs while HudController.BuildLayout()
        // is still adding HudController's own children, which in turn runs
        // while MapRuntime is still adding its own children (D21 in
        // docs/DECISIONS.md) — an immediate AddChild onto the Mission node
        // here fails with "Parent node is busy setting up children."
        _rangePreview = new RangePreview { Visible = false };
        Callable.From(() => _mission?.AddChild(_rangePreview)).CallDeferred();

        EventBus.Instance?.Subscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Subscribe<BuildPadClickedEvent>(OnPadClicked);
        EventBus.Instance?.Subscribe<BuildPadHoverChangedEvent>(OnPadHoverChanged);
        Callable.From(RefreshButtons).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Unsubscribe<BuildPadClickedEvent>(OnPadClicked);
        EventBus.Instance?.Unsubscribe<BuildPadHoverChangedEvent>(OnPadHoverChanged);
    }

    private void LoadLoadout()
    {
        for (int i = 0; i < MissionSession.Loadout.Count && i < Hotkeys.Length; i++)
        {
            var definition = GD.Load<TowerDefinition>(MissionSession.Loadout[i]);
            if (definition != null) _slots.Add(new Slot(definition, Hotkeys[i]));
        }
    }

    private void BuildLayout()
    {
        CustomMinimumSize = new Vector2(380f, 90f);
        Size = CustomMinimumSize;

        var panel = new PanelContainer { CustomMinimumSize = CustomMinimumSize, Size = CustomMinimumSize, MouseFilter = MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", BuildPanelStyle());
        AddChild(panel);

        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.AddThemeConstantOverride("separation", 3);
        panel.AddChild(column);

        column.AddChild(new Label { Text = "BUILD", HorizontalAlignment = HorizontalAlignment.Center });

        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", 3);
        column.AddChild(row);

        foreach (var slot in _slots)
        {
            var button = new Button
            {
                CustomMinimumSize = new Vector2(58f, 56f),
                FocusMode = FocusModeEnum.None,
                TooltipText = $"{slot.Definition.DisplayName} ({slot.Hotkey})",
            };
            button.AddThemeFontSizeOverride("font_size", 10);
            button.Pressed += () => SelectOrCancel(slot.Definition);
            row.AddChild(button);
            _buttons[slot.Definition] = button;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            foreach (var slot in _slots)
            {
                if (key.Keycode != slot.Hotkey) continue;
                SelectOrCancel(slot.Definition);
                GetViewport().SetInputAsHandled();
                return;
            }

            if (key.Keycode == Key.Escape && _selected != null)
            {
                CancelBuildMode();
                GetViewport().SetInputAsHandled();
            }
            return;
        }

        if (_selected != null && @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
        {
            CancelBuildMode();
            GetViewport().SetInputAsHandled();
        }
    }

    private void SelectOrCancel(TowerDefinition definition)
    {
        if (_selected == definition) { CancelBuildMode(); return; }
        _selected = definition;
        SetPadGlow(true);
        RefreshButtons();
    }

    private void CancelBuildMode()
    {
        _selected = null;
        SetPadGlow(false);
        _rangePreview?.Hide();
        RefreshButtons();
    }

    private void SetPadGlow(bool active)
    {
        if (_mission == null) return;
        foreach (var child in _mission.GetChildren())
            if (child is BuildPad pad && !pad.IsOccupied)
                pad.SetBuildModeGlow(active);
    }

    private void OnPadClicked(BuildPadClickedEvent evt)
    {
        if (_selected == null || _mission?.Placement == null) return;

        var outcome = _mission.Placement.TryPlace(_selected, evt.Pad);
        if (outcome.Success)
        {
            evt.Pad.SetBuildModeGlow(false);
            CancelBuildMode();
        }
        // Refusals (occupied pad, no scene) leave build mode active so the
        // player can try a different pad; an unaffordable pick is already
        // visible as a greyed-out, shortfall-labelled button.
    }

    private void OnPadHoverChanged(BuildPadHoverChangedEvent evt)
    {
        if (_selected == null || _rangePreview == null) return;
        if (!evt.IsHovered) { _rangePreview.Hide(); return; }

        float rangeTiles = _selected.PreForkStatsForLevel(1).RangeTiles;
        _rangePreview.RadiusPixels = rangeTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
        _rangePreview.Position = evt.Pad.GlobalPosition;
        _rangePreview.QueueRedraw();
        _rangePreview.Show();
    }

    private void OnSupplyChanged(SupplyChangedEvent evt) => RefreshButtons();

    private void RefreshButtons()
    {
        if (_mission?.Supply == null) return;
        foreach (var slot in _slots)
        {
            var button = _buttons[slot.Definition];
            int cost = slot.Definition.PreForkStatsForLevel(1).Cost;
            int shortfall = cost - _mission.Supply.Balance;
            button.Disabled = shortfall > 0;
            button.Text = shortfall > 0
                ? $"[{slot.Hotkey}] {slot.Definition.DisplayName}\n{cost}  Need +{shortfall}"
                : $"[{slot.Hotkey}] {slot.Definition.DisplayName}\n{cost}";
            button.Modulate = _selected == slot.Definition ? new Color(1f, 0.9f, 0.45f) : Colors.White;
        }
    }

    private static StyleBoxFlat BuildPanelStyle() => new()
    {
        BgColor = new Color(0.06f, 0.08f, 0.1f, 0.92f),
        BorderColor = new Color(0.65f, 0.52f, 0.28f, 0.95f),
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2,
        CornerRadiusTopLeft = 5,
        CornerRadiusTopRight = 5,
        CornerRadiusBottomRight = 5,
        CornerRadiusBottomLeft = 5,
        ContentMarginLeft = 6,
        ContentMarginTop = 4,
        ContentMarginRight = 6,
        ContentMarginBottom = 4,
    };

    // A bare world-space Node2D (not a Control — it needs to sit at a pad's
    // GlobalPosition, not a screen offset) that draws the selected tower's
    // L1 range ring while a build pad is hovered.
    private partial class RangePreview : Node2D
    {
        public float RadiusPixels;

        public override void _Draw()
        {
            if (RadiusPixels <= 0f) return;
            DrawArc(Vector2.Zero, RadiusPixels, 0f, Mathf.Tau, 48, new Color(0.95f, 0.85f, 0.3f, 0.7f), 2f);
        }
    }
}
