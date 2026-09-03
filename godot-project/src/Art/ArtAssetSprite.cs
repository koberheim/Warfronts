using Godot;

namespace FrontsOfWar.Art;

// Replacement-friendly visual slot. Scenes store only the stable catalog ID;
// approved art is opted into per slot, while placeholders remain the safe
// default for the primitive prototype.
[Tool]
public partial class ArtAssetSprite : Node2D
{
    [Export] public string AssetId = "";
    [Export] public bool UseApprovedAsset;
    [Export] public bool ShowPlaceholder = true;

    private Sprite2D _sprite;
    public string ResolvedPath { get; private set; } = "";

    public override void _Ready()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite");
        Refresh();
    }

    public void Refresh()
    {
        if (_sprite == null) return;
        var entry = ArtAssetCatalog.LoadFromProject().Find(AssetId);
        if (entry == null)
        {
            _sprite.Visible = false;
            ResolvedPath = "";
            if (!string.IsNullOrWhiteSpace(AssetId))
                GD.PushWarning($"Art asset ID is not in the catalog: {AssetId}");
            return;
        }

        ResolvedPath = entry.ResolvePath(UseApprovedAsset);
        if (!ShowPlaceholder && entry.Status != "APPROVED")
        {
            _sprite.Visible = false;
            return;
        }

        if (!ResourceLoader.Exists(ResolvedPath))
        {
            _sprite.Visible = false;
            GD.PushWarning($"Art asset path is missing for {AssetId}: {ResolvedPath}");
            return;
        }

        _sprite.Texture = ResourceLoader.Load<Texture2D>(ResolvedPath);
        _sprite.Visible = _sprite.Texture != null;
    }
}
