using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;

namespace FrontsOfWar.UI.Hud;

// The doctrine's fourth ability (GDD §8.3, §19 prompt 39) — key 4, a sibling
// to AbilityHotbar rather than a fourth entry inside it, so that control's
// uniform "one AbilityType, one world-point click" flow doesn't have to grow
// tower/pad/two-point targeting just for this one slot (see AbilityHotbar's
// own file-size note in the GDD prompt).
public partial class DoctrineAbilitySlot : Control
{
    private MapRuntime _mission;
    private Button _button;
    private Label _statusLabel;
    private bool _targeting;
    private Vector2? _firstPoint;
    private TowerController _firstTower;
    private string _lastStatus;

    public MapRuntime Mission { get => _mission; set => _mission = value; }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Ignore;
        CustomMinimumSize = new Vector2(150f, 92f);
        Size = CustomMinimumSize;

        var panel = new PanelContainer { CustomMinimumSize = CustomMinimumSize, Size = CustomMinimumSize, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(panel);
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        panel.AddChild(column);
        column.AddChild(new Label { Text = "DOCTRINE", HorizontalAlignment = HorizontalAlignment.Center });

        _button = new Button { CustomMinimumSize = new Vector2(140f, 48f), FocusMode = FocusModeEnum.None };
        _button.Pressed += () => ToggleTargeting();
        column.AddChild(_button);

        _statusLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        column.AddChild(_statusLabel);

        EventBus.Instance?.Subscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        EventBus.Instance?.Subscribe<TowerClickedEvent>(OnTowerClicked);
        EventBus.Instance?.Subscribe<BuildPadClickedEvent>(OnPadClicked);
        Callable.From(Refresh).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        EventBus.Instance?.Unsubscribe<TowerClickedEvent>(OnTowerClicked);
        EventBus.Instance?.Unsubscribe<BuildPadClickedEvent>(OnPadClicked);
    }

    public override void _Process(double delta) => Refresh();

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Key4 })
        {
            ToggleTargeting();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (!_targeting || _mission?.Doctrines == null) return;
        var mode = _mission.Doctrines.TargetingMode;
        // Tower / TowerThenPad resolve via TowerClickedEvent/BuildPadClickedEvent instead.
        if (mode is DoctrineTargetingMode.Tower or DoctrineTargetingMode.TowerThenPad or DoctrineTargetingMode.None) return;

        if (@event is InputEventKey { Pressed: true, Keycode: Key.Escape }) { CancelTargeting(); GetViewport().SetInputAsHandled(); return; }
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse) return;

        var worldPoint = GetViewport().GetCanvasTransform().AffineInverse() * mouse.Position;
        if (mode == DoctrineTargetingMode.TwoPoints && _firstPoint == null)
        {
            _firstPoint = worldPoint;
            _lastStatus = "Click the end point.";
            Refresh();
        }
        else
        {
            Activate(worldPoint, mode == DoctrineTargetingMode.TwoPoints ? _firstPoint : null);
        }
        GetViewport().SetInputAsHandled();
    }

    private void ToggleTargeting()
    {
        if (_mission?.Doctrines == null) return;
        var mode = _mission.Doctrines.TargetingMode;
        if (mode == DoctrineTargetingMode.None) { Activate(Vector2.Zero, null); return; }

        _targeting = !_targeting;
        _firstPoint = null;
        _firstTower = null;
        _lastStatus = _targeting ? TargetingPrompt(mode) : null;
        Refresh();
    }

    private void CancelTargeting()
    {
        _targeting = false;
        _firstPoint = null;
        _firstTower = null;
        _lastStatus = null;
        Refresh();
    }

    private void OnTowerClicked(TowerClickedEvent evt)
    {
        if (!_targeting || _mission?.Doctrines == null) return;
        var mode = _mission.Doctrines.TargetingMode;
        if (mode == DoctrineTargetingMode.Tower)
        {
            Activate(Vector2.Zero, null, evt.Tower);
        }
        else if (mode == DoctrineTargetingMode.TowerThenPad)
        {
            _firstTower = evt.Tower;
            _lastStatus = "Click an empty pad.";
            Refresh();
        }
    }

    private void OnPadClicked(BuildPadClickedEvent evt)
    {
        if (!_targeting || _firstTower == null || _mission?.Doctrines?.TargetingMode != DoctrineTargetingMode.TowerThenPad) return;
        if (evt.Pad.IsOccupied) { _lastStatus = "Pad is occupied."; Refresh(); return; }
        Activate(Vector2.Zero, null, _firstTower, evt.Pad);
    }

    private void Activate(Vector2 point, Vector2? second, TowerController tower = null, BuildPad pad = null)
    {
        bool activated = _mission.ActivateDoctrineAbility(point, second, tower, pad);
        _targeting = false;
        _firstPoint = null;
        _firstTower = null;
        _lastStatus = activated ? $"{_mission.Doctrines.Doctrine?.AbilityName} activated." : ShortfallMessage();
        Refresh();
    }

    private string ShortfallMessage()
    {
        var doctrines = _mission.Doctrines;
        if (doctrines.IsExhausted) return "Already used this mission.";
        int shortfall = doctrines.CpCost - _mission.CommandPoints.Balance;
        return shortfall > 0 ? $"Need {shortfall} more CP." : "Cooling down.";
    }

    private void OnCommandPointsChanged(CommandPointsChangedEvent evt) => Refresh();

    private void Refresh()
    {
        var doctrines = _mission?.Doctrines;
        if (doctrines?.Doctrine?.Ability == null || _button == null) { Visible = false; return; }

        Visible = true;
        float cooldown = doctrines.CooldownRemaining;
        bool affordable = _mission.CommandPoints.Balance >= doctrines.CpCost;
        string state = doctrines.IsExhausted ? "USED" : cooldown > 0f ? $"{cooldown:0.0}s" : affordable ? "READY" : "LOW CP";
        _button.Text = $"[4] {doctrines.Doctrine.AbilityName}\n{doctrines.CpCost} CP  {state}";
        _button.Disabled = cooldown > 0f || doctrines.IsExhausted;
        _button.Modulate = _targeting ? new Color(1f, 0.9f, 0.45f) : Colors.White;
        _statusLabel.Text = _lastStatus ?? "";
    }

    private static string TargetingPrompt(DoctrineTargetingMode mode) => mode switch
    {
        DoctrineTargetingMode.Point => "Click a target point.",
        DoctrineTargetingMode.Enemy => "Click near a target.",
        DoctrineTargetingMode.Tower => "Click a tower.",
        DoctrineTargetingMode.TowerThenPad => "Click a tower, then an empty pad.",
        DoctrineTargetingMode.TwoPoints => "Click the start point.",
        _ => "",
    };
}
