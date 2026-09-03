using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.UI;

// Floating damage numbers, color- and glyph-coded by how effective the hit
// was (GDD §5.7 point 3 — "the single most important teaching tool in the
// entire game"). A grey number with a "▼" prefix for an ineffective hit is
// how the player learns "my machine guns are failing against that tank"
// without reading a tooltip. Pure cosmetic feedback, so this runs on
// Godot's regular _Process rather than the fixed sim tick.
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

    public override void _Ready()
    {
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
            Text = FormatText(evt.DamageDealt, evt.Multiplier),
            Modulate = ColorFor(evt.Multiplier),
            GlobalPosition = evt.Enemy.GlobalPosition + new Vector2(0f, -40f),
            ZIndex = 100,
        };
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
        < 0.3f => new Color(0.65f, 0.65f, 0.65f), // ineffective — grey
        < 1.0f => new Color(0.9f, 0.9f, 0.85f),   // partial — white
        _ => new Color(0.95f, 0.55f, 0.15f),      // strong — orange
    };
}
