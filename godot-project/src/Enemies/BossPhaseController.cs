using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;

namespace FrontsOfWar.Enemies;

// Owns the B1 Breakthrough Panzer's two readable phases. The boss remains an
// EnemyController for targeting and leak handling; this class keeps the phase
// math out of the general enemy movement code.
public sealed class BossPhaseController
{
    private readonly EnemyDefinition _definition;
    private float _skirtHp;
    private float _addTimer;
    private int _pendingAdds;

    public bool IsActive => _definition.IsBoss;
    public bool IsSkirtIntact => _skirtHp > 0f;
    public float SkirtHp => Mathf.Max(0f, _skirtHp);
    public float SkirtMaxHp { get; }
    public bool HasPendingAdds => _pendingAdds > 0;

    public BossPhaseController(EnemyDefinition definition)
    {
        _definition = definition;
        SkirtMaxHp = Mathf.Max(0f, definition.SkirtHp);
        _skirtHp = SkirtMaxHp;
    }

    public void Tick(float delta)
    {
        if (!IsActive || _definition.AddDefinition == null) return;
        _addTimer += delta;
        if (_addTimer < Mathf.Max(0.1f, _definition.AddIntervalSeconds)) return;
        _addTimer = 0f;
        _pendingAdds += Mathf.Max(0, _definition.AddCount);
    }

    public float ResolveDamage(float baseDamage, DamageType type, bool spotted)
    {
        float damage = DamageResolver.ResolveDamage(
            baseDamage, type, _definition.ArmorClass, spotted, DamageTable.Default);
        if (!IsSkirtIntact) return damage;

        // The skirt is an absorbing layer. Explosive damage strips it three
        // times as quickly; no damage leaks through until the telegraphed
        // phase transition.
        float skirtDamage = damage * (type == DamageType.Explosive
            ? _definition.SkirtExplosiveMultiplier
            : 1f) * (1f - _definition.SkirtDamageReduction);
        _skirtHp = Mathf.Max(0f, _skirtHp - skirtDamage);
        if (_skirtHp <= 0f)
            EventBus.Instance?.Publish(new BossSkirtBrokenEvent());
        return 0f;
    }

    public int ConsumePendingAdds()
    {
        int count = _pendingAdds;
        _pendingAdds = 0;
        return count;
    }

    public float SpeedMultiplier => IsSkirtIntact ? 1f : Mathf.Max(1f, _definition.PostSkirtSpeedMultiplier);
}

public readonly struct BossSkirtBrokenEvent
{
    public BossSkirtBrokenEvent() { }
}
