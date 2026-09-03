using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Runtime minefield trigger. Unlike tower targeting, this proximity trap
// deliberately does not filter concealed targets; that is the E11 discovery
// described in the GDD.
public partial class MinefieldController : Node2D
{
    [Export] public TowerDefinition Definition;
    [Export(PropertyHint.Range, "1,4,1")] public int Level = 1;
    [Export] public PadTag PadTag = PadTag.Standard;

    private TowerUpgradeController _upgrade;
    private Func<IReadOnlyList<ITargetable>> _provider;
    private int _charges;
    private float _regenElapsed;
    private float _armingRemaining;

    public int Charges => _charges;
    public int MaxCharges => _upgrade?.CurrentStats().MaxCharges ?? 0;

    public void Initialize(Func<IReadOnlyList<ITargetable>> provider)
    {
        _provider = provider;
        _upgrade = new TowerUpgradeController(Definition, GameBalanceConfigAutoload.Config);
        for (int i = 1; i < Level; i++) _upgrade.Upgrade();
        _charges = MaxCharges;
        QueueRedraw();
    }

    public void SimTick(float delta)
    {
        if (_upgrade == null) return;
        _upgrade.Tick(delta);
        var stats = _upgrade.CurrentStats();
        _armingRemaining = Mathf.Max(0f, _armingRemaining - delta);
        if (_charges < stats.MaxCharges)
        {
            _regenElapsed += delta;
            while (_regenElapsed >= stats.ChargeRegenSeconds && stats.ChargeRegenSeconds > 0f)
            {
                _regenElapsed -= stats.ChargeRegenSeconds;
                _charges = Mathf.Min(stats.MaxCharges, _charges + 1);
            }
        }
        if (_armingRemaining <= 0f && _charges > 0)
        {
            foreach (var target in _provider?.Invoke() ?? Array.Empty<ITargetable>())
            {
                if (target == null || !target.IsAlive || target.IsAir ||
                    target.GlobalPosition.DistanceTo(GlobalPosition) > stats.TriggerRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize) continue;
                Trigger(stats);
                break;
            }
        }
        QueueRedraw();
    }

    public bool TriggerNow()
    {
        if (_upgrade == null || _charges <= 0 || _armingRemaining > 0f) return false;
        Trigger(_upgrade.CurrentStats());
        return true;
    }

    private void Trigger(TowerStatBlock stats)
    {
        _charges--;
        _armingRemaining = stats.TriggerArmingSeconds;
        var targets = _provider?.Invoke();
        DamageType damageType = stats.UsesDamageTypeOverride ? stats.DamageTypeOverride : Definition.DamageType;
        SignatureTargeting.ApplyBlast(targets, GlobalPosition,
            stats.BlastRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize,
            stats.DamagePerShot, damageType, null);
        if (stats.StatusEffectId == "Suppressed")
            foreach (var target in targets ?? Array.Empty<ITargetable>())
                if (target is EnemyController enemy && enemy.IsAlive && !enemy.IsAir &&
                    enemy.GlobalPosition.DistanceTo(GlobalPosition) <= stats.BlastRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize)
                    enemy.ApplySuppressed(stats.StatusDurationSeconds, 4f);
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 12f, new Color(0.35f, 0.46f, 0.3f, 0.65f));
        DrawArc(Vector2.Zero, 14f, 0f, Mathf.Tau, 24, new Color(0.85f, 0.75f, 0.3f), 2f);
        if (_upgrade != null)
            DrawRect(new Rect2(-16f, 16f, 32f, 4f), new Color(0.1f, 0.1f, 0.1f));
        if (_upgrade != null)
            DrawRect(new Rect2(-16f, 16f, 32f * _charges / Mathf.Max(1f, MaxCharges), 4f), new Color(0.85f, 0.75f, 0.3f));
    }
}
