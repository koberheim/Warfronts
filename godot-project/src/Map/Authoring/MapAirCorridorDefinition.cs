using Godot;

namespace FrontsOfWar.Map.Authoring;

// Canonical authored air geometry is tile-space. Phase 3 converts it to the
// existing runtime AirCorridorDefinition, whose current fields are pixels.
[GlobalClass]
public partial class MapAirCorridorDefinition : Resource
{
    [Export] public string Id = "";
    [Export] public string EntryMarkerId = "";
    [Export] public string ObjectiveMarkerId = "";
    [Export] public Vector2 EntryPositionTiles;
    [Export] public Vector2 ObjectivePositionTiles;
    [Export] public float WidthTiles = 3f;
    [Export] public string[] Tags = System.Array.Empty<string>();
}
