using Godot;
using FrontsOfWar.UI.Theme;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Enemies;

public partial class EnemyController
{
    // Health bar only appears once damaged (GDD §13.6 — reduces clutter),
    // always carries the armor-class glyph on its left cap when visible,
    // and status badges to the right. Never color alone — glyph shapes
    // differ by armor class, matching the accessibility rule in §13.9.
    public override void _Draw()
    {
        if (!IsAlive || Definition == null || _maxHp <= 0f) return;

        const float barWidth = 42f;
        const float barHeight = 4f;
        const float yOffset = -30f;
        float fraction = _currentHp / _maxHp;

        if (Definition.IsBoss || _currentHp < _maxHp)
        {
            DrawRect(new Rect2(-barWidth / 2f, yOffset, barWidth, barHeight), UiPalette.Slate with { A = 0.9f });
            DrawRect(new Rect2(-barWidth / 2f, yOffset, barWidth * fraction, barHeight), UiPalette.Red);
        }

        if (BossPhase is { IsSkirtIntact: true })
        {
            float skirtFraction = BossPhase.SkirtMaxHp > 0f ? BossPhase.SkirtHp / BossPhase.SkirtMaxHp : 0f;
            DrawRect(new Rect2(-barWidth / 2f, yOffset - 6f, barWidth, barHeight), UiPalette.Slate with { A = 0.9f });
            DrawRect(new Rect2(-barWidth / 2f, yOffset - 6f, barWidth * skirtFraction, barHeight), UiPalette.Amber);
        }

        DrawArmorGlyph(new Vector2(-barWidth / 2f - 7f, yOffset + barHeight / 2f));

        float badgeX = barWidth / 2f + 6f;
        float badgeY = yOffset + barHeight / 2f;
        if (Status.IsSuppressed)
        {
            DrawCircle(new Vector2(badgeX, badgeY), 3f, UiPalette.Grey);
            badgeX += 8f;
        }
        if (Status.IsSpotted)
            DrawArc(new Vector2(badgeX, badgeY), 3f, 0f, Mathf.Tau, 12, UiPalette.Red, 1.5f, true);
        if (Definition.Archetype == EnemyArchetype.Escort && _shieldRemaining > 0f)
        {
            float radius = Definition.EscortShieldRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
            var bubble = new Vector2[7];
            for (int i = 0; i < bubble.Length; i++)
                bubble[i] = new Vector2(radius, 0f).Rotated(i * Mathf.Tau / bubble.Length);
            DrawPolyline(bubble, UiPalette.Blue with { A = 0.28f }, 2f, true);
            DrawRect(new Rect2(-21f, 24f, 42f, 3f), UiPalette.Slate with { A = 0.9f });
            DrawRect(new Rect2(-21f, 24f, 42f * (_shieldRemaining / Mathf.Max(1f, Definition.EscortShieldMaxHp)), 3f), UiPalette.Blue);
        }
        if (_repairTarget != null && _repairTarget.IsAlive)
            DrawLine(Vector2.Zero, ToLocal(_repairTarget.GlobalPosition), UiPalette.Green with { A = 0.9f }, 3f);
        if (IsAir)
        {
            DrawLine(new Vector2(-20f, 10f), new Vector2(20f, 10f), new Color(0.15f, 0.15f, 0.2f, 0.35f), 8f);
            DrawColoredPolygon(new[] { new Vector2(-18f, 0f), new Vector2(18f, 0f), new Vector2(0f, -8f) }, new Color(0.8f, 0.82f, 0.86f));
        }
        if (Definition.Archetype == EnemyArchetype.Support)
        {
            DrawRect(new Rect2(-15f, -10f, 30f, 20f), new Color(0.42f, 0.54f, 0.58f));
            DrawLine(new Vector2(0f, -10f), new Vector2(10f, -22f), new Color(0.75f, 0.78f, 0.68f), 3f);
        }
        if (Definition.Archetype == EnemyArchetype.Escort)
        {
            DrawRect(new Rect2(-17f, -11f, 34f, 22f), new Color(0.48f, 0.5f, 0.58f));
            DrawLine(new Vector2(-12f, -15f), new Vector2(-6f, 15f), new Color(0.75f, 0.78f, 0.84f), 3f);
            DrawLine(new Vector2(12f, -15f), new Vector2(6f, 15f), new Color(0.75f, 0.78f, 0.84f), 3f);
        }
        if (Definition.Archetype == EnemyArchetype.Recon)
        {
            DrawCircle(Vector2.Zero, 8f, new Color(0.65f, 0.7f, 0.72f, 0.45f));
            for (int i = 0; i < 8; i += 2)
                DrawArc(Vector2.Zero, 12f, i * Mathf.Tau / 8f, (i + 1) * Mathf.Tau / 8f, 4, new Color(0.75f, 0.8f, 0.82f, 0.7f), 2f);
        }
    }

    // Deliberately distinct shapes, not just colors (Soft: square, Hardened:
    // small circle, Armored: larger circle, Heavy: diamond) — GDD §5.3's
    // "cloth square / half shield / full shield / double shield" reading,
    // approximated with primitives until real icon art exists.
    private void DrawArmorGlyph(Vector2 center)
    {
        switch (Definition.ArmorClass)
        {
            case ArmorClass.Soft:
                DrawRect(new Rect2(center - new Vector2(2.5f, 2.5f), new Vector2(5f, 5f)), new Color(0.85f, 0.85f, 0.8f));
                break;
            case ArmorClass.Hardened:
                DrawCircle(center, 3f, new Color(0.75f, 0.75f, 0.75f));
                break;
            case ArmorClass.Armored:
                DrawCircle(center, 4f, new Color(0.7f, 0.72f, 0.78f));
                break;
            case ArmorClass.Heavy:
                var points = new[]
                {
                    center + new Vector2(0f, -5f), center + new Vector2(5f, 0f),
                    center + new Vector2(0f, 5f), center + new Vector2(-5f, 0f),
                };
                DrawColoredPolygon(points, new Color(0.85f, 0.7f, 0.3f));
                break;
        }
    }
}
