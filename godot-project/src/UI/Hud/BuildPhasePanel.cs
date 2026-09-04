using Godot;
using System;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using FrontsOfWar.UI.Theme;
using FrontsOfWar.Waves;

namespace FrontsOfWar.UI.Hud;

// HUD zone D (docs/UI_DESIGN_SPEC.md §8.4; GDD §7.7, §13.4): during build
// time a countdown ring with the seconds left and the one-click "Call Wave
// Early" button showing the exact Supply it pays; during a wave just the
// ENGAGED caption. Refreshed by HudController every frame from the live
// mission state - the simulation owns the timer.
public partial class BuildPhasePanel : PanelContainer
{
    private Label _caption;
    private Label _seconds;
    private CooldownRing _ring;
    private Button _callButton;
    private HBoxContainer _timerRow;
    private float _phaseDuration = 25f;
    private bool _shownBuild = true;

    public event Action CallWaveEarly;

    public override void _Ready()
    {
        ThemeTypeVariation = "SlatePanel";
        var column = UiFactory.VBox(6);
        AddChild(column);

        _caption = UiFactory.Label("CaptionLabel", "BUILD PHASE", uppercase: true);
        column.AddChild(_caption);

        _timerRow = UiFactory.HBox(10);
        column.AddChild(_timerRow);

        var ringBox = new Control { CustomMinimumSize = new Vector2(44f, 44f), MouseFilter = MouseFilterEnum.Ignore };
        _timerRow.AddChild(ringBox);
        _ring = new CooldownRing { Mode = CooldownRing.Style.Ring, Thickness = 5f };
        _ring.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        ringBox.AddChild(_ring);
        _seconds = UiFactory.Label("BodyLabel", "25", HorizontalAlignment.Center);
        _seconds.VerticalAlignment = VerticalAlignment.Center;
        _seconds.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        ringBox.AddChild(_seconds);

        _callButton = UiFactory.Button("PrimaryButton", "Call Wave Early  +0", () => CallWaveEarly?.Invoke(), "call_wave_early");
        _callButton.FocusMode = FocusModeEnum.None;
        _callButton.CustomMinimumSize = new Vector2(0f, 44f);
        _callButton.TooltipText = "Start the next wave now and bank the remaining build-time bonus";
        _timerRow.AddChild(_callButton);

        EventBus.Instance?.Subscribe<BuildPhaseStartedEvent>(OnBuildPhaseStarted);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<BuildPhaseStartedEvent>(OnBuildPhaseStarted);
    }

    private void OnBuildPhaseStarted(BuildPhaseStartedEvent evt) => _phaseDuration = Mathf.Max(1f, evt.DurationSeconds);

    public void Refresh(MapRuntime mission)
    {
        if (mission == null) return;
        bool build = mission.IsBuildPhase;
        if (build != _shownBuild)
        {
            _shownBuild = build;
            _caption.Text = build ? "BUILD PHASE" : "ENGAGED";
            _timerRow.Visible = build;
        }
        if (!build) return;

        float remaining = mission.BuildTimeRemaining;
        _ring.SetFraction(remaining / _phaseDuration);
        string seconds = Mathf.CeilToInt(remaining).ToString();
        if (_seconds.Text != seconds) _seconds.Text = seconds;
        string call = $"Call Wave Early  +{mission.EarlyCallBonusNow}";
        if (_callButton.Text != call) _callButton.Text = call;
    }
}
