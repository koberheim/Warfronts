#if DEBUG
using System;
using Godot;
using FrontsOfWar.Editor.Documents;
using FrontsOfWar.Editor.Editing;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.Editor.Inspector;

public partial class MapInspectorPanel : PanelContainer
{
    private VBoxContainer _content;
    private Label _selectionLabel;
    private Label _categoryLabel;
    private SpinBox _x;
    private SpinBox _y;
    private SpinBox _rotation;
    private SpinBox _scale;
    private MapDocument _document;
    private SelectionService _selection;
    private bool _refreshing;

    public void Bind(MapDocument document, SelectionService selection)
    {
        if (_selection != null) _selection.Changed -= Refresh;
        _document = document; _selection = selection;
        _document.StateChanged += Refresh;
        _selection.Changed += Refresh;
        BuildUi();
        Refresh();
    }

    public override void _ExitTree()
    {
        if (_document != null) _document.StateChanged -= Refresh;
        if (_selection != null) _selection.Changed -= Refresh;
    }

    private void BuildUi()
    {
        _content = UiFactory.VBox(8);
        AddChild(_content);
        _content.AddChild(UiFactory.Label("HeadingLabel", "INSPECTOR", uppercase: true));
        _selectionLabel = UiFactory.Label("BodyLabel", "No selection");
        _content.AddChild(_selectionLabel);
        _categoryLabel = UiFactory.Label("CaptionLabel", "Select an authored object");
        _categoryLabel.AddThemeColorOverride("font_color", UiPalette.CreamMuted);
        _content.AddChild(_categoryLabel);
        _content.AddChild(UiFactory.Rule(false));
        _content.AddChild(UiFactory.Label("SubheadingLabel", "TRANSFORM", uppercase: true));
        _x = AddNumber("X position", -9999f, 9999f, 0.25f);
        _y = AddNumber("Y position", -9999f, 9999f, 0.25f);
        _rotation = AddNumber("Rotation degrees", -360f, 360f, 15f);
        _scale = AddNumber("Uniform scale", 0.1f, 8f, 0.05f);
        _content.AddChild(UiFactory.Rule(false));
        _content.AddChild(UiFactory.Label("SubheadingLabel", "DOCUMENT", uppercase: true));
        _content.AddChild(UiFactory.Label("MonoLabel", "Select an object to inspect its authored values."));
        _x.ValueChanged += _ => ApplyTransform();
        _y.ValueChanged += _ => ApplyTransform();
        _rotation.ValueChanged += _ => ApplyTransform();
        _scale.ValueChanged += _ => ApplyTransform();
    }

    private SpinBox AddNumber(string label, double minimum, double maximum, double step)
    {
        var row = UiFactory.HBox(6);
        row.AddChild(UiFactory.Label("SmallLabel", label));
        var input = new SpinBox { MinValue = minimum, MaxValue = maximum, Step = step, AllowLesser = true, AllowGreater = true, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        row.AddChild(input); _content.AddChild(row); return input;
    }

    private void Refresh()
    {
        if (_content == null) return;
        _refreshing = true;
        var handle = _document?.IsOpen == true ? MapObjectLocator.Find(_document.Current, _selection?.PrimaryId) : null;
        MapObjectTransform transform = default;
        bool hasSelection = handle != null && MapObjectLocator.TryGetTransform(handle, out transform);
        _selectionLabel.Text = hasSelection ? handle.Id : "No selection";
        _categoryLabel.Text = hasSelection ? handle.Kind.ToString() : "Select an authored object";
        _x.Editable = hasSelection; _y.Editable = hasSelection; _rotation.Editable = hasSelection && transform.CanRotate; _scale.Editable = hasSelection && transform.CanScale;
        _x.Value = hasSelection ? transform.PositionTiles.X : 0f;
        _y.Value = hasSelection ? transform.PositionTiles.Y : 0f;
        _rotation.Value = hasSelection ? Mathf.RadToDeg(transform.RotationRadians) : 0f;
        _scale.Value = hasSelection ? Mathf.Max(0.1f, transform.Scale.X) : 1f;
        _refreshing = false;
    }

    private void ApplyTransform()
    {
        if (_refreshing || _document?.IsOpen != true || string.IsNullOrEmpty(_selection?.PrimaryId)) return;
        var handle = MapObjectLocator.Find(_document.Current, _selection.PrimaryId);
        if (handle == null || !MapObjectLocator.TryGetTransform(handle, out var current)) return;
        try
        {
            var next = new MapObjectTransform(new Vector2((float)_x.Value, (float)_y.Value), Mathf.DegToRad((float)_rotation.Value), Vector2.One * (float)_scale.Value, current.CanRotate, current.CanScale);
            _document.Apply(MapTransformCommand.Set(_document.Current, handle.Id, next));
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Inspector change rejected: {exception.Message}");
            Refresh();
        }
    }
}
#endif
