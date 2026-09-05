using System;
using Godot;

namespace FrontsOfWar.UI.Theme;

// A themed 0-1 slider drawn entirely in code, the same "paint it in _Draw"
// approach RangePreview and MinefieldController's charge bar already use
// (docs/UI_DESIGN_SPEC.md §7) - fow_theme.tres has no Slider styling, and
// Godot's native HSlider needs its own grabber icon texture to look
// intentional rather than default-flat, which is new asset work this avoids.
public partial class PaperSlider : Control
{
    // Fires continuously while dragging/clicking, for live feedback (e.g.
    // audible volume change). DragEnded fires once the mouse releases, for
    // callers that want to persist rather than write on every motion event.
    public event Action<float> ValueChanged;
    public event Action DragEnded;

    private float _value;
    private bool _dragging;

    public float Value
    {
        get => _value;
        set
        {
            float clamped = Mathf.Clamp(value, 0f, 1f);
            if (Mathf.IsEqualApprox(clamped, _value)) return;
            _value = clamped;
            QueueRedraw();
        }
    }

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(160f, 28f);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouse)
        {
            if (mouse.Pressed)
            {
                _dragging = true;
                SetValueFromLocalX(mouse.Position.X);
            }
            else if (_dragging)
            {
                _dragging = false;
                DragEnded?.Invoke();
            }
            AcceptEvent();
        }
        else if (@event is InputEventMouseMotion motion && _dragging)
        {
            SetValueFromLocalX(motion.Position.X);
            AcceptEvent();
        }
    }

    private void SetValueFromLocalX(float x)
    {
        float width = Mathf.Max(1f, Size.X);
        _value = Mathf.Clamp(x / width, 0f, 1f); // bypass the equality guard so a drag feels continuous
        QueueRedraw();
        ValueChanged?.Invoke(_value);
    }

    public override void _Draw()
    {
        float trackHeight = 6f;
        var trackRect = new Rect2(0f, (Size.Y - trackHeight) * 0.5f, Size.X, trackHeight);
        DrawRect(trackRect, UiPalette.Ink.WithAlpha(0.18f));
        DrawRect(trackRect with { Size = trackRect.Size with { X = trackRect.Size.X * _value } }, UiPalette.Amber);

        var handleCenter = new Vector2(_value * Size.X, Size.Y * 0.5f);
        float handleRadius = Size.Y * 0.5f - 1f;
        DrawCircle(handleCenter, handleRadius, UiPalette.WoodDark);
        DrawCircle(handleCenter, handleRadius - 2.5f, UiPalette.Cream);
    }
}
