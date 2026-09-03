using FrontsOfWar.Combat;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

public class MinefieldManager
{
    private readonly List<MinefieldController> _fields = new();
    public IReadOnlyList<MinefieldController> Fields => _fields;

    public void Register(MinefieldController field)
    {
        if (field != null && !_fields.Contains(field)) _fields.Add(field);
    }

    public void Initialize(Func<IReadOnlyList<ITargetable>> provider)
    {
        foreach (var field in _fields) field.Initialize(provider);
    }

    public void Tick(float delta)
    {
        foreach (var field in _fields) field.SimTick(delta);
    }
}
