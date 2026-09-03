using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.UI.Hud;

// Bottom-right universal ability bar (GDD §7.6, §13.4). Point abilities
// enter target mode so the same control works with mouse clicks and keys 1–4.
// The bar is deliberately text-first while the project is still using
// primitive prototype art; CP costs and cooldown state remain unambiguous.
public partial class AbilityHotbar : Control
{
    private readonly record struct AbilityEntry(AbilityType Type, string Name, Key Hotkey);

    private static readonly AbilityEntry[] Entries =
    {
        new(AbilityType.ArtilleryStrike, "Artillery Strike", Key.Key1),
        new(AbilityType.Rally, "Rally", Key.Key2),
        new(AbilityType.EmergencyRepair, "Emergency Repair", Key.Key3),
    };

    private MapRuntime _mission;
    private readonly Dictionary<AbilityType, Button> _buttons = new();
    private Label _statusLabel;
    private AbilityType? _selectedAbility;
    private string _lastStatus;

    public MapRuntime Mission
    {
        get => _mission;
        set => _mission = value;
    }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Ignore;
        CustomMinimumSize = new Vector2(390f, 92f);
        Size = CustomMinimumSize;

        var panel = new PanelContainer
        {
            CustomMinimumSize = CustomMinimumSize,
            Size = CustomMinimumSize,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        panel.AddThemeStyleboxOverride("panel", BuildPanelStyle());
        AddChild(panel);

        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.AddThemeConstantOverride("separation", 4);
        panel.AddChild(column);

        var title = new Label
        {
            Text = "TACTICAL ABILITIES",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        column.AddChild(title);

        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", 5);
        column.AddChild(row);

        foreach (var entry in Entries)
        {
            var button = new Button
            {
                CustomMinimumSize = new Vector2(120f, 48f),
                FocusMode = FocusModeEnum.None,
                TooltipText = $"{entry.Name} ({entry.Hotkey})",
            };
            button.AddThemeFontSizeOverride("font_size", 12);
            button.Pressed += () => SelectOrActivate(entry.Type);
            row.AddChild(button);
            _buttons.Add(entry.Type, button);
        }

        _statusLabel = new Label
        {
            Text = "Keys 1–3 select an ability; click the battlefield to target it.",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        column.AddChild(_statusLabel);

        EventBus.Instance?.Subscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        Callable.From(Refresh).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
    }

    public override void _Process(double delta)
    {
        // Cooldowns are simulation-owned, so the display is refreshed from
        // the live values rather than maintaining a second timer in the UI.
        Refresh();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            foreach (var entry in Entries)
            {
                if (key.Keycode != entry.Hotkey) continue;
                SelectOrActivate(entry.Type);
                GetViewport().SetInputAsHandled();
                return;
            }
        }

        if (_selectedAbility == null || @event is not InputEventMouseButton
            {
                Pressed: true,
                ButtonIndex: MouseButton.Left,
            } mouse)
            return;

        var worldPoint = GetViewport().GetCanvasTransform().AffineInverse() * mouse.Position;
        TryActivate(_selectedAbility.Value, worldPoint);
        GetViewport().SetInputAsHandled();
    }

    private void SelectOrActivate(AbilityType type)
    {
        if (type == AbilityType.EmergencyRepair)
        {
            TryActivate(type, Vector2.Zero);
            return;
        }

        _selectedAbility = _selectedAbility == type ? null : type;
        _lastStatus = null;
        Refresh();
    }

    private void TryActivate(AbilityType type, Vector2 targetPoint)
    {
        if (_mission == null) return;

        if (_mission.ActivateAbility(type, targetPoint))
        {
            _selectedAbility = null;
            _lastStatus = $"{DisplayName(type)} activated.";
        }
        else
        {
            int shortfall = _mission.Abilities.CpCost(type) - _mission.CommandPoints.Balance;
            _lastStatus = shortfall > 0
                ? $"Need {shortfall} more CP for {DisplayName(type)}."
                : $"{DisplayName(type)} is cooling down.";
        }

        Refresh();
    }

    private void OnCommandPointsChanged(CommandPointsChangedEvent evt) => Refresh();

    private void Refresh()
    {
        if (_mission?.Abilities == null || _mission.CommandPoints == null || _statusLabel == null) return;

        foreach (var entry in Entries)
        {
            var button = _buttons[entry.Type];
            float cooldown = _mission.Abilities.CooldownRemaining(entry.Type);
            int cost = _mission.Abilities.CpCost(entry.Type);
            bool affordable = _mission.CommandPoints.Balance >= cost;
            string state = cooldown > 0f ? $"{cooldown:0.0}s" : affordable ? "READY" : "LOW CP";
            button.Text = $"[{HotkeyNumber(entry.Hotkey)}] {entry.Name}\n{cost} CP  {state}";
            button.Disabled = cooldown > 0f;
            button.Modulate = _selectedAbility == entry.Type
                ? new Color(1f, 0.9f, 0.45f)
                : Colors.White;
        }

        string status = _lastStatus ?? (_selectedAbility == null
            ? "Keys 1–3 select an ability; click the battlefield to target it."
            : $"Targeting {DisplayName(_selectedAbility.Value)} — click a point or press the button again to cancel.");
        if (status != _statusLabel.Text) _statusLabel.Text = status;
    }

    private static string DisplayName(AbilityType type) => type switch
    {
        AbilityType.ArtilleryStrike => "Artillery Strike",
        AbilityType.Rally => "Rally",
        AbilityType.EmergencyRepair => "Emergency Repair",
        _ => type.ToString(),
    };

    private static string HotkeyNumber(Key key) => key switch
    {
        Key.Key1 => "1",
        Key.Key2 => "2",
        Key.Key3 => "3",
        _ => "?",
    };

    private static StyleBoxFlat BuildPanelStyle() => new()
    {
        BgColor = new Color(0.06f, 0.08f, 0.1f, 0.92f),
        BorderColor = new Color(0.65f, 0.52f, 0.28f, 0.95f),
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2,
        CornerRadiusTopLeft = 5,
        CornerRadiusTopRight = 5,
        CornerRadiusBottomRight = 5,
        CornerRadiusBottomLeft = 5,
        ContentMarginLeft = 8,
        ContentMarginTop = 6,
        ContentMarginRight = 8,
        ContentMarginBottom = 5,
    };
}
