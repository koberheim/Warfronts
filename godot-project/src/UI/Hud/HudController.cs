using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.UI.Menus;
using FrontsOfWar.UI.Theme;
using FrontsOfWar.Waves;

namespace FrontsOfWar.UI.Hud;

// The in-mission HUD (GDD §13.4, §19 prompt 18; docs/UI_DESIGN_SPEC.md §2.1,
// §8.4). Composes the screen-space zones - resources (A), time controls
// (C), build phase (D), build bar (E), abilities (G), the pause banner and
// pause menu - and feeds them from EventBus. Built in code rather than a
// hand-authored .tscn (GDD §15.1 principle 7) with edge/center anchors so a
// wider window gains battlefield, not stretched chrome. The wave strip (B)
// is its own CanvasLayer in mission.tscn.
public partial class HudController : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private ResourcePanel _resources;
    private TimeControls _time;
    private BuildPhasePanel _buildPhase;
    private BuildBar _buildBar;
    private AbilityHotbar _abilities;
    private DoctrineAbilitySlot _doctrine;
    private PauseMenu _pauseMenu;
    private PanelContainer _pauseBanner;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _mission = GetNode<MapRuntime>(MissionPath);
        BuildLayout();

        EventBus.Instance?.Subscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Subscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        EventBus.Instance?.Subscribe<DefenseLineChangedEvent>(OnDefenseLineChanged);
        EventBus.Instance?.Subscribe<WaveStartedEvent>(OnWaveStarted);

        // Deferred: Godot calls a child's _Ready() before its parent's, so
        // MapRuntime._Ready() (which creates Supply/CommandPoints/etc.)
        // hasn't necessarily run yet at this point - see docs/DECISIONS.md.
        Callable.From(RefreshAll).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Unsubscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        EventBus.Instance?.Unsubscribe<DefenseLineChangedEvent>(OnDefenseLineChanged);
        EventBus.Instance?.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
    }

    private void BuildLayout()
    {
        // First child so it receives unhandled Esc last: build mode, ability
        // targeting and the inspection card all get to consume Esc before
        // the pause menu opens (unhandled input runs in reverse tree order).
        _pauseMenu = new PauseMenu();
        AddChild(_pauseMenu);

        _resources = new ResourcePanel();
        AddChild(_resources);
        UiFactory.Anchor(_resources, Control.LayoutPreset.TopLeft, 24, 16);

        _time = new TimeControls();
        AddChild(_time);
        UiFactory.Anchor(_time, Control.LayoutPreset.TopRight, 24, 16);
        _time.MenuRequested += _pauseMenu.Open;

        _pauseBanner = UiFactory.Panel("SlatePanelStrong");
        _pauseBanner.MouseFilter = Control.MouseFilterEnum.Ignore;
        _pauseBanner.Visible = false;
        var bannerColumn = UiFactory.VBox(2);
        _pauseBanner.AddChild(bannerColumn);
        var bannerTitle = UiFactory.Label("HeadingLabel", "PAUSED — PLANNING MODE", HorizontalAlignment.Center, uppercase: true);
        bannerTitle.AddThemeColorOverride("font_color", UiPalette.Amber);
        bannerColumn.AddChild(bannerTitle);
        bannerColumn.AddChild(UiFactory.Label("SmallLabel", "Building and upgrading stay available", HorizontalAlignment.Center));
        AddChild(_pauseBanner);
        UiFactory.Anchor(_pauseBanner, Control.LayoutPreset.CenterTop, 0, 124);

        _buildPhase = new BuildPhasePanel();
        AddChild(_buildPhase);
        UiFactory.Anchor(_buildPhase, Control.LayoutPreset.BottomLeft, 24, 24);
        _buildPhase.CallWaveEarly += () => _mission.CallNextWaveEarly();

        _buildBar = new BuildBar { Mission = _mission };
        AddChild(_buildBar);
        UiFactory.Anchor(_buildBar, Control.LayoutPreset.CenterBottom, 0, 24);

        _abilities = new AbilityHotbar { Mission = _mission };
        AddChild(_abilities);
        UiFactory.Anchor(_abilities, Control.LayoutPreset.BottomRight, 24, 24);

        _doctrine = new DoctrineAbilitySlot { Mission = _mission, Hotbar = _abilities };
        AddChild(_doctrine);
    }

    private void OnSupplyChanged(SupplyChangedEvent evt) => RefreshSupply();
    private void OnCommandPointsChanged(CommandPointsChangedEvent evt) => RefreshCommandPoints();
    private void OnDefenseLineChanged(DefenseLineChangedEvent evt) => RefreshDefenseLine();
    private void OnWaveStarted(WaveStartedEvent evt) => RefreshSupply();

    private void RefreshAll()
    {
        RefreshSupply();
        RefreshCommandPoints();
        RefreshDefenseLine();
        _time.Refresh();
    }

    private void RefreshSupply()
    {
        if (_mission?.Supply == null) return;
        int wave = Mathf.Max(1, _mission.Waves?.CurrentWaveNumber ?? 1);
        int projected = _mission.Supply.EndOfWaveIncome(wave) + _mission.CommandPosts.TotalSupplyPerWaveBonus();
        _resources.SetSupply(_mission.Supply.Balance, projected);
    }

    private void RefreshCommandPoints()
    {
        if (_mission?.CommandPoints != null) _resources.SetCommandPoints(_mission.CommandPoints.Balance);
    }

    private void RefreshDefenseLine()
    {
        if (_mission?.DefenseLine != null) _resources.SetDefenseLine(_mission.DefenseLine.Integrity, _mission.DefenseLine.MaxIntegrity);
    }

    public override void _Process(double delta)
    {
        _buildPhase.Refresh(_mission);
        _time.Refresh();
        bool paused = GameLoop.Instance?.Time.IsPaused == true;
        bool showBanner = paused && !_pauseMenu.IsOpen;
        if (_pauseBanner.Visible != showBanner) _pauseBanner.Visible = showBanner;
    }
}
