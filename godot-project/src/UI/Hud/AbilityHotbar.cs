using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Map;
using FrontsOfWar.UI.Theme;
using System.Collections.Generic;

namespace FrontsOfWar.UI.Hud;

// HUD zone G (docs/UI_DESIGN_SPEC.md §8.4; GDD §7.6, §13.4): the ABILITIES
// panel with its status line and the card row. Owns the three universal
// abilities; DoctrineAbilitySlot adds the fourth card into CardRow and
// writes its own messages through ExternalStatus (D26/D51 kept the two
// controls separate so this one's "one AbilityType, one click" flow stays
// simple). Point abilities enter target mode; keys 1-3 select or activate.
public partial class AbilityHotbar : PanelContainer
{
    private readonly record struct AbilityEntry(AbilityType Type, string Name, string IconId, Key Hotkey, string KeyLabel);

    private static readonly AbilityEntry[] Entries =
    {
        new(AbilityType.ArtilleryStrike, "Artillery Strike", "ability_artillery_strike", Key.Key1, "1"),
        new(AbilityType.Rally, "Rally", "ability_rally", Key.Key2, "2"),
        new(AbilityType.EmergencyRepair, "Emergency Repair", "ability_emergency_repair", Key.Key3, "3"),
    };

    private const string DefaultStatus = "Keys 1–4 select an ability; click the battlefield to target it.";

    private MapRuntime _mission;
    private readonly Dictionary<AbilityType, AbilityCard> _cards = new();
    private Label _statusLabel;
    private AbilityType? _selectedAbility;
    private string _lastStatus;

    public MapRuntime Mission { get => _mission; set => _mission = value; }
    public HBoxContainer CardRow { get; private set; }

    // Set by the doctrine slot while it has something to say; null hands
    // the status line back to this control.
    public string ExternalStatus { get; set; }

    public override void _Ready()
    {
        ThemeTypeVariation = "SlatePanel";
        var column = UiFactory.VBox(4);
        AddChild(column);
        column.AddChild(UiFactory.Label("CaptionLabel", "ABILITIES", uppercase: true));

        _statusLabel = UiFactory.Wrapped("SmallLabel", DefaultStatus);
        _statusLabel.CustomMinimumSize = new Vector2(440f, 0f);
        column.AddChild(_statusLabel);

        CardRow = UiFactory.HBox(8);
        column.AddChild(CardRow);

        foreach (var entry in Entries)
        {
            var card = new AbilityCard();
            CardRow.AddChild(card);
            card.Setup(entry.KeyLabel, entry.IconId, entry.Name, _mission?.Abilities?.CpCost(entry.Type) ?? 0);
            var type = entry.Type;
            card.Pressed += () => SelectOrActivate(type);
            _cards.Add(entry.Type, card);
        }

        EventBus.Instance?.Subscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
        Callable.From(Refresh).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<CommandPointsChangedEvent>(OnCommandPointsChanged);
    }

    // Cooldowns are simulation-owned, so the display reads the live values
    // rather than keeping a second timer in the UI.
    public override void _Process(double delta) => Refresh();

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
            if (key.Keycode == Key.Escape && _selectedAbility != null)
            {
                _selectedAbility = null;
                _lastStatus = null;
                GetViewport().SetInputAsHandled();
            }
            return;
        }

        if (_selectedAbility == null || @event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse)
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
            float cooldown = _mission.Abilities.CooldownRemaining(entry.Type);
            bool affordable = _mission.CommandPoints.Balance >= _mission.Abilities.CpCost(entry.Type);
            _cards[entry.Type].SetCpCost(_mission.Abilities.CpCost(entry.Type));
            _cards[entry.Type].SetState(cooldown, _mission.Abilities.CooldownSeconds(entry.Type), affordable,
                _selectedAbility == entry.Type, false);
        }

        string status = ExternalStatus ?? _lastStatus ?? (_selectedAbility == null
            ? DefaultStatus
            : $"Targeting {DisplayName(_selectedAbility.Value)} — click a target, or press the key again to cancel.");
        if (status != _statusLabel.Text) _statusLabel.Text = status;
    }

    private static string DisplayName(AbilityType type) => type switch
    {
        AbilityType.ArtilleryStrike => "Artillery Strike",
        AbilityType.Rally => "Rally",
        AbilityType.EmergencyRepair => "Emergency Repair",
        _ => type.ToString(),
    };
}
