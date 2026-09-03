using Godot;

namespace FrontsOfWar.Map;

// Authored per-map air lane. Air enemies use this straight corridor instead
// of the ground Path2D, while towers still query them in the same spatial grid.
[GlobalClass]
public partial class AirCorridorDefinition : Resource
{
    [Export] public string Id = "main_air_corridor";
    [Export] public Vector2 EntryPosition = new(0f, 0f);
    [Export] public Vector2 ObjectivePosition = new(1000f, 0f);
    [Export] public float WidthTiles = 3f;

    public float LengthPixels => EntryPosition.DistanceTo(ObjectivePosition);
}
