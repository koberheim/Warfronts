using Godot;
using System.Collections.Generic;

namespace FrontsOfWar.Combat;

// Uniform spatial grid rebuilt once per tick (GDD §15.4). Never use
// Area2D/PhysicsServer2D overlap queries per tower per frame — this grid
// exists specifically to keep 40 towers querying 200 enemies under budget.
public class SpatialGrid
{
    private readonly float _cellSize;
    private readonly Dictionary<(int, int), List<ITargetable>> _cells = new();

    public SpatialGrid(float cellSizePixels)
    {
        _cellSize = cellSizePixels;
    }

    public void Rebuild(IReadOnlyList<ITargetable> targets)
    {
        _cells.Clear();
        foreach (var target in targets)
        {
            if (!target.IsAlive) continue;
            var cell = CellOf(target.GlobalPosition);
            if (!_cells.TryGetValue(cell, out var list))
            {
                list = new List<ITargetable>();
                _cells[cell] = list;
            }
            list.Add(target);
        }
    }

    // Queries the 3x3 block of cells (9 cells) covering a circle of the
    // given radius around origin, then filters to the exact radius.
    public IEnumerable<ITargetable> QueryRadius(Vector2 origin, float radiusPixels)
    {
        var (cx, cy) = CellOf(origin);
        int cellRadius = Mathf.CeilToInt(radiusPixels / _cellSize);
        float radiusSquared = radiusPixels * radiusPixels;

        for (int dx = -cellRadius; dx <= cellRadius; dx++)
        {
            for (int dy = -cellRadius; dy <= cellRadius; dy++)
            {
                if (!_cells.TryGetValue((cx + dx, cy + dy), out var list)) continue;
                foreach (var target in list)
                {
                    if (origin.DistanceSquaredTo(target.GlobalPosition) <= radiusSquared)
                        yield return target;
                }
            }
        }
    }

    private (int, int) CellOf(Vector2 position)
        => (Mathf.FloorToInt(position.X / _cellSize), Mathf.FloorToInt(position.Y / _cellSize));
}
