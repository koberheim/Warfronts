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

    public IReadOnlyList<SignatureControllerBase> Signatures => _signatures;
    public bool CanRegister => _signatures.Count == 0;

    public bool Register(SignatureControllerBase signature) {
        if (signature == null || _signatures.Count > 0 || _signatures.Contains(signature)) return false;
        _signatures.Add(signature);
        return true;
    }

    public void Initialize(PathNetwork path, Func<IReadOnlyList<ITargetable>> targetsProvider,
                           Func<IReadOnlyList<TowerController>> towersProvider)
    {
        foreach (var signature in _signatures)
        {
            switch (signature)
            {
                case BlitzkriegCommandController blitz:
                    blitz.Initialize(path, targetsProvider, towersProvider);
                    break;
                default:
                    signature.Initialize(path, targetsProvider);
                    break;
            }
        }
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
