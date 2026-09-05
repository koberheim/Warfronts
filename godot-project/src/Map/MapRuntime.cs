using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Meta;
using FrontsOfWar.Map.Authoring;
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
    [Export] public bool DeveloperFixture;
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
    public SpecialPlacementService SpecialPlacement { get; private set; }
    public GimmickSystem Gimmicks { get; private set; }
    public DoctrineSystem Doctrines { get; private set; }
    public MissionStatsCollector Stats { get; private set; }
    public MapDefinition AuthoringMap { get; private set; }
    public RuntimeMapData AuthoringRuntimeData { get; private set; }

    private SpatialGrid _spatialGrid;
#if DEBUG
    private DebugEventLogger _debugLogger;
#endif
    private ArsenalController _arsenal;
    private float _buildTimeRemaining;
    private float _buildPhaseDuration;
    private bool _waitingForBuild;
    private bool _missionOver;
    private bool _victoryPublished;

    public override void _Ready()
    {
        try { InitializeMission(); }
        catch (System.Exception error)
        {
            _missionOver = true;
            GD.PushError($"Mission initialization failed: {error.Message}");
            GetTree().Quit(1);
        }
    }

    private void InitializeMission()
    {
        var missionDefinition = LoadMissionLayout();
        var missionSequence = IsDeveloperFixture ? DebugWaveSequence : missionDefinition.WaveSequence;
        Gimmicks = new GimmickSystem(AuthoringRuntimeData?.Gimmicks ?? System.Array.Empty<RuntimeGimmickData>());
        Path = GetNode<PathNetwork>(PathNetworkPath);
        var enemyContainer = GetNode<Node>(EnemyContainerPath);
        var projectileContainer = GetNode<Node>(ProjectileContainerPath);
        var friendlyContainer = FriendlyContainerPath == null
            ? this
            : GetNode<Node>(FriendlyContainerPath);

        var config = GameBalanceConfigAutoload.Config;
        if (missionSequence != null) Enemies.Prepare(missionSequence, enemyContainer, config);
        else if (DebugStartWave != null) Enemies.Prepare(new WaveSequence { Waves = new[] { DebugStartWave } }, enemyContainer, config);
        Random = new SeededRandom(unchecked((ulong)(uint)MissionSeed));

        Projectiles = new ProjectileManager(projectileContainer);
        Supply = new SupplyLedger(Difficulty, config);
        DefenseLine = new DefenseLineLedger(Difficulty, config);
        CommandPoints = new CommandPointLedger(config, () => CommandPosts.TotalCommandPointBonus());
        Abilities = new AbilitySystem(config);
        var pathNetworks = GetChildren().OfType<PathNetwork>().ToList();
        var authoredPaths = new PathNetworkSet();
        foreach (var network in pathNetworks) authoredPaths.Add(network);
        Waves = new WaveRunner(Enemies, authoredPaths, Path, enemyContainer);
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
        // Signature/Arsenal build-slot placement isn't wired into the HUD yet
        // (loadout's recommended kit places its signature by scene authoring,
        // not a player click — see docs/PROGRESS.md); ExtraMinefieldCapacity
        // stays a neutral 0 until a doctrine actually grants a field-count
        // bonus (Island Defense, GDD §6 T8) — nothing sets it yet.
        SpecialPlacement = new SpecialPlacementService(
            this, Supply, Signatures, Minefields, FriendlyUnits, pathNetworks, config)
        {
            ExtraMinefieldCapacity = () => 0,
        };

        // Doctrines (GDD §8.3, §19 prompt 39) — built after every manager it
        // touches exists, before Signatures/Minefields.Initialize below.
        var doctrine = DoctrineSystem.LoadDoctrine(MissionSession.CurrentNationId, MissionSession.SelectedDoctrineId);
        var friendlyScenes = new System.Collections.Generic.List<PackedScene>();
        if (doctrine?.Ability?.FriendlyUnitScene != null) friendlyScenes.Add(doctrine.Ability.FriendlyUnitScene);
        var authoredArsenal = ArsenalPath == null ? null : GetNodeOrNull<ArsenalController>(ArsenalPath);
        if (authoredArsenal?.Definition?.UnitScene != null) friendlyScenes.Add(authoredArsenal.Definition.UnitScene);
        FriendlyUnits.Prepare(friendlyScenes, config);
        Doctrines = new DoctrineSystem(doctrine, config, Towers, CommandPosts, Minefields,
            Signatures, FriendlyUnits, Path, Placement, Projectiles, CommandPoints, Supply, DefenseLine);
        Placement.DoctrineCostMultiplierProvider = Doctrines.PlacementCostMultiplier;
        SpecialPlacement.CostMultiplier = Doctrines.PlacementCostMultiplier;
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

#if DEBUG
        if (DebugLogEvents) _debugLogger = new DebugEventLogger();
#endif
        if (missionSequence?.Waves is { Length: > 0 } sequence)
        {
            TotalWaves = sequence.Length;
            int startIndex = RequestedDebugWaveIndex(sequence);
            if (IsDeveloperFixture)
            {
                Waves.StartWave(sequence[startIndex]);
                if (!DebugSingleWave) Waves.QueueWaves(sequence.Skip(startIndex + 1));
            }
            else
            {
                Waves.QueueWaves(sequence.Skip(startIndex));
                BeginBuildPhase();
            }
        }
        else if (DebugStartWave != null)
        {
            Waves.StartWave(DebugStartWave);
        }

        GameLoop.Instance.CurrentMission = this;
    }

    private static int RequestedDebugWaveIndex(WaveDefinition[] sequence)
    {
#if DEBUG
        var args = OS.GetCmdlineArgs();
        for (int i = 0; i + 1 < args.Length; i++)
        {
            if (args[i] == "--wave" && int.TryParse(args[i + 1], out int requested))
            {
                for (int waveIndex = 0; waveIndex < sequence.Length; waveIndex++)
                    if (sequence[waveIndex].WaveNumber == requested) return waveIndex;
            }
        }
#endif
        return 0;
    }

    public override void _ExitTree()
    {
        if (AuthoringMap != null) GetViewport().SizeChanged -= FitMapCamera;
        Supply?.Dispose();
        DefenseLine?.Dispose();
        CommandPoints?.Dispose();
        Stats?.Dispose();
#if DEBUG
        _debugLogger?.Dispose();
#endif
        EventBus.Instance?.Unsubscribe<BossAddsRequestedEvent>(OnBossAddsRequested);
        EventBus.Instance?.Unsubscribe<BossReachedObjectiveEvent>(OnBossReachedObjective);
        EventBus.Instance?.Unsubscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
        if (GameLoop.Instance != null && GameLoop.Instance.CurrentMission == (ISimTickable)this)
            GameLoop.Instance.CurrentMission = null;
    }

}
