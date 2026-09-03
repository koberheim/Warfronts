using FrontsOfWar.Combat;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Owns every placed tower for the current mission and ticks them in
// lockstep with GameLoop (GDD §15.4's system order: ... → Targeting →
// Firing → ...).
public class TowerManager
{
    private readonly List<TowerController> _towers = new();

    public IReadOnlyList<TowerController> Towers => _towers;

    public void Register(TowerController tower) => _towers.Add(tower);
    public void Unregister(TowerController tower) => _towers.Remove(tower);

    public void Tick(float tickDeltaSeconds, SpatialGrid grid, ProjectileManager projectileManager)
    {
        foreach (var tower in _towers)
            tower.SimTick(tickDeltaSeconds, grid, projectileManager);
    }
}
