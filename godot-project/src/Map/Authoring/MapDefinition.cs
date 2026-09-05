using Godot;

namespace FrontsOfWar.Map.Authoring;

// Canonical production-map resource. It contains authored state only: no
// selection, viewport, dirty flag, or transient runtime nodes.
[GlobalClass]
public partial class MapDefinition : Resource
{
    [Export] public int SchemaVersion;
    [Export] public MapMetadata Metadata;
    [Export] public TerrainInstance[] Terrain = System.Array.Empty<TerrainInstance>();
    [Export] public MapAssetInstance[] Assets = System.Array.Empty<MapAssetInstance>();
    [Export] public ClusterInstance[] Clusters = System.Array.Empty<ClusterInstance>();
    [Export] public PathDefinition[] Paths = System.Array.Empty<PathDefinition>();
    [Export] public MapAirCorridorDefinition[] AirCorridors = System.Array.Empty<MapAirCorridorDefinition>();
    [Export] public TowerPlacementNode[] TowerNodes = System.Array.Empty<TowerPlacementNode>();
    [Export] public GameplayMarker[] Markers = System.Array.Empty<GameplayMarker>();
    [Export] public MapZone[] Zones = System.Array.Empty<MapZone>();
    [Export] public MapGimmick[] Gimmicks = System.Array.Empty<MapGimmick>();
    [Export] public GenerationProvenance Provenance;

    public static MapDefinition CreateNew(string id, string displayName, int widthTiles = 1, int heightTiles = 1)
    {
        return new MapDefinition
        {
            SchemaVersion = MapSchemaMigrator.CurrentSchemaVersion,
            Metadata = new MapMetadata
            {
                Id = id,
                DisplayName = displayName,
                WidthTiles = widthTiles,
                HeightTiles = heightTiles,
                Status = MapAuthoringStatus.Draft,
            },
        };
    }
}
