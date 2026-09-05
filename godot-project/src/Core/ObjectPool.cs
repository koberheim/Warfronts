using Godot;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Core;

// Optional lifecycle contract for nodes whose mutable gameplay state must be
// cleared between leases. The pool owns visibility/process state; the node
// owns domain state such as HP, targets, timers, and status effects.
public interface IPoolLifecycle
{
    void OnRentedFromPool();
    void OnReturnedToPool();
}

// Pools PackedScene instances so nothing calls PackedScene.Instantiate()
// mid-wave (GDD §15.1 principle 5) — every projectile, effect, damage number,
// and enemy is rented/returned instead of created/freed.
public class ObjectPool<T> where T : Node
{
    private readonly PackedScene _scene;
    private readonly Node _poolParent;
    private readonly Stack<T> _available = new();
    private readonly HashSet<T> _instances = new();
    private readonly HashSet<T> _rented = new();
    private readonly int _hardCapacity;
    private bool _capacityFrozen;

    public int LiveCount { get; private set; }
    public int AvailableCount => _available.Count;
    public int Capacity => _instances.Count;
    public int HardCapacity => _hardCapacity;
    public bool CanRent => _available.Count > 0 || !_capacityFrozen && (_hardCapacity <= 0 || Capacity < _hardCapacity);
    public void FreezeCapacity() => _capacityFrozen = true;

    // hardCapacity <= 0 preserves the original grow-on-demand behavior. Pools
    // used by mission transients pass an explicit cap and prewarm before the
    // first wave; other existing consumers remain API-compatible.
    public ObjectPool(PackedScene scene, Node poolParent, int prewarmCount = 0, int hardCapacity = 0)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _poolParent = poolParent ?? throw new ArgumentNullException(nameof(poolParent));
        _hardCapacity = Mathf.Max(0, hardCapacity);
        Prepare(prewarmCount);
    }

    private T CreateInstance()
    {
        if (_hardCapacity > 0 && Capacity >= _hardCapacity)
            throw new InvalidOperationException($"Pool for '{_scene.ResourcePath}' reached its hard capacity of {_hardCapacity}.");
        var instance = _scene.Instantiate<T>();
        _poolParent.AddChild(instance);
        Deactivate(instance);
        _instances.Add(instance);
        return instance;
    }

    // Grows only while the mission is being prepared. Calling Prepare again is
    // idempotent and never shrinks or replaces live instances.
    public void Prepare(int desiredCapacity)
    {
        if (_capacityFrozen && desiredCapacity > Capacity)
            throw new InvalidOperationException("A frozen pool cannot grow during combat.");
        int target = Mathf.Max(0, desiredCapacity);
        if (_hardCapacity > 0) target = Mathf.Min(target, _hardCapacity);
        while (Capacity < target)
            _available.Push(CreateInstance());
    }

    public T Rent()
    {
        if (!TryRent(out var instance))
            throw new InvalidOperationException($"Pool for '{_scene.ResourcePath}' is exhausted at capacity {Capacity}.");
        return instance;
    }

    // Fixed-capacity gameplay pools use this form so overflow can be queued
    // deterministically instead of dropping a spawn or allocating mid-wave.
    public bool TryRent(out T instance)
    {
        if (_available.Count > 0)
            instance = _available.Pop();
        else if (!_capacityFrozen && (_hardCapacity <= 0 || Capacity < _hardCapacity))
            instance = CreateInstance();
        else
        {
            instance = null;
            return false;
        }

        Activate(instance);
        _rented.Add(instance);
        LiveCount++;
        return true;
    }

    public void Return(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (!_instances.Contains(instance))
            throw new InvalidOperationException("Cannot return an instance owned by another pool.");
        if (!_rented.Remove(instance))
            throw new InvalidOperationException("Cannot return an instance that is not currently rented.");
        Deactivate(instance);
        _available.Push(instance);
        LiveCount--;
    }

    private static void Activate(T instance)
    {
        instance.ProcessMode = Node.ProcessModeEnum.Inherit;
        if (instance is CanvasItem ci) ci.Visible = true;
        if (instance is IPoolLifecycle lifecycle) lifecycle.OnRentedFromPool();
    }

    private static void Deactivate(T instance)
    {
        if (instance is IPoolLifecycle lifecycle) lifecycle.OnReturnedToPool();
        instance.ProcessMode = Node.ProcessModeEnum.Disabled;
        if (instance is CanvasItem ci) ci.Visible = false;
    }
}
