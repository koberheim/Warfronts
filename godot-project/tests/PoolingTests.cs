using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Tests;

public class PoolingTests : TestClass
{
    public PoolingTests(Node scene) : base(scene) { }

    [Test]
    public void FrozenPoolCannotGrowAndRejectsDoubleReturn()
    {
        var root = new Node(); TestScene.AddChild(root);
        try
        {
            var scene = GD.Load<PackedScene>("res://scenes/enemies/enemy_basic_infantry.tscn");
            var pool = new ObjectPool<EnemyController>(scene, root, 1, 3);
            pool.FreezeCapacity();
            var first = pool.Rent();
            Require(!pool.TryRent(out _) && pool.Capacity == 1, "frozen pool cannot instantiate its unused hard capacity");
            pool.Return(first);
            bool refused = false;
            try { pool.Return(first); } catch (InvalidOperationException) { refused = true; }
            Require(refused && pool.LiveCount == 0, "double return cannot corrupt lease accounting");
        }
        finally { root.Free(); }
    }

    [Test]
    public void EnemyOverflowWaitsAndReusedEnemyResetsStatusAndRejectsOldProjectile()
    {
        var root = new Node(); TestScene.AddChild(root);
        try
        {
            var path = MakePath(root);
            var definition = GD.Load<EnemyDefinition>("res://assets/data/enemies/e1_basic_infantry.tres");
            var config = new GameBalanceConfig { EnemyPoolHardCapacityPerScene = 1, PerformanceBudgetEnemies = 1 };
            var manager = new EnemyManager();
            manager.Prepare(new WaveSequence { Waves = new[] { new WaveDefinition
            {
                WaveNumber = 1, Groups = new[] { new SpawnGroup { Enemy = definition, Count = 2 } },
            } } }, root, config);
            int preparedNodes = root.GetChildCount();
            var first = manager.Spawn(definition, path, root);
            first.ApplySpotted(20);
            first.ApplySuppressed(3, 4);
            first.SetSoftBlocked(true);
            ulong oldGeneration = first.PoolGeneration;
            var projectile = new Projectile(); root.AddChild(projectile);
            projectile.Launch(first, 80, DamageType.SmallArms, 100, 0, first.GlobalPosition);
            Require(manager.Spawn(definition, path, root) == null && manager.PendingSpawnCount == 1, "full pool queues the authored spawn");
            first.ApplyDamage(10000, DamageType.Explosive);
            manager.Tick(0);
            Require(manager.PendingSpawnCount == 0 && manager.Enemies.Count == 1, "queued spawn activated after return");
            var reused = manager.Enemies[0];
            Require(ReferenceEquals(first, reused) && reused.PoolGeneration != oldGeneration, "node reused with new lifetime identity");
            Require(!reused.Status.IsSpotted && !reused.Status.IsSuppressed && reused.CurrentHp == definition.BaseHp, "new life resets health and statuses");
            var grid = new SpatialGrid(64); grid.Rebuild(manager.GetTargetables());
            projectile.ResolveImpact(grid);
            Require(reused.CurrentHp == definition.BaseHp, "old direct shot cannot damage recycled target");
            Require(root.GetChildCount() == preparedNodes + 1, "no additional enemy nodes created during combat");
            manager.Tick(0.1f);
            Require(reused.PathProgress > 0, "soft block reset on reuse");
        }
        finally { root.Free(); }
    }

    [Test]
    public void FriendlyPoolReusesNodeAndResetsDamageAndLifetime()
    {
        var root = new Node(); TestScene.AddChild(root);
        try
        {
            var path = MakePath(root);
            var arsenal = GD.Load<ArsenalDefinition>("res://assets/data/towers/arsenal_of_democracy.tres");
            var manager = new FriendlyUnitManager(root);
            manager.Prepare(new[] { arsenal.UnitScene }, new GameBalanceConfig { FriendlyPoolHardCapacityPerScene = 1 });
            var first = manager.Spawn(arsenal.Units[0], arsenal.UnitScene, path, 200);
            ulong previous = first.PoolGeneration;
            first.ApplyDamage(first.CurrentHp);
            manager.Tick(0, new EnemyManager());
            var next = manager.Spawn(arsenal.Units[0], arsenal.UnitScene, path, 200);
            Require(ReferenceEquals(first, next) && next.PoolGeneration > previous, "friendly node reused");
            Require(next.CurrentHp == arsenal.Units[0].MaxHp && !next.IsEngaged, "friendly health/engagement reset");
        }
        finally { root.Free(); }
    }

    private static PathNetwork MakePath(Node parent)
    {
        var path = new PathNetwork();
        var curve = new Curve2D(); curve.AddPoint(Vector2.Zero); curve.AddPoint(new Vector2(640, 0));
        path.AddChild(new Path2D { Name = "Route", Curve = curve });
        parent.AddChild(path);
        return path;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
