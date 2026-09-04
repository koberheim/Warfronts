using Godot;
using FrontsOfWar.Combat;

namespace FrontsOfWar.UI.Theme;

// The UI palette for _Draw code (health bars, range rings, radial
// cooldowns, damage numbers) - a mirror of the colors authored in
// assets/ui/theme/fow_theme.tres, which is the source of truth for every
// Control node. Keep the two in step (docs/UI_DESIGN_SPEC.md). Nothing
// here is gameplay balance; GameBalanceConfig stays the tuning surface.
public static class UiPalette
{
    public static readonly Color Ink = new(0.1686f, 0.1333f, 0.1020f);
    public static readonly Color InkMuted = new(0.3608f, 0.3059f, 0.2431f);
    public static readonly Color Paper = new(0.9098f, 0.8627f, 0.7529f);
    public static readonly Color PaperDark = new(0.8392f, 0.7765f, 0.6353f);
    public static readonly Color PaperEdge = new(0.7255f, 0.6471f, 0.4863f);
    public static readonly Color WoodDark = new(0.2275f, 0.1412f, 0.0863f);
    public static readonly Color WoodMid = new(0.3529f, 0.2275f, 0.1333f);
    public static readonly Color Brass = new(0.7882f, 0.6353f, 0.2902f);
    public static readonly Color BrassHi = new(0.9098f, 0.7961f, 0.4784f);
    public static readonly Color BrassLo = new(0.5412f, 0.4157f, 0.1647f);
    public static readonly Color Slate = new(0.1098f, 0.1294f, 0.1569f);
    public static readonly Color SlateHi = new(0.1647f, 0.1922f, 0.2314f);
    public static readonly Color SlateLine = new(0.2902f, 0.3216f, 0.3843f);
    public static readonly Color Cream = new(0.9373f, 0.8902f, 0.7843f);
    public static readonly Color CreamMuted = new(0.7255f, 0.6824f, 0.5843f);
    public static readonly Color Olive = new(0.4196f, 0.4784f, 0.2392f);
    public static readonly Color Red = new(0.7216f, 0.2118f, 0.1686f);
    public static readonly Color Amber = new(0.8784f, 0.6588f, 0.2275f);
    public static readonly Color Blue = new(0.3098f, 0.6392f, 0.7804f);
    public static readonly Color Green = new(0.3725f, 0.6196f, 0.2902f);
    public static readonly Color Stamp = new(0.6471f, 0.2157f, 0.1686f);
    public static readonly Color Sa = new(0.8510f, 0.7647f, 0.4157f);
    public static readonly Color He = new(0.8784f, 0.4627f, 0.2275f);
    public static readonly Color Ap = new(0.4980f, 0.6549f, 0.8510f);
    public static readonly Color Aa = new(0.6627f, 0.8510f, 0.9098f);
    public static readonly Color Grey = new(0.6039f, 0.6039f, 0.6039f);
    public static readonly Color Shadow = new(0.0784f, 0.0549f, 0.0314f);

    // Damage-type and armor-class colors are always paired with a glyph
    // (GDD 13.9: nothing is communicated by color alone).
    public static Color ForDamageType(DamageType type) => type switch
    {
        DamageType.SmallArms => Sa,
        DamageType.Explosive => He,
        DamageType.ArmorPiercing => Ap,
        DamageType.AntiAir => Aa,
        _ => Cream,
    };

    public static Color WithAlpha(this Color color, float alpha) => new(color.R, color.G, color.B, alpha);
}
