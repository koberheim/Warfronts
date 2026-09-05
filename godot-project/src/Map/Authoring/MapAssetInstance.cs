using Godot;

namespace FrontsOfWar.Map.Authoring;

public enum MapAssetLayer { Ground, Route, Architecture, Vegetation, Flavor, Overlay }

[GlobalClass]
public partial class MapAssetInstance : Resource
{
    [Export] public string Id = "";
    [Export] public string AssetId = "";
    [Export] public string Category = "";
    [Export] public MapAssetLayer Layer;
    [Export] public Vector2 PositionTiles;
    [Export] public float RotationRadians;
    [Export] public Vector2 Scale = Vector2.One;
    [Export] public Vector2 DefaultScale = Vector2.One;
    [Export] public bool Enabled = true;
    [Export] public string[] EditorTags = System.Array.Empty<string>();
    [Export] public string[] RuntimeTags = System.Array.Empty<string>();
    [Export] public string CollisionProfile = "";
    [Export] public string NavigationProfile = "";
}
