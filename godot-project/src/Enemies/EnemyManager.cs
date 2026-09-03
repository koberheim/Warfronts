using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System.Collections.Generic;
using System;

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
    public Func<IEnumerable<ISiegeTarget>> SiegeTargetsProvider { get; set; }
    public AirCorridorDefinition AirCorridor { get; set; }

    public void Register(EnemyController enemy)
    {
        _enemies.Add(enemy);
        enemy.SetEnemyProvider(() => _enemies);
    }

    public void ResetSoftBlocks()
    {
        foreach (var enemy in _enemies) enemy.SetSoftBlocked(false);
    }

    public EnemyController Spawn(EnemyDefinition definition, PathNetwork path, Node parent, float hpScale = 1f)
    {
        var instance = definition.ControllerScene.Instantiate<EnemyController>();
        instance.Definition = definition;
        parent.AddChild(instance);
        instance.Initialize(path, hpScale, AirCorridor);
        Register(instance);
        return instance;
    }

    public void Tick(float tickDeltaSeconds)
    {
        float swarmLeadProgress = 0f;
        foreach (var enemy in _enemies)
        {
            if (enemy.Definition.SpecialAbilityId == "swarm_cohesion")
                swarmLeadProgress = Mathf.Max(swarmLeadProgress, enemy.PathProgress);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            enemy.SetCohesionLeadProgress(swarmLeadProgress);
            enemy.SetSiegeHoldDistance(GetSiegeHoldDistance(enemy));
            enemy.SimTick(tickDeltaSeconds);

            if (enemy.ReachedEnd)
            {
                if (enemy.Definition.IsBoss)
                    EventBus.Instance?.Publish(new BossReachedObjectiveEvent(enemy));
                else
                    EventBus.Instance?.Publish(new EnemyLeakedEvent(enemy, enemy.Definition.LeakCost));
                enemy.QueueFree();
                _enemies.RemoveAt(i);
            }
            else if (!enemy.IsAlive)
            {
                enemy.QueueFree();
                _enemies.RemoveAt(i);
            }

            int addCount = enemy.ConsumeBossAddRequest();
            if (addCount > 0)
                EventBus.Instance?.Publish(new BossAddsRequestedEvent(enemy, addCount));
        }
    }

    private float GetSiegeHoldDistance(EnemyController enemy)
    {
        if (enemy.Definition.SpecialAbilityId != "siege_bombard") return float.MaxValue;
        float holdDistance = float.MaxValue;
        float rangePixels = enemy.Definition.SiegeBombardRangeTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var target in SiegeTargetsProvider?.Invoke() ?? Array.Empty<ISiegeTarget>())
        {
            if (target == null || target.IsSiegeImmune) continue;
            float targetDistance = enemy.PathNetwork.GetClosestDistance(target.SiegePosition) - rangePixels;
            if (targetDistance >= enemy.PathDistancePixels)
                holdDistance = Mathf.Min(holdDistance, targetDistance);
        }
        return holdDistance;
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
