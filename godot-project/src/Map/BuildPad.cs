using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.Map;

// A discrete tower placement site (GDD §7.5, §19 prompt 7). Expects a child
// Area2D named "HoverArea" for mouse detection and an optional child
// CanvasItem named "Highlight" that this script toggles on hover.
public partial class BuildPad : Node2D
{
    [Export] public PadTag Tag = PadTag.Standard;

    public bool IsOccupied { get; private set; }
    public bool IsHovered { get; private set; }

    private Area2D _hoverArea;
    private CanvasItem _highlight;
    private bool _buildModeGlow;

    public override void _Ready()
    {
        _hoverArea = GetNodeOrNull<Area2D>("HoverArea");
        _highlight = GetNodeOrNull<CanvasItem>("Highlight");
        if (_highlight != null) _highlight.Visible = false;

        if (_hoverArea == null)
        {
            GD.PushError($"BuildPad '{Name}' has no child Area2D named 'HoverArea'.");
            return;
        }

        _hoverArea.MouseEntered += OnMouseEntered;
        _hoverArea.MouseExited += OnMouseExited;
        _hoverArea.InputEvent += OnInputEvent;
    }

    public override void _ExitTree()
    {
        if (_hoverArea == null) return;
        _hoverArea.MouseEntered -= OnMouseEntered;
        _hoverArea.MouseExited -= OnMouseExited;
        _hoverArea.InputEvent -= OnInputEvent;
    }

    public void SetOccupied(bool occupied) => IsOccupied = occupied;

    // Toggled by the build bar (GDD §13.4: "build pads glow when the build
    // menu is open") for every unoccupied pad while a tower is selected.
    // Reuses the same Highlight node as mouse hover — the two never conflict
    // because hover-out only turns the highlight back off if build mode
    // isn't also asking for it.
    public void SetBuildModeGlow(bool active)
    {
        _buildModeGlow = active;
        if (_highlight != null) _highlight.Visible = active || IsHovered;
        QueueRedraw();
    }

    private void OnMouseEntered()
    {
        IsHovered = true;
        if (_highlight != null) _highlight.Visible = true;
        QueueRedraw();
        EventBus.Instance?.Publish(new BuildPadHoverChangedEvent(this, true));
    }

    private void OnMouseExited()
    {
        IsHovered = false;
        if (_highlight != null) _highlight.Visible = _buildModeGlow;
        QueueRedraw();
        EventBus.Instance?.Publish(new BuildPadHoverChangedEvent(this, false));
    }

    // Build-mode ring (UI spec §9: amber 2 px ring + soft fill while a build
    // card is selected, stronger fill on hover) drawn over the pad mark.
    public override void _Draw()
    {
        if (!_buildModeGlow || IsOccupied) return;
        var amber = UI.Theme.UiPalette.Amber;
        var rect = new Rect2(-20f, -20f, 40f, 40f);
        if (IsHovered) DrawRect(rect, amber with { A = 0.2f });
        DrawRect(rect, amber with { A = 0.95f }, false, 2f);
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            EventBus.Instance?.Publish(new BuildPadClickedEvent(this));
    }
}
