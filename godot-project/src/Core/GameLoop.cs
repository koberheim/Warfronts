using Godot;

namespace FrontsOfWar.Core;

// Fixed 60Hz simulation tick, decoupled from render framerate (GDD §15.1
// principle 4, §15.4). Runs off _PhysicsProcess (Godot's fixed-step
// callback) and owns the deterministic system update order. Game speed
// multiplies ticks-per-physics-frame rather than Engine.TimeScale.
public partial class GameLoop : Node
{
    public static GameLoop Instance { get; private set; }

    public TimeController Time { get; } = new();

    // The live mission, set by MapRuntime._Ready / cleared on _ExitTree.
    // Null outside a mission (main menu, loadout screens).
    public ISimTickable CurrentMission { get; set; }

    // Debug counter: at 1x/2x/3x speed this should advance at exactly
    // 60/120/180 ticks per second (M0 acceptance check).
    public ulong TickCount { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        Engine.PhysicsTicksPerSecond = 60;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        int ticks = Time.TicksThisFrame;
        for (int i = 0; i < ticks; i++)
            RunTick();
    }

    private void RunTick()
    {
        TickCount++;

        // Deterministic system order lives in MapRuntime.SimTick (GDD §15.4:
        // Time → Spawns → Movement → Targeting → Firing → Projectiles →
        // Damage → Status → Cleanup → UI) — GameLoop only owns timing.
        float fixedDelta = 1f / GameBalanceConfigAutoload.Config.SimulationHz;
        CurrentMission?.SimTick(fixedDelta);
    }
}
