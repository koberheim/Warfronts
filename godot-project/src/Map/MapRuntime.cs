using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Map;

// The live orchestrator for one mission (GDD §15.2 file tree). Owns every
// per-mission manager and runs GameLoop's deterministic system order each
// tick (§15.4): Time → Spawns → Movement → Targeting/Firing → Projectiles →
// Cleanup. Registers itself as GameLoop.CurrentMission so GameLoop can drive
// it without depending on this namespace (see ISimTickable).
public partial class MapRuntime : Node2D, ISimTickable
{
    [Export] public Difficulty Difficulty = Difficulty.Regular;
    [Export] public NodePath PathNetworkPath;
    [Export] public NodePath EnemyContainerPath;
    [Export] public NodePath ProjectileContainerPath;
    [Export] public NodePath TowerContainerPath;
    [Export] public NodePath CommandPostContainerPath;

    // Dev-only: if set, this wave starts immediately on mission load. No
    // mission flow (briefing → build → wave sequence) exists yet — that's
    // M3 — so this is how M1/M2 test scenes exercise WaveRunner end to end.
    [Export] public WaveDefinition DebugStartWave;
    [Export] public bool DebugLogEvents;

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

    private SpatialGrid _spatialGrid;
    private DebugEventLogger _debugLogger;

    public override void _Ready()
    {
        Path = GetNode<PathNetwork>(PathNetworkPath);
        var enemyContainer = GetNode<Node>(EnemyContainerPath);
        var projectileContainer = GetNode<Node>(ProjectileContainerPath);

        var config = GameBalanceConfigAutoload.Config;

        Projectiles = new ProjectileManager(projectileContainer);
        Supply = new SupplyLedger(Difficulty, config);
        DefenseLine = new DefenseLineLedger(Difficulty, config);
        CommandPoints = new CommandPointLedger(config, () => CommandPosts.TotalCommandPointBonus());
        Abilities = new AbilitySystem(config);
        Waves = new WaveRunner(Enemies, Path, enemyContainer);

        float cellSizePixels = config.SpatialGridCellSizeTiles * config.TilePixelSize;
        _spatialGrid = new SpatialGrid(cellSizePixels);

        // Register any towers already placed in the scene (e.g. for an M1
        // test scene). Done here rather than each TowerController
        // self-registering in its own _Ready(), because Godot calls
        // children's _Ready() before their parent's — a tower under this
        // node would run before GameLoop.CurrentMission is even set.
        if (TowerContainerPath != null)
        {
            var towerContainer = GetNodeOrNull<Node>(TowerContainerPath);
            if (towerContainer != null)
                foreach (var child in towerContainer.GetChildren())
                    if (child is TowerController tower)
                        Towers.Register(tower);
        }

        if (CommandPostContainerPath != null)
        {
            var cpContainer = GetNodeOrNull<Node>(CommandPostContainerPath);
            if (cpContainer != null)
                foreach (var child in cpContainer.GetChildren())
                    if (child is CommandPostController post)
                        CommandPosts.Register(post);
        }

        if (DebugLogEvents) _debugLogger = new DebugEventLogger();
        if (DebugStartWave != null) Waves.StartWave(DebugStartWave);

        GameLoop.Instance.CurrentMission = this;
    }

    public override void _ExitTree()
    {
        Supply?.Dispose();
        DefenseLine?.Dispose();
        CommandPoints?.Dispose();
        _debugLogger?.Dispose();
        if (GameLoop.Instance != null && GameLoop.Instance.CurrentMission == (ISimTickable)this)
            GameLoop.Instance.CurrentMission = null;
    }

    public void SimTick(float tickDeltaSeconds)
    {
        var config = GameBalanceConfigAutoload.Config;

        Waves.Tick(tickDeltaSeconds);
        Enemies.Tick(tickDeltaSeconds);
        _spatialGrid.Rebuild(Enemies.GetTargetables());
        CommandPosts.Tick(tickDeltaSeconds, Towers, config.TilePixelSize);
        Towers.Tick(tickDeltaSeconds, _spatialGrid, Projectiles);
        Projectiles.Tick(tickDeltaSeconds, _spatialGrid);
        Abilities.Tick(tickDeltaSeconds, _spatialGrid);
    }

    public void RegisterTower(TowerController tower) => Towers.Register(tower);
    public void StartWave(WaveDefinition wave) => Waves.StartWave(wave);

    public bool ActivateAbility(Economy.AbilityType type, Vector2 targetPoint)
        => Abilities.TryActivate(type, targetPoint, CommandPoints, Towers, DefenseLine);
}
