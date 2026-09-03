using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;

namespace FrontsOfWar.Enemies;

public partial class FriendlyUnitController : Node2D, IDamageSource
{
    [Export] public FriendlyUnitDefinition Definition;

    private PathNetwork _path;
    private float _distance;
    private float _hp;
    private float _lifetime;
    private EnemyController _engagedEnemy;
    private float _engagedSeconds;

    public string SourceId => $"friendly_{Name}";
    public bool IsAlive => _hp > 0f;
    public float CurrentHp => _hp;
    public bool IsEngaged => _engagedEnemy is { IsAlive: true };

    public void Initialize(FriendlyUnitDefinition definition, PathNetwork path, float startDistance)
    {
        Definition = definition;
        _path = path;
        _distance = Mathf.Clamp(startDistance, 0f, path.LengthPixels);
        _hp = definition.MaxHp;
        GlobalPosition = path.GetPositionAtDistance(_distance);
        QueueRedraw();
    }

    public void SimTick(float delta, System.Collections.Generic.IReadOnlyList<EnemyController> enemies)
    {
        if (!IsAlive) return;
        _lifetime += delta;
        if (_lifetime >= Definition.LifetimeSeconds) { _hp = 0f; return; }

        if (_engagedEnemy == null || !_engagedEnemy.IsAlive)
            _engagedEnemy = FindNearestEnemy(enemies);

        if (_engagedEnemy != null && _engagedEnemy.IsAlive)
        {
            float distance = GlobalPosition.DistanceTo(_engagedEnemy.GlobalPosition);
            if (distance <= GameBalanceConfigAutoload.Config.TilePixelSize * 0.45f)
            {
                _engagedSeconds += delta;
                _engagedEnemy.SetSoftBlocked(_engagedSeconds < 3f);
                _engagedEnemy.ApplyDamage(Definition.DamagePerSecond * delta, Definition.DamageType, this);
                QueueRedraw();
                return;
            }
        }

        _engagedSeconds = 0f;
        float speed = Definition.MoveSpeedTilesPerSec * GameBalanceConfigAutoload.Config.TilePixelSize;
        _distance = Mathf.Max(0f, _distance - speed * delta);
        GlobalPosition = _path.GetPositionAtDistance(_distance);
        QueueRedraw();
    }

    public void ApplyDamage(float amount)
    {
        if (!IsAlive || amount <= 0f) return;
        _hp = Mathf.Max(0f, _hp - amount);
        QueueRedraw();
    }

    private EnemyController FindNearestEnemy(System.Collections.Generic.IReadOnlyList<EnemyController> enemies)
    {
        EnemyController best = null;
        float bestDistance = float.MaxValue;
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            float distance = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (distance < bestDistance) { bestDistance = distance; best = enemy; }
        }
        return best;
    }

    public override void _Draw()
    {
        const float width = 22f;
        DrawCircle(Vector2.Zero, 13f, new Color(0.82f, 0.86f, 0.9f, 1f));
        DrawArc(Vector2.Zero, 15f, 0f, Mathf.Tau, 32, Colors.White, 3f);
        DrawRect(new Rect2(-4f, -18f, 8f, 6f), new Color(0.35f, 0.65f, 0.95f));
        DrawRect(new Rect2(-width / 2f, 18f, width, 3f), new Color(0.1f, 0.1f, 0.1f));
        DrawRect(new Rect2(-width / 2f, 18f, width * (_hp / Mathf.Max(1f, Definition?.MaxHp ?? 1f)), 3f), Colors.White);
    }
}
