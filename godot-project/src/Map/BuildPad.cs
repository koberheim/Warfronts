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

    private void OnMouseEntered()
    {
        IsHovered = true;
        if (_highlight != null) _highlight.Visible = true;
        EventBus.Instance?.Publish(new BuildPadHoverChangedEvent(this, true));
    }

    private void OnMouseExited()
    {
        IsHovered = false;
        if (_highlight != null) _highlight.Visible = false;
        EventBus.Instance?.Publish(new BuildPadHoverChangedEvent(this, false));
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            EventBus.Instance?.Publish(new BuildPadClickedEvent(this));
    }
}
