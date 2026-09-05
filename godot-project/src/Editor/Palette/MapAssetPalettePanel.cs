#if DEBUG
using System;
using System.Linq;
using Godot;
using FrontsOfWar.Art;
using FrontsOfWar.Editor.Documents;
using FrontsOfWar.Editor.Editing;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Palette;

public partial class MapAssetPalettePanel : VBoxContainer
{
    private MapDocument _document;
    private LineEdit _search;
    private VBoxContainer _results;
    private string _selectedAssetId = "";
    public event Action<string> AssetSelected;

    public void Bind(MapDocument document)
    {
        _document = document;
        if (_search == null) Build();
        Refresh();
    }

    private void Build()
    {
        AddChild(new Label { Text = "CATALOG PLACEMENT", TooltipText = "Choose a catalog entry, then click the board to place it." });
        _search = new LineEdit { PlaceholderText = "Search ID, name, tag...", ClearButtonEnabled = true };
        _search.TextChanged += _ => Refresh(); AddChild(_search);
        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(0f, 116f), SizeFlagsVertical = SizeFlags.ExpandFill, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        _results = new VBoxContainer(); scroll.AddChild(_results); AddChild(scroll);
    }

    public void Refresh()
    {
        if (_results == null) return;
        foreach (Node child in _results.GetChildren()) child.QueueFree();
        var entries = ArtPaletteService.Query(ArtAssetCatalog.LoadFromProject(), new ArtPaletteQuery { Search = _search?.Text ?? "" });
        foreach (var entry in entries.Take(24))
        {
            var item = new Button { Text = $"{entry.Name}  ·  {entry.Status}", Alignment = HorizontalAlignment.Left, TooltipText = entry.Id };
            var selected = entry;
            item.Pressed += () => { _selectedAssetId = selected.Id; AssetSelected?.Invoke(selected.Id); };
            _results.AddChild(item);
        }
        if (entries.Count == 0) _results.AddChild(new Label { Text = "No catalog entries match." });
        else if (entries.Count > 24) _results.AddChild(new Label { Text = $"{entries.Count - 24} more — refine search" });
    }

    public string SelectedAssetId => _selectedAssetId;
    public void ClearSelection() => _selectedAssetId = "";

    public void PlaceAt(Vector2 positionTiles, string category = "Flavor")
    {
        if (_document?.Current == null || string.IsNullOrWhiteSpace(_selectedAssetId)) return;
        _document.Apply(MapAssetCommands.AddAsset(_document.Current, _selectedAssetId, category, positionTiles));
    }
}
#endif
