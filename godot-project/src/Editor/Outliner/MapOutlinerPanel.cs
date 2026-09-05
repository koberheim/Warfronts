#if DEBUG
using System;
using Godot;
using FrontsOfWar.Editor.Editing;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.Editor.Outliner;

public partial class MapOutlinerPanel : VBoxContainer
{
    private MapDefinition _map;
    private SelectionService _selection;

    public void Bind(MapDefinition map, SelectionService selection)
    {
        if (_selection != null) _selection.Changed -= Rebuild;
        _map = map; _selection = selection;
        if (_selection != null) _selection.Changed += Rebuild;
        Rebuild();
    }

    private void Rebuild()
    {
        foreach (Node child in GetChildren()) child.QueueFree();
        if (_map == null)
        {
            AddChild(UiFactory.Wrapped("SmallLabel", "No document loaded. New or open a map to inspect its authored objects."));
            return;
        }
        AddChild(UiFactory.Label("SubheadingLabel", "MAP HIERARCHY", uppercase: true));
        AddChild(UiFactory.Rule(false));
        AddCategory("TERRAIN", _map.Terrain, item => item.Id, MapEditorTool.Select);
        AddCategory("ENVIRONMENT", _map.Assets, item => item.Id, MapEditorTool.Select);
        AddCategory("CLUSTERS", _map.Clusters, item => item.Id, MapEditorTool.Select);
        AddCategory("ROUTES", _map.Paths, item => item.Id, MapEditorTool.Select);
        AddCategory("TOWER NODES", _map.TowerNodes, item => item.Id, MapEditorTool.Select);
        AddCategory("MARKERS", _map.Markers, item => item.Id, MapEditorTool.Select);
        AddCategory("ZONES", _map.Zones, item => item.Id, MapEditorTool.Select);
        AddCategory("GIMMICKS", _map.Gimmicks, item => item.Id, MapEditorTool.Select);
    }

    private void AddCategory<T>(string title, T[] items, Func<T, string> id, MapEditorTool tool)
    {
        if (items == null || items.Length == 0) return;
        AddChild(UiFactory.Label("CaptionLabel", title, uppercase: true));
        foreach (T item in items)
        {
            if (item == null) continue;
            string itemId = id(item);
            var button = UiFactory.Button("GhostButton", $"  {itemId}", () => _selection?.Set(itemId));
            button.Alignment = HorizontalAlignment.Left;
            button.TooltipText = "Select this authored object";
            AddChild(button);
        }
    }
}
#endif
