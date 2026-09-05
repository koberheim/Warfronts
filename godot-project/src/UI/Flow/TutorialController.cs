using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.Meta;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Flow;

// Eight short, integrated prompts on a paper card below the wave strip
// (docs/UI_DESIGN_SPEC.md §8.6; GDD §13.10). The game is paused while the
// card is up, so the tutorial teaches the player's first decisions without
// execution pressure. Copy stays as authored; only the presentation changed.
public partial class TutorialController : CanvasLayer
{
    private static readonly string[] Steps =
    {
        "1 / Defense Line — enemies follow the route to the objective. Stop them before they leak.",
        "2 / Build pads — click a pad to place your first tower before starting the first wave.",
        "3 / Damage types — Small Arms handles Soft targets; Armor-Piercing handles tanks.",
        "4 / Targeting — select a tower to inspect its priority, range, and lifetime damage.",
        "5 / Preview — the strip shows the next wave in full, then two waves of warning information.",
        "6 / Command Points — abilities appear in the bottom-right hotbar; pause still allows planning.",
        "7 / Signature — every nation has a signature. Complete one mission with a nation to unlock its signature.",
        "8 / Adapt — upgrades, sells, and the post-mortem explain what to change before retrying."
    };

    private PanelContainer _card;
    private readonly TextureRect[] _pips = new TextureRect[8];
    private Label _title;
    private Label _body;
    private Button _next;
    private int _step;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Callable.From(StartIfNeeded).CallDeferred();
    }

    private void StartIfNeeded()
    {
        if (MissionSession.TutorialCompleted || DisplayServer.GetName() == "headless") return;

        _card = UiFactory.Panel("PaperPanel");
        _card.CustomMinimumSize = new Vector2(640f, 0f);
        AddChild(_card);
        UiFactory.Anchor(_card, Control.LayoutPreset.CenterTop, 0, 196);

        var column = UiFactory.VBox(8);
        _card.AddChild(column);

        var header = UiFactory.HBox(8);
        column.AddChild(header);
        header.AddChild(UiFactory.Label("PaperSubheadingLabel", "TUTORIAL"));
        header.AddChild(UiFactory.Spacer(expand: true));
        var pipRow = UiFactory.HBox(4);
        header.AddChild(pipRow);
        for (int i = 0; i < _pips.Length; i++)
        {
            _pips[i] = UiFactory.Icon("level_pip_off", 10, UiPalette.InkMuted) ?? new TextureRect();
            pipRow.AddChild(_pips[i]);
        }

        _title = UiFactory.Label("PaperHeadingLabel", "");
        column.AddChild(_title);
        _body = UiFactory.Wrapped("PaperBodyLabel", "");
        column.AddChild(_body);

        _next = UiFactory.Button("PrimaryButton", "Next", Advance);
        _next.CustomMinimumSize = new Vector2(140f, 44f);
        var skip = UiFactory.Button("PaperButton", "Skip tutorial", Finish);
        var actions = UiFactory.HBox(8);
        actions.AddChild(skip);
        actions.AddChild(UiFactory.Spacer(expand: true));
        actions.AddChild(_next);
        column.AddChild(actions);

        GameLoop.Instance.Time.Pause();
        Refresh();
        UiFactory.FadeIn(_card);
        _next.GrabFocus();
    }

    private void Advance()
    {
        _step++;
        if (_step >= Steps.Length) { Finish(); return; }
        Refresh();
        _next.GrabFocus();
    }

    private void Finish()
    {
        MissionSession.TutorialCompleted = true;
        // Screenshot runs are visual fixtures, not player sessions: never
        // let a captured Skip/Begin interaction mutate a developer profile.
        if (ScreenshotCapture.ArgValue(OS.GetCmdlineArgs(), "--screenshot-dir") == null
            && !ProfileStore.TryCompleteTutorial(out string saveError))
            GD.PushWarning($"Tutorial completion will be retried on the next profile save: {saveError}");
        GameLoop.Instance.Time.Resume();
        _card.QueueFree();
        _card = null;
    }

    private void Refresh()
    {
        // Authored as "N / Title — body"; split for the card's heading.
        string text = Steps[_step];
        int slash = text.IndexOf('/');
        int dash = text.IndexOf('—');
        string title = dash > slash ? text[(slash + 1)..dash].Trim() : $"Step {_step + 1}";
        string body = dash > 0 ? text[(dash + 1)..].Trim() : text;
        _title.Text = title.ToUpperInvariant();
        _body.Text = body.Length > 0 ? char.ToUpperInvariant(body[0]) + body[1..] : body;
        for (int i = 0; i < _pips.Length; i++)
            _pips[i].Texture = UiIcons.Get(i <= _step ? "level_pip_on" : "level_pip_off");
        _next.Text = _step == Steps.Length - 1 ? "Begin" : "Next";
        EventBus.Instance?.Publish(new TutorialStepChangedEvent(_step));
    }
}
