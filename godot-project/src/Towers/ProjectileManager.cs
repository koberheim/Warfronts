using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Pools and ticks every in-flight projectile (GDD §19 prompt 11, §15.1
// principle 5 — never PackedScene.Instantiate() mid-wave). One ObjectPool
// per distinct projectile scene, since different tower archetypes use
// different projectile visuals.
public class ProjectileManager
{
    private readonly Node _poolParent;
    private readonly Dictionary<PackedScene, ObjectPool<Projectile>> _pools = new();
    private readonly List<(Projectile Projectile, ObjectPool<Projectile> Pool)> _active = new();

    public ProjectileManager(Node poolParent)
    {
        _poolParent = poolParent;
    }

    public void Spawn(TowerDefinition definition, TowerStatBlock stats, Vector2 origin,
                      ITargetable target, IDamageSource source)
    {
        var pool = GetOrCreatePool(definition.ProjectileScene);
        var projectile = pool.Rent();

        float tilePixelSize = GameBalanceConfigAutoload.Config.TilePixelSize;
        float speedPixelsPerSec = definition.ProjectileSpeedTilesPerSec * tilePixelSize;
        float blastRadiusPixels = stats.BlastRadiusTiles * tilePixelSize;

        projectile.Launch(target, stats.DamagePerShot * stats.DamageMultiplier, definition.DamageType,
            speedPixelsPerSec, blastRadiusPixels, origin, source);
        _active.Add((projectile, pool));
    }

    // Indirect-fire archetypes (T3, T7) firing at a ground point rather than
    // a live target — see TowerController.FireAtDensestCluster. Flight time
    // is still approximated as constant-speed straight-line travel (a
    // simplification vs. GDD's described shell arc — acceptable for now,
    // since only the flight *duration* is mechanically load-bearing).
    public void SpawnAtPoint(TowerDefinition definition, TowerStatBlock stats, Vector2 origin,
                             Vector2 impactPoint, IDamageSource source)
    {
        var pool = GetOrCreatePool(definition.ProjectileScene);
        var projectile = pool.Rent();

        float tilePixelSize = GameBalanceConfigAutoload.Config.TilePixelSize;
        float speedPixelsPerSec = definition.ProjectileSpeedTilesPerSec * tilePixelSize;
        float blastRadiusPixels = stats.BlastRadiusTiles * tilePixelSize;

        projectile.LaunchAtPoint(impactPoint, stats.DamagePerShot * stats.DamageMultiplier, definition.DamageType,
            speedPixelsPerSec, blastRadiusPixels, origin, source);
        _active.Add((projectile, pool));
    }

    public void Tick(float tickDeltaSeconds, SpatialGrid grid)
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var (projectile, pool) = _active[i];
            if (!projectile.SimTick(tickDeltaSeconds)) continue;

            projectile.ResolveImpact(grid);
            pool.Return(projectile);
            _active.RemoveAt(i);
        }
    }

    private ObjectPool<Projectile> GetOrCreatePool(PackedScene scene)
    {
        if (!_pools.TryGetValue(scene, out var pool))
        {
            pool = new ObjectPool<Projectile>(scene, _poolParent, prewarmCount: 8);
            _pools[scene] = pool;
        }
        return pool;
    }
}
