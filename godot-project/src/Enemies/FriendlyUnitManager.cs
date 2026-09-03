using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System.Collections.Generic;

namespace FrontsOfWar.Enemies;

public class FriendlyUnitManager
{
    private readonly Node _parent;
    private readonly List<FriendlyUnitController> _units = new();
    private int _nextId;

    public IReadOnlyList<FriendlyUnitController> Units => _units;
    public int LivingCount => _units.Count;

    public FriendlyUnitManager(Node parent) => _parent = parent;

    public FriendlyUnitController Spawn(FriendlyUnitDefinition definition, PackedScene scene, PathNetwork path, float distance)
    {
        var unit = scene.Instantiate<FriendlyUnitController>();
        unit.Name = $"Friendly_{definition.Id}_{_nextId++}";
        _parent.AddChild(unit);
        unit.Initialize(definition, path, distance);
        _units.Add(unit);
        return unit;
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
                unit.QueueFree();
                _units.RemoveAt(i);
            }
        }
    }
}
