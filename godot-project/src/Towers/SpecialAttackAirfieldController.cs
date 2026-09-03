using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Economy;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Japanese stored-strike signature. It is ground-only and never depicts a
// pilot or self-destruction; the presentation is an abstract air-raid marker.
public partial class SpecialAttackAirfieldController : SignatureControllerBase, IDamageSource
{
    private sealed class Strike
    {
        public Vector2 Point;
        public float Elapsed;
    }

    private readonly List<Strike> _strikes = new();
    private float _chargeElapsed;
    private Func<IReadOnlyList<ITargetable>> _provider;

    public string SourceId => $"special_attack_airfield_{Name}";
    public int StoredCharges => Charges;
    public int StoredChargeCapacity => ChargeCapacity;
    public int ActiveStrikeCount => _strikes.Count;

    protected override int ChargeCapacity => GetInt(Definition?.AirfieldStoredChargeCaps, 2);

    public override void _Ready() { EnableSignatureInput(); SetupSignatureClickArea(); }

    public override void Initialize(Map.PathNetwork path, Func<IReadOnlyList<ITargetable>> targetsProvider)
    {
        base.Initialize(path, targetsProvider);
        _provider = targetsProvider;
        _chargeElapsed = 0f;
        _strikes.Clear();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion && IsTargeting) { QueueRedraw(); return; }
        if (!TryGetTargetClick(@event, out Vector2 point)) return;
        if (@event is InputEventKey) return;
        TryActivateAtPoint(point);
        GetViewport().SetInputAsHandled();
    }

    public bool TrySpendCommandPoints(CommandPointLedger ledger)
    {
        if (ledger == null || !ledger.TrySpend(Definition.AirfieldCommandPointCost)) return false;
        _chargeElapsed += Definition.AirfieldCommandPointProgress;
        CompleteReadyCharges();
        return true;
    }

    public bool TryActivateAtPoint(Vector2 point)
    {
        if (!TrySpendCharges(1)) return false;
        _strikes.Add(new Strike { Point = point });
        EventBus.Instance?.Publish(new SignatureActivatedEvent(this, point, Charges));
        EventBus.Instance?.Publish(new SignatureTelegraphEvent(this, point, Definition.AirfieldApproachSeconds));
        return true;
    }

    public void SimTick(float delta)
    {
        if (Definition == null) return;
        _chargeElapsed += delta;
        CompleteReadyCharges();
        for (int i = _strikes.Count - 1; i >= 0; i--)
        {
            var strike = _strikes[i];
            strike.Elapsed += delta;
            if (strike.Elapsed < Definition.AirfieldApproachSeconds) continue;
            ResolveStrike(strike.Point);
            _strikes.RemoveAt(i);
        }
        QueueRedraw();
    }

    private void CompleteReadyCharges()
    {
        float interval = GetFloat(Definition.AirfieldChargeIntervals, 40f);
        while (Charges < ChargeCapacity && _chargeElapsed >= interval)
        {
            _chargeElapsed -= interval;
            Charges++;
        }
        if (Charges >= ChargeCapacity) _chargeElapsed = 0f;
    }

    private void ResolveStrike(Vector2 point)
    {
        float damage = Definition.AirfieldStrikeDamage + (CurrentLevel >= 2 ? Definition.AirfieldLevelTwoDamageBonus : 0f);
        float radius = (Definition.AirfieldBlastRadiusTiles + (CurrentLevel >= 3 ? Definition.AirfieldLevelThreeBlastBonusTiles : 0f)) *
            GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var target in _provider?.Invoke() ?? Array.Empty<ITargetable>())
        {
            if (target == null || !target.IsAlive || target.IsAir || target.GlobalPosition.DistanceTo(point) > radius) continue;
            SignatureTargeting.ApplyDamage(target, damage * Definition.AirfieldArmorPiercingFraction, DamageType.ArmorPiercing, this);
            SignatureTargeting.ApplyDamage(target, damage * Definition.AirfieldExplosiveFraction, DamageType.Explosive, this);
            if (CurrentLevel >= 3 && target is EnemyController enemy && enemy.IsAlive)
                enemy.ApplySpotted(Definition.AirfieldSpottedDurationSeconds);
        }
    }

    public override void _Draw()
    {
        DrawSignatureBase(new Color(0.42f, 0.32f, 0.25f), new Color(0.95f, 0.72f, 0.3f));
        foreach (var strike in _strikes)
        {
            DrawLine(Vector2.Zero, ToLocal(strike.Point), new Color(0.95f, 0.78f, 0.35f, 0.7f), 3f, true);
            float radius = (Definition.AirfieldBlastRadiusTiles + (CurrentLevel >= 3 ? Definition.AirfieldLevelThreeBlastBonusTiles : 0f)) * GameBalanceConfigAutoload.Config.TilePixelSize;
            DrawArc(ToLocal(strike.Point), radius, 0f, Mathf.Tau, 48, new Color(1f, 0.45f, 0.2f, 0.75f), 3f);
        }
        if (IsTargeting)
        {
            Vector2 point = GetGlobalMousePosition();
            DrawLine(Vector2.Zero, ToLocal(point), new Color(0.95f, 0.8f, 0.35f, 0.6f), 2f, true);
        }
    }
}
