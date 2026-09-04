using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.Waves;

namespace FrontsOfWar.UI.Hud;

// The in-mission HUD (GDD §13.4, §19 prompt 18). Built programmatically
// rather than hand-authored as a .tscn — this is a text-native, agent-
// editable approach just like the scene files (GDD §15.1 principle 7), and
// keeps a fast-moving UI file from accumulating brittle hand-written anchor/
// offset blocks while it's still being iterated on. Split into static
// chrome (this CanvasLayer) so a floating health bar moving never forces a
// redraw of unrelated UI (GDD §3.3's performance note).
public partial class HudController : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private Label _supplyLabel;
    private Label _commandPointsLabel;
    private Label _defenseLineLabel;
    private Label _waveLabel;
    private Label _speedLabel;
    private Label _buildLabel;
    private Button _callWaveButton;
    private AbilityHotbar _abilityHotbar;
    private DoctrineAbilitySlot _doctrineSlot;
    private BuildBar _buildBar;

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);
        BuildLayout();

        EventBus.Instance?.Subscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Subscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        EventBus.Instance?.Subscribe<DefenseLineChangedEvent>(OnDefenseLineChanged);
        EventBus.Instance?.Subscribe<WaveStartedEvent>(OnWaveStarted);

        // Deferred: Godot calls a child's _Ready() before its parent's, so
        // MapRuntime._Ready() (which creates Supply/CommandPoints/etc.)
        // hasn't necessarily run yet at this point — see docs/DECISIONS.md.
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
        // Top-left: Supply, Command Points, Defense Line (§13.4).
        var topLeft = new VBoxContainer { Position = new Vector2(16, 12) };
        AddChild(topLeft);
        _supplyLabel = MakeLabel(topLeft);
        _commandPointsLabel = MakeLabel(topLeft);
        _defenseLineLabel = MakeLabel(topLeft);

        // Top-center: wave counter.
        var topCenter = new VBoxContainer { Position = new Vector2(500, 12) };
        AddChild(topCenter);
        _waveLabel = MakeLabel(topCenter);

        // Top-right: speed control (§13.4, §7.7 — Space cycles 1x/2x/3x).
        var topRight = new HBoxContainer { Position = new Vector2(1000, 12) };
        AddChild(topRight);
        _speedLabel = MakeLabel(topRight);
        var speedButton = new Button { Text = "Speed" };
        speedButton.Pressed += OnSpeedButtonPressed;
        topRight.AddChild(speedButton);
        var pauseButton = new Button { Text = "Pause" };
        pauseButton.Pressed += OnPauseButtonPressed;
        topRight.AddChild(pauseButton);

        _buildLabel = new Label();
        AddChild(_buildLabel);
        _buildLabel.Position = new Vector2(16, 540);
        _callWaveButton = new Button { Text = "Call Wave Early", Position = new Vector2(180, 532), Visible = false };
        _callWaveButton.Pressed += () => _mission.CallNextWaveEarly();
        AddChild(_callWaveButton);

        _abilityHotbar = new AbilityHotbar
        {
            Mission = _mission,
            Position = new Vector2(750, 535),
        };
        AddChild(_abilityHotbar);

        _doctrineSlot = new DoctrineAbilitySlot
        {
            Mission = _mission,
            Position = new Vector2(750, 430),
        };
        AddChild(_doctrineSlot);

        // Bottom-center (§13.4). x:340-720 sits clear of the build-phase
        // label/Call Wave Early button (bottom-left, ends ~310) and the
        // ability hotbar (bottom-right, starts at x=750).
        _buildBar = new BuildBar
        {
            Mission = _mission,
            Position = new Vector2(340, 528),
        };
        AddChild(_buildBar);
    }

    private static Label MakeLabel(Container parent)
    {
        var label = new Label();
        parent.AddChild(label);
        return label;
    }

    private void OnSpeedButtonPressed()
    {
        GameLoop.Instance.Time.CycleSpeed();
        RefreshSpeed();
    }

    private void OnPauseButtonPressed()
    {
        GameLoop.Instance.Time.TogglePause();
        RefreshSpeed();
    }

    private void OnSupplyChanged(SupplyChangedEvent evt) => RefreshSupply();
    private void OnCommandPointsChanged(CommandPointsChangedEvent evt) => RefreshCommandPoints();
    private void OnDefenseLineChanged(DefenseLineChangedEvent evt) => RefreshDefenseLine();
    private void OnWaveStarted(WaveStartedEvent evt) => RefreshWave(evt.WaveNumber);

    private void RefreshAll()
    {
        RefreshSupply();
        RefreshCommandPoints();
        RefreshDefenseLine();
        RefreshSpeed();
        RefreshWave(_mission.Waves?.CurrentWaveNumber ?? 0);
        RefreshBuildPhase();
    }

    private void RefreshSupply() => _supplyLabel.Text = $"Supply: {_mission.Supply.Balance}";
    private void RefreshCommandPoints() => _commandPointsLabel.Text = $"Command Points: {_mission.CommandPoints.Balance}";

    private void RefreshDefenseLine()
        => _defenseLineLabel.Text = $"Defense Line: {_mission.DefenseLine.Integrity} / {_mission.DefenseLine.MaxIntegrity}";

    private void RefreshWave(int waveNumber) => _waveLabel.Text = $"Wave: {waveNumber}";

    private void RefreshBuildPhase()
    {
        bool isBuild = _mission.IsBuildPhase;
        _buildLabel.Text = isBuild
            ? $"BUILD PHASE  /  next wave in {_mission.BuildTimeRemaining:0.0}s"
            : "ENGAGED";
        _callWaveButton.Visible = isBuild;
    }

    public override void _Process(double delta) => RefreshBuildPhase();

    private void RefreshSpeed()
    {
        var time = GameLoop.Instance.Time;
        _speedLabel.Text = time.IsPaused ? "PAUSED" : $"{(int)time.CurrentSpeed}x";
    }
}
