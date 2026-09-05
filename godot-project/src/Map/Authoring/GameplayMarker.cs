using Godot;

namespace FrontsOfWar.Map.Authoring;

public enum GameplayMarkerKind
{
    GroundEntry,
    Objective,
    SpawnPoint,
    PathJunction,
    AirEntry,
    AirObjective,
    CameraBounds,
    NoPlacementZone,
    RestrictedTerrain,
    ScriptedEvent,
}

[GlobalClass]
public partial class GameplayMarker : Resource
{
    [Export] public string Id = "";
    [Export] public GameplayMarkerKind Kind;
    [Export] public Vector2 PositionTiles;
    [Export] public Vector2 ShapeSizeTiles;
    [Export] public string PathId = "";
    [Export] public MapProperty[] Metadata = System.Array.Empty<MapProperty>();
}
