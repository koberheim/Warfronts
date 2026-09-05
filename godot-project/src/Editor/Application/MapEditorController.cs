#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FrontsOfWar.Editor.Editing;
using FrontsOfWar.Editor.Inspector;
using FrontsOfWar.Editor.Outliner;
using FrontsOfWar.Editor.Palette;
using FrontsOfWar.Editor.Viewport;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.Editor.Application;

// Standalone developer workbench. Phase 2 owns document state; Phases 3–4
// attach rendering, selection, commands, transforms, and inspection here.
public partial class MapEditorController : Control
{
    private Label _statusLabel;
    private Label _documentLabel;
    private MenuButton _fileMenu;
    private MapEditorFileWorkflow _fileWorkflow;
    private MapEditorViewport _viewport;
    private MapOutlinerPanel _outliner;
    private MapInspectorPanel _inspector;
    private MapAssetPalettePanel _palette;
    private Label _diagnosticsLabel;
    private SelectionService _selection;
    private Button _undoButton;
    private Button _redoButton;

    public override void _Ready()
    {
        GetTree().AutoAcceptQuit = false;
        _selection = new SelectionService();
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        BuildShell();
        _fileWorkflow = new MapEditorFileWorkflow();
        AddChild(_fileWorkflow);
        _fileWorkflow.Configure(_fileMenu, _documentLabel, _statusLabel);
        _viewport.BindSelection(_selection);
        _viewport.MoveSelectionRequested += MoveSelection;
        _viewport.TileClicked += PlaceSelectedAsset;
        _outliner.Bind(_fileWorkflow.Document.Current, _selection);
        _inspector.Bind(_fileWorkflow.Document, _selection);
        _palette.Bind(_fileWorkflow.Document);
        _palette.AssetSelected += BeginAssetPlacement;
        _fileWorkflow.Document.StateChanged += RefreshDocumentSurface;
        _selection.Changed += RefreshSelectionSurface;
        RefreshDocumentSurface();
        UiFactory.FadeIn(this, 0.15f);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest) _fileWorkflow?.RequestApplicationClose();
    }

    public override void _ExitTree()
    {
        if (_fileWorkflow != null) _fileWorkflow.Document.StateChanged -= RefreshDocumentSurface;
        if (_selection != null) _selection.Changed -= RefreshSelectionSurface;
        if (_viewport != null) _viewport.MoveSelectionRequested -= MoveSelection;
        if (_viewport != null) _viewport.TileClicked -= PlaceSelectedAsset;
        if (_palette != null) _palette.AssetSelected -= BeginAssetPlacement;
        GetTree().AutoAcceptQuit = true;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;
        if (key.CtrlPressed && key.Keycode == Key.Z) ApplyHistory(key.ShiftPressed);
        else if (key.CtrlPressed && key.Keycode == Key.Y) Redo();
        else if (key.CtrlPressed && key.Keycode == Key.S) _fileWorkflow?.SaveCommand();
        else if (key.Keycode == Key.Delete) ApplyCommand(MapObjectOperations.Delete(Current, SelectedIds()), "DELETED SELECTION");
        else if (key.CtrlPressed && key.Keycode == Key.D) ApplyCommand(MapObjectOperations.Duplicate(Current, SelectedIds(), Vector2.One), "DUPLICATED SELECTION");
        else if (key.CtrlPressed && key.Keycode == Key.C) CopySelection();
        else if (key.CtrlPressed && key.Keycode == Key.V) ApplyCommand(MapClipboard.Paste(Current, Vector2.One), "PASTED SELECTION");
        else if (key.Keycode == Key.Q) ApplyCommand(MapTransformCommand.Rotate(Current, SelectedIds(), -Mathf.Pi / 2f), "ROTATED LEFT");
        else if (key.Keycode == Key.E) ApplyCommand(MapTransformCommand.Rotate(Current, SelectedIds(), Mathf.Pi / 2f), "ROTATED RIGHT");
        else if (_selection.Tool == MapEditorTool.Move && key.Keycode == Key.Left) MoveSelection(Vector2.Left);
        else if (_selection.Tool == MapEditorTool.Move && key.Keycode == Key.Right) MoveSelection(Vector2.Right);
        else if (_selection.Tool == MapEditorTool.Move && key.Keycode == Key.Up) MoveSelection(Vector2.Up);
        else if (_selection.Tool == MapEditorTool.Move && key.Keycode == Key.Down) MoveSelection(Vector2.Down);
        else if (key.Keycode == Key.Escape) { _selection.Clear(); _palette?.ClearSelection(); _statusLabel.Text = "PLACEMENT CANCELLED"; }
    }

    private MapDefinition Current => _fileWorkflow?.Document.Current;

    private void BuildShell()
    {
        var background = new ColorRect { Name = "WorkbenchBackground", Color = UiPalette.WoodDark, MouseFilter = MouseFilterEnum.Ignore };
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(background);
        var chrome = UiFactory.VBox(0); chrome.Name = "Chrome"; chrome.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(chrome);
        chrome.AddChild(BuildHeader()); chrome.AddChild(BuildWorkspace()); chrome.AddChild(BuildStatusBar());
    }

    private Control BuildHeader()
    {
        var header = UiFactory.Panel("SlatePanelStrong"); header.Name = "Header"; header.CustomMinimumSize = new Vector2(0f, 62f);
        var row = UiFactory.HBox(12); row.Alignment = BoxContainer.AlignmentMode.Center; header.AddChild(row);
        row.AddChild(UiFactory.Label("HeadingLabel", "FRONTS OF WAR", uppercase: true));
        var divider = UiFactory.Label("HeadingLabel", "/"); divider.AddThemeColorOverride("font_color", UiPalette.Brass); row.AddChild(divider);
        row.AddChild(UiFactory.Label("SubheadingLabel", "Map Editor")); row.AddChild(UiFactory.Spacer(12f));
        _fileMenu = new MenuButton { Text = "File", ThemeTypeVariation = "GhostButton" }; row.AddChild(_fileMenu);
        foreach (string menu in new[] { "Edit", "View", "Generate", "Map" })
        {
            string selected = menu; var button = UiFactory.Button("GhostButton", menu, () => ShowPhaseMessage(selected));
            button.TooltipText = $"{menu} commands are staged for later map-editor phases."; row.AddChild(button);
        }
        row.AddChild(UiFactory.Button("PrimaryButton", "TEST MAP", LaunchPreview));
        row.AddChild(UiFactory.Button("GhostButton", "PUBLISH", PublishCurrent));
        row.AddChild(UiFactory.Spacer(expand: true)); _documentLabel = UiFactory.Label("MonoLabel", "No map open"); row.AddChild(_documentLabel);
        var marker = UiFactory.Label("CaptionLabel", "DEVELOPER TOOL", uppercase: true); marker.AddThemeColorOverride("font_color", UiPalette.BrassHi); row.AddChild(marker);
        return header;
    }

    private Control BuildWorkspace()
    {
        var workspace = UiFactory.VBox(8); workspace.Name = "Workspace"; workspace.SizeFlagsVertical = SizeFlags.ExpandFill;
        var inset = UiFactory.Margin(8, 8, 8, 8); inset.SizeFlagsVertical = SizeFlags.ExpandFill; workspace.AddChild(inset);
        var stack = UiFactory.VBox(8); stack.SizeFlagsVertical = SizeFlags.ExpandFill; inset.AddChild(stack);
        var primary = UiFactory.HBox(8); primary.Name = "PrimaryRow"; primary.SizeFlagsVertical = SizeFlags.ExpandFill; stack.AddChild(primary);
        primary.AddChild(BuildAssetPalette()); primary.AddChild(BuildViewportRegion()); primary.AddChild(BuildInspector()); stack.AddChild(BuildDiagnostics()); return workspace;
    }

    private Control BuildAssetPalette()
    {
        var panel = UiFactory.Panel("SlatePanel"); panel.Name = "AssetPalette"; panel.CustomMinimumSize = new Vector2(290f, 0f); panel.SizeFlagsVertical = SizeFlags.ExpandFill;
        var content = UiFactory.VBox(8); panel.AddChild(content);
        content.AddChild(UiFactory.Label("HeadingLabel", "ASSET LIBRARY", uppercase: true));
        _palette = new MapAssetPalettePanel { CustomMinimumSize = new Vector2(0f, 130f), SizeFlagsVertical = SizeFlags.ShrinkBegin };
        content.AddChild(_palette);
        content.AddChild(UiFactory.Rule(false)); content.AddChild(UiFactory.Label("CaptionLabel", "AUTHORED OBJECTS", uppercase: true));
        _outliner = new MapOutlinerPanel { SizeFlagsVertical = SizeFlags.ExpandFill }; content.AddChild(_outliner); return panel;
    }

    private Control BuildViewportRegion()
    {
        var region = UiFactory.VBox(6); region.Name = "ViewportRegion"; region.SizeFlagsHorizontal = SizeFlags.ExpandFill; region.SizeFlagsVertical = SizeFlags.ExpandFill;
        var toolbar = UiFactory.Panel("SlatePanelStrong"); toolbar.CustomMinimumSize = new Vector2(0f, 48f); var tools = UiFactory.HBox(6); tools.Alignment = BoxContainer.AlignmentMode.Center; toolbar.AddChild(tools);
        tools.AddChild(UiFactory.Label("SmallLabel", "MAP BOARD", uppercase: true)); tools.AddChild(UiFactory.Spacer(expand: true));
        foreach (var tool in new[] { MapEditorTool.Select, MapEditorTool.Move, MapEditorTool.Rotate, MapEditorTool.Scale })
        {
            var selectedTool = tool; tools.AddChild(UiFactory.Button(selectedTool == MapEditorTool.Select ? "PrimaryButton" : "GhostButton", selectedTool.ToString(), () => SetTool(selectedTool)));
        }
        _undoButton = UiFactory.Button("GhostButton", "Undo", Undo); _redoButton = UiFactory.Button("GhostButton", "Redo", Redo); tools.AddChild(_undoButton); tools.AddChild(_redoButton);
        tools.AddChild(UiFactory.Button("GhostButton", "Frame", () => _viewport?.FocusSelection())); tools.AddChild(UiFactory.Label("MonoLabel", "SNAP 1 TILE")); region.AddChild(toolbar);
        var frame = UiFactory.Panel("SlatePanelStrong"); frame.Name = "ViewportFrame"; frame.SizeFlagsHorizontal = SizeFlags.ExpandFill; frame.SizeFlagsVertical = SizeFlags.ExpandFill;
        _viewport = new MapEditorViewport { CustomMinimumSize = new Vector2(640f, 520f), SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        frame.AddChild(_viewport); region.AddChild(frame); return region;
    }

    private Control BuildInspector()
    {
        var panel = UiFactory.Panel("SlatePanel"); panel.Name = "Inspector"; panel.CustomMinimumSize = new Vector2(320f, 0f); panel.SizeFlagsVertical = SizeFlags.ExpandFill;
        _inspector = new MapInspectorPanel { SizeFlagsVertical = SizeFlags.ExpandFill }; panel.AddChild(_inspector); return panel;
    }

    private Control BuildDiagnostics()
    {
        var panel = UiFactory.Panel("SlatePanel"); panel.Name = "Diagnostics"; panel.CustomMinimumSize = new Vector2(0f, 112f); var content = UiFactory.VBox(6); panel.AddChild(content);
        var header = UiFactory.HBox(8); header.AddChild(UiFactory.Label("SubheadingLabel", "DIAGNOSTICS", uppercase: true)); header.AddChild(UiFactory.Spacer(expand: true));
        _diagnosticsLabel = UiFactory.Label("MonoLabel", "0 ERRORS   0 WARNINGS"); _diagnosticsLabel.AddThemeColorOverride("font_color", UiPalette.Green); header.AddChild(_diagnosticsLabel); content.AddChild(header); content.AddChild(UiFactory.Rule(false));
        content.AddChild(UiFactory.Label("SmallLabel", "Publish validation checks runtime markers, terrain, bounds, IDs, and catalog status.")); return panel;
    }

    private Control BuildStatusBar()
    {
        var bar = UiFactory.Panel("TeletypePanel"); bar.Name = "StatusBar"; bar.CustomMinimumSize = new Vector2(0f, 36f); var row = UiFactory.HBox(10); bar.AddChild(row);
        _statusLabel = UiFactory.Label("MonoLabel", "READY — NO MAP OPEN"); row.AddChild(_statusLabel); row.AddChild(UiFactory.Spacer(expand: true)); row.AddChild(UiFactory.Label("MonoLabel", "GRID 1 TILE   ·   ZOOM 100%   ·   DEVELOPER")); return bar;
    }

    private void SetTool(MapEditorTool tool) { _selection.Tool = tool; _viewport.SetTool(tool); _statusLabel.Text = $"TOOL: {tool.ToString().ToUpperInvariant()}"; }
    private void Undo() { if (_fileWorkflow.Document.Undo()) _statusLabel.Text = "UNDO"; RefreshHistoryButtons(); }
    private void Redo() { if (_fileWorkflow.Document.Redo()) _statusLabel.Text = "REDO"; RefreshHistoryButtons(); }
    private void ApplyHistory(bool redo) { if (redo) Redo(); else Undo(); }
    private void MoveSelection(Vector2 deltaTiles) => ApplyCommand(MapTransformCommand.Move(Current, SelectedIds(), deltaTiles), "MOVED SELECTION");

    private void CopySelection()
    {
        if (_fileWorkflow?.Document.IsOpen != true) return;
        MapClipboard.Copy(Current, SelectedIds()); _statusLabel.Text = "COPIED SELECTION";
    }

    private void ApplyCommand(IMapEditCommand command, string status)
    {
        if (_fileWorkflow?.Document.IsOpen != true) return;
        try { _fileWorkflow.Document.Apply(command); _statusLabel.Text = status; _selection.Clear(); }
        catch (Exception exception) { _statusLabel.Text = $"EDIT REJECTED: {exception.Message}"; }
        RefreshHistoryButtons();
    }

    private string[] SelectedIds() => _selection?.SelectedIds is { Count: > 0 } ids ? new List<string>(ids).ToArray() : Array.Empty<string>();

    private void RefreshDocumentSurface()
    {
        if (_fileWorkflow == null) return;
        _viewport.SetMap(Current); _outliner.Bind(Current, _selection); _palette?.Refresh(); RefreshDiagnostics(); RefreshHistoryButtons();
    }

    private void RefreshSelectionSurface()
    {
        _viewport?.QueueRedraw(); RefreshHistoryButtons();
        if (_selection?.PrimaryId is { Length: > 0 } id) _statusLabel.Text = $"SELECTED {id}";
    }

    private void RefreshHistoryButtons()
    {
        if (_undoButton == null || _fileWorkflow == null) return;
        _undoButton.Disabled = !_fileWorkflow.Document.CanUndo; _redoButton.Disabled = !_fileWorkflow.Document.CanRedo;
    }

    private void RefreshDiagnostics()
    {
        if (_diagnosticsLabel == null) return;
        if (Current == null)
        {
            _diagnosticsLabel.Text = "0 ERRORS   0 WARNINGS";
            _diagnosticsLabel.AddThemeColorOverride("font_color", UiPalette.Green);
            return;
        }
        var result = FrontsOfWar.Editor.Diagnostics.MapDiagnosticsService.Validate(Current);
        _diagnosticsLabel.Text = FrontsOfWar.Editor.Diagnostics.MapDiagnosticsService.Summary(result);
        _diagnosticsLabel.AddThemeColorOverride("font_color", result.Errors.Any() ? UiPalette.Red : UiPalette.Green);
    }

    private void ShowPhaseMessage(string menu)
    {
        _statusLabel.Text = menu == "Edit" ? "EDIT: CTRL+Z UNDO   CTRL+Y REDO   DELETE REMOVE   CTRL+D DUPLICATE" : $"{menu.ToUpperInvariant()} COMMANDS ARRIVE IN A LATER MAP-EDITOR PHASE";
    }

    private void LaunchPreview()
    {
        if (_fileWorkflow?.Document?.Current == null)
        {
            _statusLabel.Text = "TEST MAP: OPEN OR CREATE A MAP FIRST";
            return;
        }
        try
        {
            if (_fileWorkflow.Document.IsDirty) _fileWorkflow.Document.Save();
            string projectPath = ProjectSettings.GlobalizePath("res://");
            var validation = MapProductionValidator.Validate(_fileWorkflow.Document.Current);
            if (!validation.CanPublish)
                throw new InvalidOperationException(string.Join("; ", validation.Errors.Select(item => item.Message)));
            var error = MapPreviewLauncher.Launch(projectPath, _fileWorkflow.Document.FilePath);
            _statusLabel.Text = error == Error.Ok ? "TEST MAP LAUNCHED" : $"TEST MAP FAILED: {error}";
        }
        catch (Exception exception) { _statusLabel.Text = $"TEST MAP FAILED: {exception.Message}"; }
    }

    private void BeginAssetPlacement(string assetId)
    {
        _selection.Tool = MapEditorTool.Select;
        _statusLabel.Text = $"PLACE {assetId.ToUpperInvariant()} — CLICK BOARD, ESCAPE CANCELS";
    }

    private void PlaceSelectedAsset(Vector2 positionTiles)
    {
        if (_palette == null || string.IsNullOrWhiteSpace(_palette.SelectedAssetId) || _fileWorkflow?.Document?.Current == null) return;
        try
        {
            _fileWorkflow.Document.Apply(MapAssetCommands.AddAsset(_fileWorkflow.Document.Current, _palette.SelectedAssetId, "Catalog", positionTiles));
            _statusLabel.Text = $"PLACED {_palette.SelectedAssetId} AT {positionTiles}";
        }
        catch (Exception exception) { _statusLabel.Text = $"PLACEMENT FAILED: {exception.Message}"; }
    }

    private void PublishCurrent()
    {
        if (_fileWorkflow?.Document?.Current == null) { _statusLabel.Text = "PUBLISH: OPEN OR CREATE A MAP FIRST"; return; }
        try
        {
            var result = MapPublisher.Publish(_fileWorkflow.Document.Current);
            _statusLabel.Text = result.Published ? $"PUBLISHED {result.Path.GetFile()}" : $"PUBLISH BLOCKED: {result.Validation.Errors.FirstOrDefault()?.Message ?? "diagnostics"}";
            if (result.Published) _fileWorkflow.Document.SaveAs(result.Path);
        }
        catch (Exception exception) { _statusLabel.Text = $"PUBLISH FAILED: {exception.Message}"; }
    }
}
#endif
