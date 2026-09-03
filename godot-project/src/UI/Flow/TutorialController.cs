using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.UI.Flow;

// Eight short, integrated prompts. The game is paused while the card is up,
// so the tutorial teaches the player's first decisions without execution
// pressure and can point at the live battlefield in a later art pass.
public partial class TutorialController : CanvasLayer
{
    private static readonly string[] Steps =
    {
        "1 / Defense Line — enemies follow the route to the objective. Stop them before they leak.",
        "2 / Build pads — click a pad to place a tower. The current grey-box defense is already deployed.",
        "3 / Damage types — Small Arms handles Soft targets; Armor-Piercing handles tanks.",
        "4 / Targeting — select a tower to inspect its priority, range, and lifetime damage.",
        "5 / Preview — the strip shows the next wave in full, then two waves of warning information.",
        "6 / Command Points — abilities appear in the bottom-right hotbar; pause still allows planning.",
        "7 / Signature — the Arsenal factory produces friendly units that walk backward and stall enemies.",
        "8 / Adapt — upgrades, sells, and the post-mortem explain what to change before retrying."
    };

    private PanelContainer _panel;
    private Label _label;
    private int _step;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Callable.From(StartIfNeeded).CallDeferred();
    }

    private void StartIfNeeded()
    {
        if (MissionSession.TutorialCompleted || OS.HasFeature("headless")) return;
        _panel = new PanelContainer { Position = new Vector2(220, 180), CustomMinimumSize = new Vector2(600, 170) };
        AddChild(_panel);
        var box = new VBoxContainer();
        _panel.AddChild(box);
        _label = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        box.AddChild(_label);
        var next = new Button { Text = "Next", CustomMinimumSize = new Vector2(140, 42) };
        next.Pressed += Advance;
        box.AddChild(next);
        GameLoop.Instance.Time.Pause();
        Refresh();
    }

    private void Advance()
    {
        _step++;
        if (_step >= Steps.Length)
        {
            MissionSession.TutorialCompleted = true;
            GameLoop.Instance.Time.Resume();
            _panel.QueueFree();
            return;
        }
        Refresh();
    }

    private void Refresh()
    {
        _label.Text = $"TUTORIAL\n\n{Steps[_step]}\n\nPause-and-highlight tutorial — press Next to continue.";
        EventBus.Instance?.Publish(new TutorialStepChangedEvent(_step));
    }
}
