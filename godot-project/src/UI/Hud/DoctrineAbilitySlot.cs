using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Hud;

// The doctrine's fourth ability (GDD §8.3, §19 prompt 39) - key 4. A logic-
// only sibling of AbilityHotbar that places its card into the hotbar's row
// and speaks through the shared status line, so the four cards read as one
// bar while this control keeps its own tower / pad / two-point targeting.
public partial class DoctrineAbilitySlot : Control
{
    private MapRuntime _mission;
    private AbilityHotbar _hotbar;
    private AbilityCard _card;
    private bool _targeting;
    private Vector2? _firstPoint;
    private TowerController _firstTower;
    private string _lastStatus;

    public MapRuntime Mission { get => _mission; set => _mission = value; }
    public AbilityHotbar Hotbar { get => _hotbar; set => _hotbar = value; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        EventBus.Instance?.Subscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        EventBus.Instance?.Subscribe<TowerClickedEvent>(OnTowerClicked);
        EventBus.Instance?.Subscribe<BuildPadClickedEvent>(OnPadClicked);
        // Deferred: MapRuntime builds DoctrineSystem in its own _Ready, which
        // runs after this child's (see HudController's note on ordering).
        Callable.From(CreateCard).CallDeferred();
    }

    private void CreateCard()
    {
        var doctrine = _mission?.Doctrines?.Doctrine;
        if (doctrine?.Ability == null || _hotbar?.CardRow == null) return;
        _card = new AbilityCard();
        _hotbar.CardRow.AddChild(_card);
        string iconId = UiIcons.Get("ability_" + doctrine.Id) != null ? "ability_" + doctrine.Id : "ability_doctrine";
        _card.Setup("4", iconId, doctrine.AbilityName, _mission.Doctrines.CpCost);
        _card.TooltipText = $"{doctrine.AbilityName}  [4]\n{doctrine.AbilityDescription}";
        _card.Pressed += () => ToggleTargeting();
        Refresh();
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
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Escape }) { CancelTargeting(); GetViewport().SetInputAsHandled(); return; }
        // Tower / TowerThenPad resolve via TowerClickedEvent/BuildPadClickedEvent instead.
        if (mode is DoctrineTargetingMode.Tower or DoctrineTargetingMode.TowerThenPad or DoctrineTargetingMode.None) return;
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
        if (doctrines.IsExhausted) return "Used this mission.";
        int shortfall = doctrines.CpCost - _mission.CommandPoints.Balance;
        return shortfall > 0 ? $"Need {shortfall} more CP for {doctrines.Doctrine?.AbilityName}." : "Cooling down.";
    }

    private void OnCommandPointsChanged(CommandPointsChangedEvent evt) => Refresh();

    private void Refresh()
    {
        var doctrines = _mission?.Doctrines;
        if (_card == null || doctrines?.Doctrine?.Ability == null) return;

        bool affordable = _mission.CommandPoints.Balance >= doctrines.CpCost;
        _card.SetState(doctrines.CooldownRemaining, doctrines.Doctrine.Ability.CooldownSeconds, affordable, _targeting, doctrines.IsExhausted);
        if (_hotbar != null) _hotbar.ExternalStatus = _lastStatus;
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
