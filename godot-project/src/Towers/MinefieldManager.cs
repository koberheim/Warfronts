using FrontsOfWar.Combat;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

public class MinefieldManager
{
    private readonly List<MinefieldController> _fields = new();
    private Func<IReadOnlyList<ITargetable>> _provider;
    public IReadOnlyList<MinefieldController> Fields => _fields;

    public void Register(MinefieldController field)
    {
        if (field == null || _fields.Contains(field)) return;
        _fields.Add(field);
        if (_provider != null) field.Initialize(_provider);
    }

    public void Initialize(Func<IReadOnlyList<ITargetable>> provider)
    {
        _provider = provider;
        foreach (var field in _fields) field.Initialize(provider);
    }

    public void Tick(float delta)
    {
        foreach (var field in _fields) field.SimTick(delta);
    }
}
