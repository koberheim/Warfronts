using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// German timing signature. It applies a temporary, explicit modifier bundle
// to nearby towers; TowerManager resets these values every tick.
public partial class BlitzkriegCommandController : SignatureControllerBase
{
    private Func<IReadOnlyList<TowerController>> _towersProvider;
    private float _activeRemaining;
    private float _rechargeRemaining;

    public bool IsActive => _activeRemaining > 0f;
    public float ActiveRemaining => _activeRemaining;
    public float RechargeRemaining => _rechargeRemaining;

    public override void _Ready() { EnableSignatureInput(); SetupSignatureClickArea(() => TryActivate()); }

    public void Initialize(PathNetwork path, Func<IReadOnlyList<ITargetable>> targetsProvider,
                           Func<IReadOnlyList<TowerController>> towersProvider)
    {
        base.Initialize(path, targetsProvider);
        _towersProvider = towersProvider;
        _activeRemaining = 0f;
        _rechargeRemaining = 0f;
    }

    public bool TryActivate()
    {
        if (Definition == null || IsActive || _rechargeRemaining > 0f) return false;
        _activeRemaining = GetFloat(Definition.BlitzActiveDurations, 12f);
        EventBus.Instance?.Publish(new SignatureActivatedEvent(this, GlobalPosition, Charges));
        return true;
    }

    public bool TrySpendCommandPoints(CommandPointLedger ledger, int commandPoints)
    {
        if (ledger == null || commandPoints < 2 || commandPoints % 2 != 0 ||
            !ledger.TrySpend(commandPoints)) return false;
        _rechargeRemaining = Mathf.Max(0f, _rechargeRemaining - commandPoints / 2f * 4f);
        return true;
    }

    public void SimTick(float delta)
    {
        if (Definition == null) return;
        if (IsActive)
        {
            _activeRemaining -= delta;
            if (_activeRemaining <= 0f)
                _rechargeRemaining = GetFloat(Definition.BlitzRechargeDurations, 30f) * DoctrineRegenMultiplier;
        }
        else if (_rechargeRemaining > 0f) _rechargeRemaining -= delta;

        foreach (var tower in _towersProvider?.Invoke() ?? Array.Empty<TowerController>())
        {
            if (tower == null || GlobalPosition.DistanceSquaredTo(tower.GlobalPosition) >
                Mathf.Pow(GetFloat(Definition.BlitzRadiusTiles, 7.5f) * GameBalanceConfigAutoload.Config.TilePixelSize, 2f)) continue;
            tower.SignatureRateOfFireMultiplier = IsActive ? Definition.BlitzRateOfFireMultiplier : Definition.BlitzPassiveRateOfFireMultiplier;
            if (IsActive)
            {
                tower.SignatureProjectileVelocityMultiplier = Definition.BlitzProjectileVelocityMultiplier;
                tower.SignatureTraverseSpeedMultiplier = Definition.BlitzTraverseSpeedMultiplier;
                tower.SignatureIndirectDelayReductionSeconds = Definition.BlitzIndirectDelayReductionSeconds;
            }
            tower.QueueRedraw();
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawSignatureBase(new Color(0.52f, 0.34f, 0.2f), new Color(1f, 0.7f, 0.25f));
        float radius = GetFloat(Definition?.BlitzRadiusTiles, 7.5f) * GameBalanceConfigAutoload.Config.TilePixelSize;
        DrawArc(Vector2.Zero, radius, 0f, Mathf.Tau, 64, IsActive ? new Color(1f, 0.65f, 0.15f, 0.5f) : new Color(0.7f, 0.55f, 0.25f, 0.25f), 3f);
        if (IsActive) DrawCircle(Vector2.Zero, 10f, new Color(1f, 0.65f, 0.15f, 0.35f));
    }
}
