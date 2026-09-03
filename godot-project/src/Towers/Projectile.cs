using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;

namespace FrontsOfWar.Towers;

// A pooled, leading projectile (GDD §19 prompt 11). The intercept point is
// computed once at launch and flown to in a straight line — correct for the
// game's fast direct-fire shells (e.g. T4's 0.25s flight); indirect-fire
// arcs (T3/T7) will extend this when they land at M2/M5.
public partial class Projectile : Node2D
{
    private ITargetable _target;
    private DamageType _damageType;
    private float _damage;
    private float _blastRadiusPixels;
    private float _hitTolerancePixels;
    private IDamageSource _source;

    private Vector2 _startPoint;
    private Vector2 _impactPoint;
    private float _flightDuration;
    private float _elapsed;

    public bool IsDone { get; private set; }

    public void Launch(ITargetable target, float damage, DamageType damageType,
                        float speedPixelsPerSec, float blastRadiusPixels, Vector2 origin,
                        IDamageSource source = null)
    {
        _target = target;
        _damage = damage;
        _damageType = damageType;
        _blastRadiusPixels = blastRadiusPixels;
        _hitTolerancePixels = 24f;
        _source = source;

        _startPoint = origin;
        GlobalPosition = origin;
        IsDone = false;
        _elapsed = 0f;

        Vector2 predicted = InterceptSolver.PredictInterceptPoint(origin, target.GlobalPosition, target.Velocity, speedPixelsPerSec)
                             ?? target.GlobalPosition;
        _impactPoint = predicted;

        float distance = origin.DistanceTo(_impactPoint);
        _flightDuration = speedPixelsPerSec > 0f ? distance / speedPixelsPerSec : 0f;
    }

    // Indirect-fire archetypes (T3 Field Mortar, T7 Heavy Artillery) target
    // a fixed ground point, not a live unit — no interception math needed,
    // and always resolves as an area hit (see ResolveImpact's blast branch).
    public void LaunchAtPoint(Vector2 impactPoint, float damage, DamageType damageType,
                               float speedPixelsPerSec, float blastRadiusPixels, Vector2 origin,
                               IDamageSource source = null)
    {
        _target = null;
        _damage = damage;
        _damageType = damageType;
        _blastRadiusPixels = blastRadiusPixels;
        _source = source;

        _startPoint = origin;
        _impactPoint = impactPoint;
        GlobalPosition = origin;
        IsDone = false;
        _elapsed = 0f;

        float distance = origin.DistanceTo(impactPoint);
        _flightDuration = speedPixelsPerSec > 0f ? distance / speedPixelsPerSec : 0f;
    }

    // Returns true once this projectile has impacted this tick, so the
    // caller (ProjectileManager) knows to resolve damage and return it to
    // the pool.
    public bool SimTick(float tickDeltaSeconds)
    {
        if (IsDone) return true;

        _elapsed += tickDeltaSeconds;
        float t = _flightDuration > 0f ? Mathf.Clamp(_elapsed / _flightDuration, 0f, 1f) : 1f;
        GlobalPosition = _startPoint.Lerp(_impactPoint, t);

        if (t >= 1f)
        {
            IsDone = true;
            return true;
        }
        return false;
    }

    public void ResolveImpact(SpatialGrid grid)
    {
        if (_blastRadiusPixels > 0f)
        {
            foreach (var hit in grid.QueryRadius(_impactPoint, _blastRadiusPixels))
            {
                if (hit is EnemyController enemy)
                    enemy.ApplyDamage(_damage, _damageType, _source);
                else
                    hit.ApplyDamage(_damage, _damageType);
            }
        }
        else if (_target != null && _target.IsAlive
                 && _target.GlobalPosition.DistanceSquaredTo(_impactPoint) <= _hitTolerancePixels * _hitTolerancePixels)
        {
            if (_target is EnemyController enemy)
                enemy.ApplyDamage(_damage, _damageType, _source);
            else
                _target.ApplyDamage(_damage, _damageType);
        }
        // Otherwise: a clean miss — the target dodged out of the predicted
        // impact point before the shell arrived.
    }
}
