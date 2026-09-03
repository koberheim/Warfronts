namespace FrontsOfWar.Enemies;

// The 12 launch enemy archetypes (GDD §10.2). Only BasicInfantry and
// MediumArmor have behavior implemented as of M1 — the rest are named here
// so EnemyDefinition assets and future EnemyController special-ability code
// have a stable enum to target without a later breaking rename.
public enum EnemyArchetype
{
    BasicInfantry,   // E1
    FastInfantry,    // E2
    SwarmInfantry,   // E3
    ArmoredInfantry, // E4
    LightVehicle,    // E5
    MediumArmor,     // E6
    HeavyArmor,      // E7
    AirUnit,         // E8
    Support,         // E9
    Escort,          // E10
    Recon,           // E11
    Siege,           // E12
}
