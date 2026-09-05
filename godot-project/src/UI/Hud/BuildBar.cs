using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;
using System.Collections.Generic;

namespace FrontsOfWar.UI.Hud;

// HUD zone E (docs/UI_DESIGN_SPEC.md §8.4; GDD §13.4, §7.4, §7.5, §19
// prompts 18-19): six TowerCards for the mission loadout. Selecting one
// glows every free BuildPad, previews range on hover, and places through
// TowerPlacementService on click. Works while the simulation is paused -
// TimeController only gates SimTick, never Godot's input dispatch - since
// placing towers during the build phase is the point of the build phase.
public partial class BuildBar : PanelContainer
{
    private static readonly Key[] Hotkeys = { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y };

    private MapRuntime _mission;
    private readonly List<(TowerDefinition Definition, Key Hotkey, TowerCard Card)> _slots = new();
    private TowerDefinition _selected;
    private RangePreview _rangePreview;

    public MapRuntime Mission { get => _mission; set => _mission = value; }

    public override void _Ready()
    {
        ThemeTypeVariation = "SlatePanel";
        var column = UiFactory.VBox(4);
        AddChild(column);
        column.AddChild(UiFactory.Label("CaptionLabel", "BUILD", uppercase: true));
        var row = UiFactory.HBox(8);
        column.AddChild(row);

        for (int i = 0; i < MissionSession.Loadout.Count && i < Hotkeys.Length; i++)
        {
            var definition = GD.Load<TowerDefinition>(MissionSession.Loadout[i]);
            if (definition == null) continue;
            var card = new TowerCard();
            row.AddChild(card);
            card.Setup(definition, Hotkeys[i]);
            var captured = definition;
            card.Pressed += () => SelectOrCancel(captured);
            _slots.Add((definition, Hotkeys[i], card));
        }

        // Deferred: this _Ready runs while MapRuntime is still adding its own
        // children (D21 in docs/DECISIONS.md) - an immediate AddChild onto the
        // Mission node fails with "Parent node is busy setting up children."
        _rangePreview = new RangePreview { Visible = false };
        Callable.From(() => _mission?.AddChild(_rangePreview)).CallDeferred();

        EventBus.Instance?.Subscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Subscribe<BuildPadClickedEvent>(OnPadClicked);
        EventBus.Instance?.Subscribe<BuildPadHoverChangedEvent>(OnPadHoverChanged);
        Callable.From(RefreshCards).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Unsubscribe<BuildPadClickedEvent>(OnPadClicked);
        EventBus.Instance?.Unsubscribe<BuildPadHoverChangedEvent>(OnPadHoverChanged);
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
        else if (_selected?.Archetype == TowerArchetype.Minefield
            && @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse)
        {
            var worldPoint = GetViewport().GetCanvasTransform().AffineInverse() * mouse.Position;
            // Success (0) is TowerPlacementResult's default enum value, so a
            // null mission/service must fall back to an explicit failure
            // here rather than `default(TowerPlacementOutcome)`.
            var outcome = _mission?.SpecialPlacement?.TryPlaceMinefield(_selected, worldPoint)
                ?? new TowerPlacementOutcome(TowerPlacementResult.NoControllerScene);
            if (outcome.Success) CancelBuildMode();
            // Refusals (off-route, field limit, too close to another field,
            // insufficient Supply) leave build mode active so the player can
            // try another point on the route, matching OnPadClicked below.
            GetViewport().SetInputAsHandled();
        }
    }

    private void SelectOrCancel(TowerDefinition definition)
    {
        if (_selected == definition) { CancelBuildMode(); return; }
        _selected = definition;
        // Minefields are free-placed on route path segments, not build pads
        // (GDD §6 T8) - there is nothing to glow.
        if (definition.Archetype != TowerArchetype.Minefield) SetPadGlow(true);
        RefreshCards();
    }

    private void CancelBuildMode()
    {
        _selected = null;
        SetPadGlow(false);
        _rangePreview?.Hide();
        RefreshCards();
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
        // player can try a different pad; an unaffordable pick is already a
        // disabled card with its shortfall shown.
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

    private void OnSupplyChanged(SupplyChangedEvent evt) => RefreshCards();

    private void RefreshCards()
    {
        if (_mission?.Supply == null) return;
        foreach (var slot in _slots)
        {
            int cost = slot.Definition.PreForkStatsForLevel(1).Cost;
            slot.Card.SetState(cost, cost - _mission.Supply.Balance, _selected == slot.Definition);
            if (slot.Definition.Archetype == TowerArchetype.Minefield && _mission.SpecialPlacement != null)
                slot.Card.SetFieldCount(_mission.Minefields.Fields.Count, _mission.SpecialPlacement.EffectiveMaxMinefields);
        }
    }

    // A bare world-space Node2D (not a Control - it sits at a pad's
    // GlobalPosition, not a screen offset) drawing the selected tower's L1
    // range ring while a build pad is hovered (spec §9: amber, 2 px, 48
    // segments, soft fill).
    private partial class RangePreview : Node2D
    {
        public float RadiusPixels;

        public override void _Draw()
        {
            if (RadiusPixels <= 0f) return;
            DrawCircle(Vector2.Zero, RadiusPixels, UiPalette.Amber with { A = 0.12f });
            DrawArc(Vector2.Zero, RadiusPixels, 0f, Mathf.Tau, 48, UiPalette.Amber with { A = 0.9f }, 2f, true);
        }
    }
}
