using Godot;

namespace FrontsOfWar.Map.Authoring;

[GlobalClass]
public partial class ClusterInstance : Resource
{
    [Export] public string Id = "";
    [Export] public string CatalogAssetId = "";
    [Export] public Vector2 PositionTiles;
    [Export] public float RotationRadians;
    [Export] public float UniformScale = 1f;
    [Export] public bool Enabled = true;
    [Export] public MapAssetInstance[] Children = System.Array.Empty<MapAssetInstance>();
    [Export] public string[] Tags = System.Array.Empty<string>();
}
