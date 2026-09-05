#if DEBUG
using System;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Editor.Editing;
using FrontsOfWar.Editor.Rendering;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.Editor.Viewport;

public partial class MapEditorViewport : Control
{
    private const int MajorGridInterval = 4;
    private MapRenderSnapshot _snapshot;
    private SelectionService _selection;
    public MapLayerState Layers { get; } = new();
    private Vector2 _cameraOffset;
    private float _zoom = 1f;
    private bool _panning;
    private bool _dragging;
    private Vector2 _dragStartTile;
    private Vector2 _lastMouse;
    private bool _hasCamera;
    private float TilePixelSize => GameBalanceConfigAutoload.Config?.TilePixelSize ?? 64f;
    public float Zoom => _zoom;
    public event Action<Vector2> CursorTileChanged;
    public event Action<Vector2> MoveSelectionRequested;
    public event Action<Vector2> TileClicked;

    public override void _Ready()
    {
        Name = "Viewport";
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Stop;
        Resized += OnResized;
        QueueRedraw();
    }

    public void BindSelection(SelectionService selection)
    {
        if (_selection != null) _selection.Changed -= QueueRedraw;
        _selection = selection;
        if (_selection != null) _selection.Changed += QueueRedraw;
        QueueRedraw();
    }

    public void SetMap(MapDefinition map)
    {
        _snapshot = map == null ? null : MapSceneFactory.Build(map);
        _hasCamera = false;
        QueueRedraw();
    }

    public void SetTool(MapEditorTool tool)
    {
        if (_selection != null) _selection.Tool = tool;
        QueueRedraw();
    }

    public void CenterMap()
    {
        if (_snapshot == null) return;
        _zoom = 1f;
        _hasCamera = true;
        _cameraOffset = (Size - MapSizePixels()) * 0.5f;
        QueueRedraw();
    }

    public void FocusSelection()
    {
        if (_snapshot == null || _selection == null) return;
        foreach (var item in _snapshot.Items)
            if (_selection.Contains(item.Id))
            {
                _cameraOffset = Size * 0.5f - TileToScreen(item.PositionTiles);
                QueueRedraw();
                return;
            }
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), UiPalette.Slate);
        EnsureCamera();
        DrawGrid();
        if (_snapshot == null)
        {
            DrawEmptyState();
            return;
        }
        DrawRect(new Rect2(_cameraOffset, MapSizePixels()), new Color("#c7b68d").WithAlpha(0.08f));
        foreach (var path in _snapshot.Paths) DrawPath(path);
        foreach (var item in _snapshot.Items)
            if (Layers.ShouldRender(ToObjectKind(item.Kind))) DrawItem(item);
        DrawMapFrame();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            CursorTileChanged?.Invoke(ScreenToTile(motion.Position));
            if (_panning)
            {
                _cameraOffset += motion.Position - _lastMouse;
                _lastMouse = motion.Position;
                QueueRedraw();
            }
            return;
        }
        if (@event is not InputEventMouseButton button) return;
        if (button.ButtonIndex == MouseButton.Middle)
        {
            _panning = button.Pressed; _lastMouse = button.Position; AcceptEvent(); return;
        }
        if ((button.ButtonIndex == MouseButton.WheelUp || button.ButtonIndex == MouseButton.WheelDown) && button.Pressed)
        {
            SetZoomAround(button.Position, button.ButtonIndex == MouseButton.WheelUp ? 1.12f : 0.89f); AcceptEvent(); return;
        }
        if (button.ButtonIndex == MouseButton.Left && !button.Pressed && _dragging)
        {
            _dragging = false;
            Vector2 delta = MapCoordinateSystem.SnapToTile(ScreenToTile(button.Position) - _dragStartTile);
            if (delta != Vector2.Zero) MoveSelectionRequested?.Invoke(delta);
            AcceptEvent();
            return;
        }
        if (button.ButtonIndex == MouseButton.Left && button.Pressed)
        {
            MapRenderItem hit = HitTest(button.Position);
            if (hit == null) TileClicked?.Invoke(MapCoordinateSystem.SnapToTile(ScreenToTile(button.Position)));
            bool additive = Input.IsKeyPressed(Key.Shift) || Input.IsKeyPressed(Key.Ctrl);
            if (hit == null) _selection?.Clear();
            else if (additive) _selection?.Toggle(hit.Id);
            else _selection?.Set(hit.Id);
            _dragging = hit != null && _selection?.Tool == MapEditorTool.Move;
            _dragStartTile = ScreenToTile(button.Position);
            AcceptEvent();
        }
    }

    public Vector2 ScreenToTile(Vector2 screenPosition)
        => MapCoordinateSystem.PixelToTile(screenPosition - _cameraOffset, TilePixelSize * _zoom);

    public Vector2 TileToScreen(Vector2 tilePosition)
        => _cameraOffset + MapCoordinateSystem.TileToPixel(tilePosition, TilePixelSize) * _zoom;

    public MapRenderItem HitTest(Vector2 screenPosition)
    {
        if (_snapshot == null) return null;
        for (int i = _snapshot.Items.Length - 1; i >= 0; i--)
        {
            var item = _snapshot.Items[i];
            float radius = Mathf.Max(item.SizeTiles.X, item.SizeTiles.Y) * TilePixelSize * _zoom * 0.65f;
            if (TileToScreen(item.PositionTiles).DistanceTo(screenPosition) <= Mathf.Max(radius, 10f)) return item;
        }
        return null;
    }

    private void DrawGrid()
    {
        if (_snapshot == null)
        {
            for (float x = 0f; x <= Size.X; x += 32f) DrawLine(new Vector2(x, 0f), new Vector2(x, Size.Y), UiPalette.SlateLine.WithAlpha(0.18f));
            for (float y = 0f; y <= Size.Y; y += 32f) DrawLine(new Vector2(0f, y), new Vector2(Size.X, y), UiPalette.SlateLine.WithAlpha(0.18f));
            return;
        }
        for (int x = 0; x <= _snapshot.WidthTiles; x++)
        {
            bool major = x % MajorGridInterval == 0;
            DrawLine(TileToScreen(new Vector2(x, 0f)), TileToScreen(new Vector2(x, _snapshot.HeightTiles)), UiPalette.SlateLine.WithAlpha(major ? 0.48f : 0.22f), major ? 1.4f : 1f);
        }
        for (int y = 0; y <= _snapshot.HeightTiles; y++)
        {
            bool major = y % MajorGridInterval == 0;
            DrawLine(TileToScreen(new Vector2(0f, y)), TileToScreen(new Vector2(_snapshot.WidthTiles, y)), UiPalette.SlateLine.WithAlpha(major ? 0.48f : 0.22f), major ? 1.4f : 1f);
        }
    }

    private void DrawPath(MapRenderPath path)
    {
        if (path.PointsTiles.Length < 2) return;
        var points = new Vector2[path.PointsTiles.Length];
        for (int i = 0; i < points.Length; i++) points[i] = TileToScreen(path.PointsTiles[i]);
        DrawPolyline(points, path.Color.WithAlpha(0.82f), Mathf.Max(2f, path.WidthTiles * TilePixelSize * _zoom), true);
        DrawPolyline(points, UiPalette.Cream.WithAlpha(0.24f), 1f, true);
    }

    private void DrawItem(MapRenderItem item)
    {
        Vector2 center = TileToScreen(item.PositionTiles);
        Vector2 size = item.SizeTiles * TilePixelSize * _zoom;
        bool selected = _selection?.Contains(item.Id) == true;
        if (item.Kind == MapRenderKind.Marker)
        {
            var diamond = new[] { center + new Vector2(0f, -size.Y * 0.5f), center + new Vector2(size.X * 0.5f, 0f), center + new Vector2(0f, size.Y * 0.5f), center - new Vector2(size.X * 0.5f, 0f), center + new Vector2(0f, -size.Y * 0.5f) };
            DrawPolyline(diamond, selected ? UiPalette.BrassHi : item.Color, 2f, true); return;
        }
        if (item.Kind == MapRenderKind.Zone)
        {
            DrawRect(new Rect2(center - size * 0.5f, size), item.Color, true);
            DrawRect(new Rect2(center - size * 0.5f, size), selected ? UiPalette.BrassHi : item.Color, false, selected ? 3f : 1f); return;
        }
        if (item.Kind == MapRenderKind.TowerNode)
        {
            DrawCircle(center, Mathf.Max(7f, size.X * 0.5f), item.Color);
            DrawLine(center, center + Vector2.Right.Rotated(item.RotationRadians) * size.X * 0.7f, UiPalette.WoodDark, 2f);
        }
        else DrawStyleBox(StyleBoxFlatFor(item.Color), new Rect2(center - size * 0.5f, size));
        if (selected) DrawRect(new Rect2(center - size * 0.5f - Vector2.One * 4f, size + Vector2.One * 8f), UiPalette.BrassHi, false, 2.5f);
    }

    private void DrawMapFrame()
    {
        var frame = new Rect2(_cameraOffset, MapSizePixels());
        DrawRect(frame, UiPalette.Brass.WithAlpha(0.72f), false, 2f);
        DrawString(ThemeDB.FallbackFont, frame.Position + new Vector2(8f, -8f), $"{_snapshot.WidthTiles} × {_snapshot.HeightTiles} TILES", HorizontalAlignment.Left, -1f, 13, UiPalette.CreamMuted);
    }

    private void DrawEmptyState()
    {
        if (Size.X < 220f || Size.Y < 140f) return;
        Vector2 center = Size * 0.5f;
        DrawString(ThemeDB.FallbackFont, center - new Vector2(75f, -6f), "OPEN A MAP TO INSPECT", HorizontalAlignment.Left, -1f, 14, UiPalette.CreamMuted);
        DrawString(ThemeDB.FallbackFont, center - new Vector2(68f, -28f), "FILE  ›  NEW MAP", HorizontalAlignment.Left, -1f, 12, UiPalette.BrassHi);
    }

    private void SetZoomAround(Vector2 screenPosition, float factor)
    {
        Vector2 before = ScreenToTile(screenPosition);
        _zoom = Mathf.Clamp(_zoom * factor, 0.35f, 3f);
        _cameraOffset = screenPosition - MapCoordinateSystem.TileToPixel(before, TilePixelSize) * _zoom;
        QueueRedraw();
    }

    private void EnsureCamera()
    {
        if (_hasCamera || _snapshot == null) return;
        _cameraOffset = (Size - MapSizePixels()) * 0.5f; _hasCamera = true;
    }

    private Vector2 MapSizePixels() => new(_snapshot.WidthTiles * TilePixelSize * _zoom, _snapshot.HeightTiles * TilePixelSize * _zoom);
    private void OnResized() { if (!_hasCamera) QueueRedraw(); }
    private static StyleBoxFlat StyleBoxFlatFor(Color color)
        => new() { BgColor = color.WithAlpha(Mathf.Max(0.32f, color.A)), CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomLeft = 2, CornerRadiusBottomRight = 2 };

    private static MapObjectKind ToObjectKind(MapRenderKind kind) => kind switch
    {
        MapRenderKind.Terrain => MapObjectKind.Terrain,
        MapRenderKind.Asset => MapObjectKind.Asset,
        MapRenderKind.Cluster => MapObjectKind.Cluster,
        MapRenderKind.TowerNode => MapObjectKind.TowerNode,
        MapRenderKind.Marker => MapObjectKind.Marker,
        MapRenderKind.Zone => MapObjectKind.Zone,
        _ => MapObjectKind.Asset,
    };
}
#endif
