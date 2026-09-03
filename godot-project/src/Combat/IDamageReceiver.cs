namespace FrontsOfWar.Combat;

// Optional extension for damage sources that need attribution (towers,
// signatures, and future abilities) without making the targeting contract
// depend on the Enemies namespace.
public interface IDamageReceiver
{
    void ApplyDamage(float baseDamage, DamageType type, IDamageSource source);
}
