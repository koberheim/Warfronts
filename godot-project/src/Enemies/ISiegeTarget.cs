using Godot;

namespace FrontsOfWar.Enemies;

public interface ISiegeTarget
{
    Vector2 SiegePosition { get; }
    bool IsSiegeImmune { get; }
}
