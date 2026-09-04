using Godot;

namespace FrontsOfWar.UI.Theme;

// The Defense Line bar (docs/UI_DESIGN_SPEC.md §8.4): a fixed number of
// segments so the player counts hits rather than reading a gradient, olive
// while healthy and red at or below the low-water fraction. Shape (segment
// count) plus color together, never color alone (GDD §13.9).
public partial class SegmentedBar : Control
{
    public int Segments = 20;
    public int Filled = 20;
    public float LowFraction = 0.25f;
    public Color FillColor = UiPalette.Olive;
    public Color LowColor = UiPalette.Red;
    public Color EmptyColor = UiPalette.SlateHi;
    public Color LineColor = UiPalette.SlateLine;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        if (CustomMinimumSize == Vector2.Zero) CustomMinimumSize = new Vector2(240f, 14f);
    }

    public void SetValue(int filled, int max)
    {
        Segments = Mathf.Max(1, max);
        Filled = Mathf.Clamp(filled, 0, Segments);
        QueueRedraw();
    }

    public override void _Draw()
    {
        float width = Size.X > 0f ? Size.X : CustomMinimumSize.X;
        float height = Size.Y > 0f ? Size.Y : CustomMinimumSize.Y;
        float gap = 1f;
        float segmentWidth = (width - gap * (Segments - 1)) / Segments;
        bool low = Segments > 0 && (float)Filled / Segments <= LowFraction;
        var fill = low ? LowColor : FillColor;

        for (int i = 0; i < Segments; i++)
        {
            float x = i * (segmentWidth + gap);
            DrawRect(new Rect2(x, 0f, segmentWidth, height), i < Filled ? fill : EmptyColor);
        }
        DrawRect(new Rect2(0f, 0f, width, height), LineColor, false, 1f);
    }
}
