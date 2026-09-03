using Godot;

namespace FrontsOfWar.Combat;

// The 16-cell damage multiplier table from GDD §5.4.
// Rows are DamageType, columns are ArmorClass, in declaration order.
// The AntiAir row's Armored/Heavy cells are 0 by convention: those cells are
// never reached in practice because only Soft/Hardened-armored units can be
// Air targets, and non-AA damage types never resolve against Air targets at
// all (TargetingService simply never acquires them — see §5.4).
[GlobalClass]
public partial class DamageTable : Resource
{
    // The table below has no tunable [Export] fields (yet) — several of its
    // cells are explicit hard floors per the GDD (e.g. AA vs Air), so until
    // a real balancing need arises it's simplest to treat it as stateless
    // and share one instance rather than requiring a .tres asset per user.
    public static readonly DamageTable Default = new();

    private static readonly float[,] Table =
    {
        // Soft, Hardened, Armored, Heavy
        { 1.00f, 0.55f, 0.20f, 0.08f }, // SmallArms
        { 0.85f, 1.00f, 0.65f, 0.40f }, // Explosive
        { 0.45f, 0.85f, 1.25f, 1.50f }, // ArmorPiercing
        { 1.00f, 0.75f, 0.00f, 0.00f }, // AntiAir
    };

    public float Multiplier(DamageType type, ArmorClass armor)
        => Table[(int)type, (int)armor];
}
