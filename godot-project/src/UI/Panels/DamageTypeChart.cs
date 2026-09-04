using Godot;
using System.Collections.Generic;
using FrontsOfWar.Combat;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Panels;

// The post-mortem's "damage dealt by type" bars (docs/UI_DESIGN_SPEC.md
// §8.8): four bars in the damage-type token colors, each with its glyph and
// its share, so the player can see at a glance that (say) 6 % of their
// damage was armor-piercing while tanks walked through.
public partial class DamageTypeChart : Control
{
    private static readonly DamageType[] Order = { DamageType.SmallArms, DamageType.Explosive, DamageType.ArmorPiercing, DamageType.AntiAir };

    private readonly Dictionary<DamageType, float> _values = new();
    private float _total;

    public void SetValues(IReadOnlyDictionary<DamageType, float> damageByType)
    {
        _values.Clear();
        _total = 0f;
        foreach (var type in Order)
        {
            float value = damageByType.TryGetValue(type, out var v) ? v : 0f;
            _values[type] = value;
            _total += value;
        }
        QueueRedraw();
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        if (CustomMinimumSize == Vector2.Zero) CustomMinimumSize = new Vector2(320f, 150f);
    }

    public override void _Draw()
    {
        var font = GetThemeFont("font", "PaperSmallLabel");
        int fontSize = GetThemeFontSize("font_size", "PaperSmallLabel");
        float width = Size.X;
        float height = Size.Y;
        const float labelBand = 40f;
        float chartHeight = height - labelBand;
        float column = width / Order.Length;
        float barWidth = column * 0.55f;

        DrawLine(new Vector2(0f, chartHeight), new Vector2(width, chartHeight), UiPalette.InkMuted with { A = 0.6f }, 1f);

        for (int i = 0; i < Order.Length; i++)
        {
            var type = Order[i];
            float share = _total > 0f ? _values[type] / _total : 0f;
            float x = i * column + (column - barWidth) / 2f;
            float barHeight = Mathf.Max(2f, chartHeight * share);
            var color = UiPalette.ForDamageType(type);
            DrawRect(new Rect2(x, chartHeight - barHeight, barWidth, barHeight), color);
            DrawRect(new Rect2(x, chartHeight - barHeight, barWidth, barHeight), UiPalette.Ink with { A = 0.5f }, false, 1f);

            var icon = UiIcons.Get(UiIcons.ForDamageType(type));
            float centerX = i * column + column / 2f;
            if (icon != null)
                DrawTextureRect(icon, new Rect2(centerX - 9f, chartHeight + 4f, 18f, 18f), false, UiPalette.Ink);
            string label = $"{share:P0}";
            DrawString(font, new Vector2(i * column, chartHeight + 36f), label, HorizontalAlignment.Center, column, fontSize, UiPalette.Ink);
        }
    }
}
