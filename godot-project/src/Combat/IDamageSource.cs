namespace FrontsOfWar.Combat;

// Identifies the system-owned object responsible for a damage instance
// without making Combat depend on a concrete tower or ability type.
public interface IDamageSource
{
    string SourceId { get; }
}
