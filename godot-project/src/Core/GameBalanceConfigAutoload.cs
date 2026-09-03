using Godot;

namespace FrontsOfWar.Core;

// Loads the singleton GameBalanceConfig resource and exposes it statically.
// Falls back to the Resource's own field defaults if no .tres override has
// been authored yet at res://assets/data/config/game_balance_config.tres.
public partial class GameBalanceConfigAutoload : Node
{
    private const string ResourcePath = "res://assets/data/config/game_balance_config.tres";

    public static GameBalanceConfig Config { get; private set; }

    public override void _EnterTree()
    {
        Config = ResourceLoader.Exists(ResourcePath)
            ? GD.Load<GameBalanceConfig>(ResourcePath)
            : new GameBalanceConfig();
    }
}
