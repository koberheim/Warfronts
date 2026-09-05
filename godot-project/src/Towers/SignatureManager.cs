using FrontsOfWar.Combat;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Owns signature lifetimes and provides the shared per-mission dependencies.
// A normal loadout registers exactly one signature per map.
public class SignatureManager
{
    private readonly List<SignatureControllerBase> _signatures = new();
    private PathNetwork _path;
    private Func<IReadOnlyList<ITargetable>> _targetsProvider;
    private Func<IReadOnlyList<TowerController>> _towersProvider;
    public int Limit { get; set; } = 1;

    public IReadOnlyList<SignatureControllerBase> Signatures => _signatures;
    public bool CanRegister => _signatures.Count < Limit;

    public bool Register(SignatureControllerBase signature, PathNetwork path = null) {
        if (signature == null || !CanRegister || _signatures.Contains(signature)) return false;
        _signatures.Add(signature);
        if (_targetsProvider != null) InitializeOne(signature, path ?? _path);
        return true;
    }

    public void Initialize(PathNetwork path, Func<IReadOnlyList<ITargetable>> targetsProvider,
                           Func<IReadOnlyList<TowerController>> towersProvider)
    {
        _path = path; _targetsProvider = targetsProvider; _towersProvider = towersProvider;
        foreach (var signature in _signatures) InitializeOne(signature, path);
    }

    private void InitializeOne(SignatureControllerBase signature, PathNetwork path)
    {
        if (signature is BlitzkriegCommandController blitz) blitz.Initialize(path, _targetsProvider, _towersProvider);
        else signature.Initialize(path, _targetsProvider);
    }

    public void Tick(float delta)
    {
        foreach (var signature in _signatures)
        {
            switch (signature)
            {
                case RafScrambleController raf: raf.SimTick(delta); break;
                case KatyushaStormController katyusha: katyusha.SimTick(delta); break;
                case BlitzkriegCommandController blitz: blitz.SimTick(delta); break;
                case BersaglieriChargeController bersaglieri: bersaglieri.SimTick(delta); break;
                case SpecialAttackAirfieldController airfield: airfield.SimTick(delta); break;
            }
        }
    }
}
