using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Italian burst signature. Units are intentionally autonomous and rendered
// as outlined, lower-layer primitives until the held nation-unit art pass.
public partial class BersaglieriChargeController : SignatureControllerBase, IDamageSource
{
    private sealed class AssaultUnit
    {
        public float Distance;
        public float Life;
        public bool GrenadeThrown;
    }

    private readonly List<AssaultUnit> _units = new();
    private float _cooldownRemaining;
    private Func<IReadOnlyList<ITargetable>> _provider;

    public string SourceId => $"bersaglieri_charge_{Name}";
    public int LivingUnitCount => _units.Count;
    public float DeploymentCooldownRemaining => Mathf.Max(0f, _cooldownRemaining);

    public override void _Ready() { EnableSignatureInput(); SetupSignatureClickArea(() => TryDeploy()); }

    public override void Initialize(Map.PathNetwork path, Func<IReadOnlyList<ITargetable>> targetsProvider)
    {
        base.Initialize(path, targetsProvider);
        _provider = targetsProvider;
        Charges = 0;
        _cooldownRemaining = 0f;
    }

    public bool TryDeploy()
    {
        if (Path == null || _cooldownRemaining > 0f ||
            _units.Count + GetInt(Definition.BersaglieriSquadSizes, 4) > Definition.BersaglieriMaxUnits) return false;
        int count = GetInt(Definition.BersaglieriSquadSizes, 4);
        float start = Path.GetClosestDistance(GlobalPosition);
        for (int i = 0; i < count; i++)
            _units.Add(new AssaultUnit { Distance = Mathf.Max(0f, start - i * 8f), Life = Definition.BersaglieriLifetimeSeconds });
        _cooldownRemaining = GetFloat(Definition.BersaglieriDeploymentIntervals, 18f);
        EventBus.Instance?.Publish(new SignatureActivatedEvent(this, GlobalPosition, 0));
        return true;
    }

    public void SimTick(float delta)
    {
        if (Definition == null || Path == null) return;
        _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - delta);
        var targets = _provider?.Invoke();
        for (int i = _units.Count - 1; i >= 0; i--)
        {
            var unit = _units[i];
            unit.Life -= delta;
            if (unit.Life <= 0f) { _units.RemoveAt(i); continue; }
            EnemyController target = FindNearestEnemy(unit, targets);
            if (target == null)
            {
                unit.Distance = Mathf.Max(0f, unit.Distance - Definition.BersaglieriSpeedMultiplier *
                    GameBalanceConfigAutoload.Config.TilePixelSize * delta);
                continue;
            }

            float distance = Path.GetClosestDistance(target.GlobalPosition);
            unit.Distance = Mathf.Max(0f, distance);
            Vector2 unitPosition = Path.GetPositionAtDistance(unit.Distance);
            if (unitPosition.DistanceTo(target.GlobalPosition) > Definition.BersaglieriSuppressionRadiusTiles *
                GameBalanceConfigAutoload.Config.TilePixelSize) continue;

            if (!unit.GrenadeThrown)
            {
                unit.GrenadeThrown = true;
                SignatureTargeting.ApplyBlast(targets, target.GlobalPosition,
                    Definition.BersaglieriGrenadeBlastRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize,
                    Definition.BersaglieriGrenadeDamage, DamageType.Explosive, this);
            }
            SignatureTargeting.ApplyDamage(target, 18f * delta, DamageType.SmallArms, this);
            foreach (var nearby in targets ?? Array.Empty<ITargetable>())
            {
                if (nearby is not EnemyController enemy || !enemy.IsAlive || enemy.IsAir ||
                    enemy.GlobalPosition.DistanceTo(unitPosition) > Definition.BersaglieriSuppressionRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize) continue;
                enemy.ApplySuppressed(GameBalanceConfigAutoload.Config.SuppressedDurationSeconds, 4f);
                if (CurrentLevel >= 3) enemy.ApplySpotted(Definition.BersaglieriSpottedDurationSeconds);
            }
        }
        QueueRedraw();
    }

    private EnemyController FindNearestEnemy(AssaultUnit unit, IReadOnlyList<ITargetable> targets)
    {
        EnemyController best = null;
        float bestDistance = float.MaxValue;
        Vector2 origin = Path.GetPositionAtDistance(unit.Distance);
        foreach (var target in targets ?? Array.Empty<ITargetable>())
        {
            if (target is not EnemyController enemy || !enemy.IsAlive || enemy.IsAir ||
                (enemy.IsConcealed && !enemy.IsRevealed && CurrentLevel < 3)) continue;
            float distance = origin.DistanceSquaredTo(enemy.GlobalPosition);
            if (distance < bestDistance) { bestDistance = distance; best = enemy; }
        }
        return best;
    }

    public override void _Draw()
    {
        DrawSignatureBase(new Color(0.62f, 0.38f, 0.2f), new Color(0.95f, 0.83f, 0.4f));
        foreach (var unit in _units)
        {
            if (Path == null) continue;
            Vector2 position = ToLocal(Path.GetPositionAtDistance(unit.Distance));
            DrawCircle(position, 9f, new Color(0.82f, 0.86f, 0.9f));
            DrawArc(position, 11f, 0f, Mathf.Tau, 16, Colors.White, 2f);
            DrawLine(position + new Vector2(-12f, 13f), position + new Vector2(12f, 13f), new Color(0.78f, 0.62f, 0.3f, 0.5f), 2f);
        }
    }
}
