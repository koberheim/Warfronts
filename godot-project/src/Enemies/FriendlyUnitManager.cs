using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Enemies;

public class FriendlyUnitManager
{
    private readonly Node _parent;
    private readonly List<FriendlyUnitController> _units = new();
    private readonly Dictionary<PackedScene, ObjectPool<FriendlyUnitController>> _pools = new();
    private readonly Dictionary<FriendlyUnitController, ObjectPool<FriendlyUnitController>> _leases = new();
    private readonly Queue<(FriendlyUnitDefinition Definition, PackedScene Scene, PathNetwork Path, float Distance)> _pending = new();
    private bool _prepared;
    private int _nextId;
    public IReadOnlyList<FriendlyUnitController> Units => _units;
    public int LivingCount => _units.Count;
    public int PendingCount => _pending.Count;

    public FriendlyUnitManager(Node parent) => _parent = parent;

    public void Prepare(IEnumerable<PackedScene> scenes, GameBalanceConfig config)
    {
        if (_pools.Count > 0) throw new InvalidOperationException("Prepare friendly pools before spawning.");
        int capacity = Mathf.Max(1, config.FriendlyPoolHardCapacityPerScene);
        foreach (var scene in scenes)
        {
            if (scene == null || _pools.ContainsKey(scene)) continue;
            var pool = new ObjectPool<FriendlyUnitController>(scene, _parent, capacity, capacity);
            pool.FreezeCapacity();
            _pools.Add(scene, pool);
        }
        _prepared = true;
    }

    public FriendlyUnitController Spawn(FriendlyUnitDefinition definition, PackedScene scene, PathNetwork path, float distance)
    {
        if (definition == null || scene == null || path == null) throw new ArgumentException("A friendly spawn requires definition, scene and path.");
        if (!_pools.TryGetValue(scene, out var pool))
        {
            if (_prepared) throw new InvalidOperationException("Friendly scene was not prepared before combat.");
            pool = new ObjectPool<FriendlyUnitController>(scene, _parent);
            _pools.Add(scene, pool);
        }
        if (_pending.Count > 0 || !pool.TryRent(out var unit))
        {
            _pending.Enqueue((definition, scene, path, distance));
            return null;
        }
        Activate(unit, pool, definition, path, distance);
        return unit;
    }

    private void Activate(FriendlyUnitController unit, ObjectPool<FriendlyUnitController> pool,
        FriendlyUnitDefinition definition, PathNetwork path, float distance)
    {
        unit.Name = $"Friendly_{definition.Id}_{_nextId++}";
        unit.Initialize(definition, path, distance);
        _units.Add(unit);
        _leases.Add(unit, pool);
    }

    public void Tick(float delta, EnemyManager enemies)
    {
        enemies.ResetSoftBlocks();
        for (int i = _units.Count - 1; i >= 0; i--)
        {
            var unit = _units[i];
            unit.SimTick(delta, enemies.Enemies);
            if (!unit.IsAlive)
            {
                _units.RemoveAt(i);
                var pool = _leases[unit];
                _leases.Remove(unit);
                pool.Return(unit);
            }
        }
        while (_pending.Count > 0)
        {
            var next = _pending.Peek();
            var pool = _pools[next.Scene];
            if (!pool.TryRent(out var unit)) break;
            _pending.Dequeue();
            Activate(unit, pool, next.Definition, next.Path, next.Distance);
        }
    }
}
