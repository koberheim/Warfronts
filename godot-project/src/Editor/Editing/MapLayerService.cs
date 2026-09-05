#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public enum MapEditorLayer { Terrain, Paths, TowerNodes, Architecture, Vegetation, Flavor, Gameplay, Debug }

public sealed class MapLayerState
{
    private readonly Dictionary<MapEditorLayer, bool> _visible = Enum.GetValues<MapEditorLayer>().ToDictionary(layer => layer, _ => true);
    private readonly HashSet<MapEditorLayer> _locked = new();
    public bool IsVisible(MapEditorLayer layer) => _visible[layer];
    public bool IsLocked(MapEditorLayer layer) => _locked.Contains(layer);
    public void SetVisible(MapEditorLayer layer, bool visible) => _visible[layer] = visible;
    public void SetLocked(MapEditorLayer layer, bool locked) { if (locked) _locked.Add(layer); else _locked.Remove(layer); }
    public bool CanMutate(MapObjectKind kind) => !IsLocked(ToLayer(kind));
    public bool ShouldRender(MapObjectKind kind) => IsVisible(ToLayer(kind));
    public static MapEditorLayer ToLayer(MapObjectKind kind) => kind switch
    {
        MapObjectKind.Terrain => MapEditorLayer.Terrain,
        MapObjectKind.Path or MapObjectKind.AirCorridor => MapEditorLayer.Paths,
        MapObjectKind.TowerNode => MapEditorLayer.TowerNodes,
        MapObjectKind.Marker or MapObjectKind.Zone => MapEditorLayer.Gameplay,
        MapObjectKind.Cluster or MapObjectKind.ClusterChild => MapEditorLayer.Architecture,
        MapObjectKind.Gimmick => MapEditorLayer.Debug,
        _ => MapEditorLayer.Flavor,
    };
}
#endif
