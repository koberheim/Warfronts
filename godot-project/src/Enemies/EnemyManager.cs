using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using System.Collections.Generic;

namespace FrontsOfWar.Enemies;

// Owns the lifetime of every live enemy for the current mission. Ticked
// explicitly by MapRuntime as part of GameLoop's deterministic system order
// (GDD §15.4: Time → Spawns → Movement → ...) rather than relying on Godot's
// automatic per-node _PhysicsProcess, which wouldn't respect game speed.
public class EnemyManager
{
    private readonly List<EnemyController> _enemies = new();
    private readonly List<ITargetable> _targetableScratch = new();

    public IReadOnlyList<EnemyController> Enemies => _enemies;

    public void Register(EnemyController enemy) => _enemies.Add(enemy);

    public void Tick(float tickDeltaSeconds)
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            enemy.SimTick(tickDeltaSeconds);

            if (enemy.ReachedEnd)
            {
                EventBus.Instance?.Publish(new EnemyLeakedEvent(enemy, enemy.Definition.LeakCost));
                enemy.QueueFree();
                _enemies.RemoveAt(i);
            }
            else if (!enemy.IsAlive)
            {
                enemy.QueueFree();
                _enemies.RemoveAt(i);
            }
        }
    }

    // Returned list is reused across calls — callers must finish using it
    // before the next Tick(); this avoids a per-tick allocation for what is
    // otherwise a very hot path (rebuilding the targeting SpatialGrid).
    public List<ITargetable> GetTargetables()
    {
        _targetableScratch.Clear();
        foreach (var enemy in _enemies)
            _targetableScratch.Add(enemy);
        return _targetableScratch;
    }
}
