using Godot;

namespace FrontsOfWar.Map.Authoring;

public enum MapZoneKind { GameplayLane, CameraBounds, NoPlacement, RestrictedTerrain }

[GlobalClass]
public partial class MapZone : Resource
{
    [Export] public string Id = "";
    [Export] public MapZoneKind Kind;
    [Export] public Vector2 CenterTiles;
    [Export] public Vector2 SizeTiles = Vector2.One;
    [Export] public bool Enabled = true;
    [Export] public string[] Tags = System.Array.Empty<string>();
}
