using Godot;
using System.Collections.Generic;

namespace FrontsOfWar.Core;

// Pools PackedScene instances so nothing calls PackedScene.Instantiate()
// mid-wave (GDD §15.1 principle 5) — every projectile, effect, damage number,
// and enemy is rented/returned instead of created/freed.
public class ObjectPool<T> where T : Node
{
    private readonly PackedScene _scene;
    private readonly Node _poolParent;
    private readonly Stack<T> _available = new();

    public int LiveCount { get; private set; }

    public ObjectPool(PackedScene scene, Node poolParent, int prewarmCount = 0)
    {
        _scene = scene;
        _poolParent = poolParent;
        for (int i = 0; i < prewarmCount; i++)
            _available.Push(CreateInstance());
    }

    private T CreateInstance()
    {
        var instance = _scene.Instantiate<T>();
        _poolParent.AddChild(instance);
        Deactivate(instance);
        return instance;
    }

    public T Rent()
    {
        var instance = _available.Count > 0 ? _available.Pop() : CreateInstance();
        Activate(instance);
        LiveCount++;
        return instance;
    }

    public void Return(T instance)
    {
        Deactivate(instance);
        _available.Push(instance);
        LiveCount--;
    }

    private static void Activate(T instance)
    {
        instance.ProcessMode = Node.ProcessModeEnum.Inherit;
        if (instance is CanvasItem ci) ci.Visible = true;
    }

    private static void Deactivate(T instance)
    {
        instance.ProcessMode = Node.ProcessModeEnum.Disabled;
        if (instance is CanvasItem ci) ci.Visible = false;
    }
}
