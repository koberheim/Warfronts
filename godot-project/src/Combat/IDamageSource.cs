using Godot;

namespace FrontsOfWar.Combat;

// Identifies the system-owned object responsible for a damage instance
// without making Combat depend on a concrete tower or ability type. Every
// implementer is already a Node2D, so GlobalPosition costs them nothing -
// added for Elite Medium Armor's Frontal Plate (GDD §10.3), which needs to
// know where an incoming hit came from relative to the enemy's heading.
public interface IDamageSource
{
    string SourceId { get; }
    Vector2 GlobalPosition { get; }
}
