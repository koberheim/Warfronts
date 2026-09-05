using Godot;

namespace FrontsOfWar.Map.Authoring;

public enum MapAuthoringStatus { Draft, Review, Production, Deprecated }

[GlobalClass]
public partial class MapMetadata : Resource
{
    [Export] public string Id = "";
    [Export] public string DisplayName = "";
    [Export] public string Theater = "";
    [Export] public string Biome = "";
    [Export] public int WidthTiles = 1;
    [Export] public int HeightTiles = 1;
    [Export] public int SignatureLimit = 1;
    [Export] public string TerrainSetId = "";
    [Export] public MapAuthoringStatus Status = MapAuthoringStatus.Draft;
    [Export(PropertyHint.MultilineText)] public string Notes = "";
    [Export] public string[] CampaignUsageIds = System.Array.Empty<string>();
}
