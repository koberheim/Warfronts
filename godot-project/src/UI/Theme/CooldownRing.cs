using Godot;

namespace FrontsOfWar.UI.Theme;

// Radial time display (docs/UI_DESIGN_SPEC.md §8.4, §10): a Ring for the
// build-phase countdown (track + amber sweep of the time remaining) or a Pie
// laid over an ability icon (slate wedge covering the cooldown remaining).
// Continuous, never blinking - the only motion the HUD is allowed to loop.
public partial class CooldownRing : Control
{
    public enum Style { Ring, Pie }

    public Style Mode = Style.Ring;
    public float Fraction;              // 0..1 of time remaining
    public float Thickness = 5f;
    public Color SweepColor = UiPalette.Amber;
    public Color TrackColor = UiPalette.SlateHi;

    private const int SegmentsPerTurn = 64;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void SetFraction(float fraction)
    {
        float clamped = Mathf.Clamp(fraction, 0f, 1f);
        if (Mathf.IsEqualApprox(clamped, Fraction)) return;
        Fraction = clamped;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var center = Size / 2f;
        float radius = Mathf.Min(Size.X, Size.Y) / 2f;
        if (radius <= 0f) return;
        float start = -Mathf.Pi / 2f;
        float sweep = Mathf.Tau * Fraction;

        if (Mode == Style.Ring)
        {
            float r = radius - Thickness / 2f;
            DrawArc(center, r, 0f, Mathf.Tau, SegmentsPerTurn, TrackColor, Thickness, true);
            if (Fraction > 0.001f)
                DrawArc(center, r, start, start + sweep, Mathf.Max(2, (int)(SegmentsPerTurn * Fraction)), SweepColor, Thickness, true);
            return;
        }

        if (Fraction <= 0.001f) return;
        int steps = Mathf.Max(2, (int)(SegmentsPerTurn * Fraction));
        var points = new Vector2[steps + 2];
        points[0] = center;
        for (int i = 0; i <= steps; i++)
        {
            float angle = start + sweep * i / steps;
            points[i + 1] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        DrawColoredPolygon(points, SweepColor);
    }
}
