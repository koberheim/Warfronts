using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.UI.Theme;
using System.Collections.Generic;

namespace FrontsOfWar.UI;

// Floating damage numbers, color- and glyph-coded by how effective the hit
// was (GDD §5.7 point 3 - "the single most important teaching tool in the
// entire game"). A grey number with a "▼" prefix for an ineffective hit is
// how the player learns "my machine guns are failing against that tank"
// without reading a tooltip. Pure cosmetic feedback, so this runs on
// Godot's regular _Process rather than the fixed sim tick. Colors follow
// UI spec §9 (strong = HE orange, partial = cream, ineffective = grey).
public partial class DamageNumberSpawner : Node2D
{
    private const float RiseSpeed = 30f; // px/sec
    private const float LifetimeSeconds = 0.9f;

    private class FloatingNumber
    {
        public Label Label;
        public float Elapsed;
    }

    private readonly List<FloatingNumber> _active = new();
    private Vector2 _labelScale = Vector2.One;

    public override void _Ready()
    {
        // World-space labels are magnified by the table camera; counter it so
        // the number reads at the spec's 18 px on screen.
        var camera = GetViewport()?.GetCamera2D();
        if (camera != null && camera.Zoom.X > 0f) _labelScale = Vector2.One / camera.Zoom;
        EventBus.Instance?.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
    }

    private void OnEnemyDamaged(EnemyDamagedEvent evt)
    {
        var label = new Label
        {
            ThemeTypeVariation = "DamageNumberLabel",
            Text = FormatText(evt.DamageDealt, evt.Multiplier),
            GlobalPosition = evt.Enemy.GlobalPosition + new Vector2(0f, -40f),
            Scale = _labelScale,
            ZIndex = 100,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeColorOverride("font_color", ColorFor(evt.Multiplier));
        AddChild(label);
        _active.Add(new FloatingNumber { Label = label });
    }

    public override void _Process(double delta)
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var entry = _active[i];
            entry.Elapsed += (float)delta;

            if (entry.Elapsed >= LifetimeSeconds)
            {
                entry.Label.QueueFree();
                _active.RemoveAt(i);
                continue;
            }

            entry.Label.GlobalPosition -= new Vector2(0f, RiseSpeed * (float)delta);
            entry.Label.Modulate = entry.Label.Modulate with { A = 1f - entry.Elapsed / LifetimeSeconds };
        }
    }

    private static string FormatText(float damage, float multiplier)
        => multiplier < 0.3f ? $"▼{damage:F0}" : $"{damage:F0}";

    private static Color ColorFor(float multiplier) => multiplier switch
    {
        < 0.3f => UiPalette.Grey,   // ineffective
        < 1.0f => UiPalette.Cream,  // partial
        _ => UiPalette.He,          // strong
    };
}
