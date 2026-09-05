using Godot;

namespace FrontsOfWar.Map.Authoring;

[GlobalClass]
public partial class TowerPlacementNode : Resource
{
    [Export] public string Id = "";
    [Export] public Vector2 PositionTiles;
    [Export] public float RotationRadians;
    [Export] public PadTag Tag;

    // Ruined Town's clipped-range-arc gimmick (GDD §11.1 M4) - the only
    // range-shape variation in the game, authored per pad. 180 (no
    // clipping) is the default for every pad on every other map.
    [Export] public float ArcFacingDegrees;
    [Export] public float ArcHalfAngleDegrees = 180f;
    [Export] public string[] AllowedArchetypeIds = System.Array.Empty<string>();
    [Export] public bool Enabled = true;
    [Export] public string FootprintProfile = "single_tile";
    [Export] public bool GeneratedSuggestion;
    [Export] public MapProperty[] RuntimeMetadata = System.Array.Empty<MapProperty>();
}
