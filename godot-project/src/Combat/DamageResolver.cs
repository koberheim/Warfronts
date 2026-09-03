namespace FrontsOfWar.Combat;

// Pure, engine-agnostic damage math (GDD §15.3). No Node/Resource side effects,
// so it is directly unit-testable without a running scene tree.
public static class DamageResolver
{
    public const float SpottedMultiplier = 1.25f;

    public static float ResolveDamage(float baseDamage, DamageType type, ArmorClass armor,
                                       bool isSpotted, DamageTable table)
        => baseDamage * table.Multiplier(type, armor) * (isSpotted ? SpottedMultiplier : 1f);
}
