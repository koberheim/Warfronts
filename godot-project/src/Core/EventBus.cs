using Godot;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Core;

// Typed pub/sub so systems stay decoupled (GDD §15.1 principle 3) — a tower
// does not know what a wave is. Publish<T> does a dictionary lookup and an
// invoke only; no allocation happens on the publish path.
public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; }

    private readonly Dictionary<Type, Delegate> _handlers = new();

    public override void _EnterTree() => Instance = this;

    public override void _ExitTree()
    {
        _handlers.Clear();
        if (Instance == this) Instance = null;
    }

    public void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        _handlers[type] = _handlers.TryGetValue(type, out var existing)
            ? Delegate.Combine(existing, handler)
            : handler;
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var existing)) return;

        var combined = Delegate.Remove(existing, handler);
        if (combined == null) _handlers.Remove(type);
        else _handlers[type] = combined;
    }

    public void Publish<T>(T evt)
    {
        if (_handlers.TryGetValue(typeof(T), out var existing) && existing is Action<T> action)
            action.Invoke(evt);
    }
}
