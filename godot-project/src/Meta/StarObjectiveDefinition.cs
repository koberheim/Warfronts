using Godot;

namespace FrontsOfWar.Meta;

// The mission-specific third star (GDD §11.3 — stars 1 and 2 are fixed:
// completion, and >=75% Defense Line Integrity). One int parameter covers
// every objective kind authored so far; add a kind before reaching for a
// second parameter rather than overloading this one.
public enum StarObjectiveKind
{
    MaxTowersBuilt,  // IntParameter = the max tower count allowed
    NoArchetype,     // IntParameter = (int)TowerArchetype to avoid
    MinDifficulty,   // IntParameter = (int)Difficulty required or higher
    BossKilled,      // IntParameter unused
}

[GlobalClass]
public partial class StarObjectiveDefinition : Resource
{
    [Export] public StarObjectiveKind Kind;
    [Export] public int IntParameter;
    [Export] public string Description = "";
}
