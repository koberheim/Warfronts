namespace FrontsOfWar.Combat;

// The five targeting-priority modes every tower exposes as a dropdown
// (GDD §6, "Universal rules for all towers"), plus DensestCluster which is
// unique to indirect-fire archetypes (T3 Field Mortar, T7 Heavy Artillery).
public enum TargetingProfile
{
    First,
    Last,
    Strongest,
    Weakest,
    Closest,
    DensestCluster,
}
