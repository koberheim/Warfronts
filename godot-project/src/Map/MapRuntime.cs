using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Meta;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;
using System.Linq;

namespace FrontsOfWar.Map;

// The live orchestrator for one mission (GDD §15.2 file tree). Owns every
// per-mission manager and runs GameLoop's deterministic system order each
// tick (§15.4): Time → Spawns → Movement → Targeting/Firing → Projectiles →
// Cleanup. Registers itself as GameLoop.CurrentMission so GameLoop can drive
// it without depending on this namespace (see ISimTickable).
public partial class MapRuntime : Node2D, ISimTickable
{
    [Export] public Difficulty Difficulty = Difficulty.Regular;
    [Export] public int MissionSeed = 1;
    [Export] public NodePath PathNetworkPath;
    [Export] public NodePath EnemyContainerPath;
    [Export] public NodePath ProjectileContainerPath;
    [Export] public NodePath TowerContainerPath;
    [Export] public NodePath CommandPostContainerPath;
    [Export] public NodePath FriendlyContainerPath;
    [Export] public NodePath ArsenalPath;
    [Export] public NodePath SignatureContainerPath;
    [Export] public NodePath MinefieldContainerPath;
    [Export] public AirCorridorDefinition AirCorridor;

    // Dev-only: if set, this wave starts immediately on mission load. No
    // mission flow (briefing → build → wave sequence) uses the same debug
    // hooks; DebugSingleWave remains useful for fast system checks.
    [Export] public WaveDefinition DebugStartWave;
    [Export] public WaveSequence DebugWaveSequence;
    [Export] public bool DebugLogEvents;
    [Export] public bool DebugSingleWave;
    [Export] public float MissionBuildTimeSeconds = 25f;

    public PathNetwork Path { get; private set; }
    public EnemyManager Enemies { get; } = new();
    public TowerManager Towers { get; } = new();
    public CommandPostManager CommandPosts { get; } = new();
    public ProjectileManager Projectiles { get; private set; }
    public SupplyLedger Supply { get; private set; }
    public DefenseLineLedger DefenseLine { get; private set; }
    public CommandPointLedger CommandPoints { get; private set; }
    public AbilitySystem Abilities { get; private set; }
    public WaveRunner Waves { get; private set; }
    public FriendlyUnitManager FriendlyUnits { get; private set; }
    public SignatureManager Signatures { get; } = new();
    public MinefieldManager Minefields { get; } = new();
    public SeededRandom Random { get; private set; }
    public TowerPlacementService Placement { get; private set; }
    public DoctrineSystem Doctrines { get; private set; }
    public MissionStatsCollector Stats { get; private set; }

    private SpatialGrid _spatialGrid;
    private DebugEventLogger _debugLogger;
    private ArsenalController _arsenal;
    private float _buildTimeRemaining;
    private bool _waitingForBuild;
    private bool _missionOver;
    private bool _victoryPublished;

    public override void _Ready()
    {
        Path = GetNode<PathNetwork>(PathNetworkPath);
        var enemyContainer = GetNode<Node>(EnemyContainerPath);
        var projectileContainer = GetNode<Node>(ProjectileContainerPath);
        var friendlyContainer = FriendlyContainerPath == null
            ? this
            : GetNode<Node>(FriendlyContainerPath);

        var config = GameBalanceConfigAutoload.Config;
        Random = new SeededRandom(unchecked((ulong)(uint)MissionSeed));

        Projectiles = new ProjectileManager(projectileContainer);
        Supply = new SupplyLedger(Difficulty, config);
        DefenseLine = new DefenseLineLedger(Difficulty, config);
        CommandPoints = new CommandPointLedger(config, () => CommandPosts.TotalCommandPointBonus());
        Abilities = new AbilitySystem(config);
        Waves = new WaveRunner(Enemies, Path, enemyContainer);
        Stats = new MissionStatsCollector(Difficulty, DefenseLine, () => Waves.CurrentWaveNumber);
        Enemies.AirCorridor = AirCorridor;
        Enemies.SiegeTargetsProvider = () => Towers.Towers.Select(tower => (ISiegeTarget)tower).ToArray();
        FriendlyUnits = new FriendlyUnitManager(friendlyContainer);
        EventBus.Instance?.Subscribe<BossAddsRequestedEvent>(OnBossAddsRequested);
        EventBus.Instance?.Subscribe<BossReachedObjectiveEvent>(OnBossReachedObjective);
        EventBus.Instance?.Subscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);

        float cellSizePixels = config.SpatialGridCellSizeTiles * config.TilePixelSize;
        _spatialGrid = new SpatialGrid(cellSizePixels);

        // Register any towers already placed in the scene (e.g. for an M1
        // test scene). Done here rather than each TowerController
        // self-registering in its own _Ready(), because Godot calls
        // children's _Ready() before their parent's — a tower under this
        // node would run before GameLoop.CurrentMission is even set.
        Node towerContainer = null;
        if (TowerContainerPath != null)
        {
            towerContainer = GetNodeOrNull<Node>(TowerContainerPath);
            if (towerContainer != null)
                foreach (var child in towerContainer.GetChildren())
                    if (child is TowerController tower)
                        Towers.Register(tower);
        }

        Node commandPostContainer = null;
        if (CommandPostContainerPath != null)
        {
            commandPostContainer = GetNodeOrNull<Node>(CommandPostContainerPath);
            if (commandPostContainer != null)
                foreach (var child in commandPostContainer.GetChildren())
                    if (child is CommandPostController post)
                        CommandPosts.Register(post);
        }

        // Built here (not lazily) so it's ready before the HUD's build bar
        // reads it in its own _Ready() — see D21 in docs/DECISIONS.md on
        // child-before-parent ordering. Falls back to this mission root for
        // either container so a test/mission scene without a dedicated
        // CommandPostContainer can still place T9 (matches the
        // FriendlyContainerPath fallback above).
        Placement = new TowerPlacementService(
            towerContainer ?? this, commandPostContainer ?? this, Supply, Towers, CommandPosts);

        // Doctrines (GDD §8.3, §19 prompt 39) — built after every manager it
        // touches exists, before Signatures/Minefields.Initialize below.
        // United States only, since no other nation is selectable yet (§13.3
        // is deferred — see LoadoutController).
        var doctrine = DoctrineSystem.LoadDoctrine("united_states", MissionSession.SelectedDoctrineId);
        Doctrines = new DoctrineSystem(doctrine, config, Towers, CommandPosts, Minefields,
            Signatures, FriendlyUnits, Path, Placement, Projectiles, CommandPoints, Supply, DefenseLine);
        Placement.DoctrineCostMultiplierProvider = Doctrines.PlacementCostMultiplier;
        Doctrines.ApplyMissionStart();

        if (ArsenalPath != null)
        {
            _arsenal = GetNodeOrNull<ArsenalController>(ArsenalPath);
            _arsenal?.Initialize(FriendlyUnits, Path);
        }

        if (SignatureContainerPath != null)
        {
            var signatureContainer = GetNodeOrNull<Node>(SignatureContainerPath);
            if (signatureContainer != null)
                foreach (var child in signatureContainer.GetChildren())
                    if (child is SignatureControllerBase signature) Signatures.Register(signature);
        }
        Signatures.Initialize(Path, () => Enemies.GetTargetables(), () => Towers.Towers);

        if (MinefieldContainerPath != null)
        {
            var minefieldContainer = GetNodeOrNull<Node>(MinefieldContainerPath);
            if (minefieldContainer != null)
                foreach (var child in minefieldContainer.GetChildren())
                    if (child is MinefieldController field) Minefields.Register(field);
        }
        Minefields.Initialize(() => Enemies.GetTargetables());

        if (DebugLogEvents) _debugLogger = new DebugEventLogger();
        if (DebugWaveSequence?.Waves is { Length: > 0 } sequence)
        {
            int startIndex = RequestedDebugWaveIndex(sequence);
            Waves.StartWave(sequence[startIndex]);
            if (!DebugSingleWave)
                for (int i = startIndex + 1; i < sequence.Length; i++) Waves.QueueWaves(new[] { sequence[i] });
        }
        else if (DebugStartWave != null)
        {
            Waves.StartWave(DebugStartWave);
        }

        GameLoop.Instance.CurrentMission = this;
    }

    private static int RequestedDebugWaveIndex(WaveDefinition[] sequence)
    {
        var args = OS.GetCmdlineArgs();
        for (int i = 0; i + 1 < args.Length; i++)
        {
            if (args[i] == "--wave" && int.TryParse(args[i + 1], out int requested))
            {
                for (int waveIndex = 0; waveIndex < sequence.Length; waveIndex++)
                    if (sequence[waveIndex].WaveNumber == requested) return waveIndex;
            }
        }
        return 0;
    }

    public override void _ExitTree()
    {
        Supply?.Dispose();
        DefenseLine?.Dispose();
        CommandPoints?.Dispose();
        Stats?.Dispose();
        _debugLogger?.Dispose();
        EventBus.Instance?.Unsubscribe<BossAddsRequestedEvent>(OnBossAddsRequested);
        EventBus.Instance?.Unsubscribe<BossReachedObjectiveEvent>(OnBossReachedObjective);
        EventBus.Instance?.Unsubscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
        if (GameLoop.Instance != null && GameLoop.Instance.CurrentMission == (ISimTickable)this)
            GameLoop.Instance.CurrentMission = null;
    }

    public void SimTick(float tickDeltaSeconds)
    {
        if (_missionOver) return;
        var config = GameBalanceConfigAutoload.Config;

        Waves.Tick(tickDeltaSeconds);
        FriendlyUnits.Tick(tickDeltaSeconds, Enemies);
        Enemies.Tick(tickDeltaSeconds);
        _spatialGrid.Rebuild(Enemies.GetTargetables());
        CommandPosts.RevealTargets(Enemies.Enemies, config.TilePixelSize);
        CommandPosts.Tick(tickDeltaSeconds, Towers, config.TilePixelSize);
        Towers.ResetSignatureModifiers();
        Signatures.Tick(tickDeltaSeconds);
        Minefields.Tick(tickDeltaSeconds);
        Towers.Tick(tickDeltaSeconds, _spatialGrid, Projectiles);
        Projectiles.Tick(tickDeltaSeconds, _spatialGrid);
        Abilities.Tick(tickDeltaSeconds, _spatialGrid);
        Doctrines?.Tick(tickDeltaSeconds, _spatialGrid);

        if (!_victoryPublished && !_waitingForBuild && !Waves.IsRunning && Enemies.Enemies.Count == 0
            && Waves.PeekUpcoming(1).Count > 0)
        {
            Supply.Credit(Supply.EndOfWaveIncome(Waves.CurrentWaveNumber));
            _waitingForBuild = true;
            var nextWave = Waves.PeekUpcoming(1)[0];
            _buildTimeRemaining = nextWave.IsBossWave ? 40f : MissionBuildTimeSeconds;
            EventBus.Instance?.Publish(new BuildPhaseStartedEvent(Waves.CurrentWaveNumber + 1, _buildTimeRemaining));
        }

        if (_waitingForBuild)
        {
            _buildTimeRemaining -= tickDeltaSeconds;
            if (_buildTimeRemaining <= 0f)
            {
                _waitingForBuild = false;
                Waves.StartNextWave();
            }
        }

        if (!_victoryPublished && !_waitingForBuild && !Waves.IsRunning && Enemies.Enemies.Count == 0
            && Waves.PeekUpcoming(1).Count == 0 && Waves.CurrentWaveNumber > 0)
        {
            Supply.Credit(Supply.EndOfWaveIncome(Waves.CurrentWaveNumber));
            _victoryPublished = true;
            _missionOver = true;
            MissionSession.LastMissionWon = true;
            MissionSession.LastWaveReached = Waves.CurrentWaveNumber;
            EventBus.Instance?.Publish(new MissionCompletedEvent(true));
        }
    }

    public void RegisterTower(TowerController tower) => Towers.Register(tower);
    public void StartWave(WaveDefinition wave) => Waves.StartWave(wave);

    public float BuildTimeRemaining => Mathf.Max(0f, _buildTimeRemaining);
    public bool IsBuildPhase => _waitingForBuild;

    public void CallNextWaveEarly()
    {
        if (!_waitingForBuild) return;
        float fraction = Mathf.Clamp(_buildTimeRemaining / Mathf.Max(1f, MissionBuildTimeSeconds), 0f, 1f);
        Supply.Credit(Supply.EarlyCallBonus(Waves.CurrentWaveNumber, fraction));
        _waitingForBuild = false;
        Waves.StartNextWave();
    }

    public bool ActivateAbility(Economy.AbilityType type, Vector2 targetPoint)
        => Abilities.TryActivate(type, targetPoint, CommandPoints, Towers, DefenseLine);

    public bool ActivateDoctrineAbility(Vector2 primaryPoint, Vector2? secondaryPoint = null,
        TowerController towerTarget = null, BuildPad padTarget = null)
        => Doctrines?.TryActivate(primaryPoint, _spatialGrid, secondaryPoint, towerTarget, padTarget) ?? false;

    private void OnBossAddsRequested(BossAddsRequestedEvent evt)
    {
        if (evt.Boss?.BossPhase == null || evt.Boss.Definition.AddDefinition == null) return;
        for (int i = 0; i < evt.Count; i++)
            Enemies.Spawn(evt.Boss.Definition.AddDefinition, Path, evt.Boss.GetParent(), 1f);
    }

    private void OnBossReachedObjective(BossReachedObjectiveEvent evt)
    {
        if (!_missionOver) DefenseLine.ForceDeplete();
    }

    private void OnDefenseLineDepleted(DefenseLineDepletedEvent evt)
    {
        if (_missionOver) return;
        _missionOver = true;
        MissionSession.LastMissionWon = false;
        MissionSession.LastWaveReached = Waves?.CurrentWaveNumber ?? 0;
        EventBus.Instance?.Publish(new MissionCompletedEvent(false));
    }
}
