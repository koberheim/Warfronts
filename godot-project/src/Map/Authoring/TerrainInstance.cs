using Godot;

namespace FrontsOfWar.Map.Authoring;

public enum TerrainCollisionClass { None, Decorative, BlocksPlacement, BlocksMovement }

[GlobalClass]
public partial class TerrainInstance : Resource
{
    [Export] public string Id = "";
    [Export] public string AssetId = "";
    [Export] public Vector2I Cell;
    [Export(PropertyHint.Range, "0,3,1")] public int RotationQuarterTurns;
    [Export] public string TerrainSetId = "";
    [Export] public string AdjacencyVariant = "";
    [Export] public string SocketMetadata = "";
    [Export] public TerrainCollisionClass CollisionClass;
    [Export] public string[] EditorTags = System.Array.Empty<string>();
}
