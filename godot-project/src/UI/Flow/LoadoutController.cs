using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Doctrines;

namespace FrontsOfWar.UI.Flow;

// Loadout screen (GDD §13.3, §19 prompt 39). Doctrine selection is United
// States only today, since no other nation is selectable yet — see
// MapRuntime.LoadDoctrine's own note.
public partial class LoadoutController : Node2D
{
    private static readonly string[] DoctrineIds = { "lend_lease", "airborne", "combined_arms" };

    private Label _doctrineLabel;

    public override void _Ready()
    {
        var box = new VBoxContainer { Position = new Vector2(130, 80), Size = new Vector2(760, 470) };
        AddChild(box);
        box.AddChild(new Label { Text = "LOADOUT  /  UNITED STATES" });
        box.AddChild(new Label
        {
            Text = "Recommended Mission 1 loadout\n\n[Q] Automatic Gun     [W] Field Mortar     [E] Anti-Tank Gun\n" +
                   "[R] Command Post       [T] Marksman Post      [Y] Flak Battery\n\nDifficulty: Regular\n\n" +
                   "Build these six towers on open build pads from the in-mission build bar.\n" +
                   "The Arsenal of Democracy signature factory is pre-placed on the map.",
        });

        box.AddChild(new Label { Text = "\nDoctrine:" });
        var doctrineRow = new HBoxContainer();
        box.AddChild(doctrineRow);
        foreach (var id in DoctrineIds)
        {
            var doctrine = GD.Load<DoctrineDefinition>($"res://assets/data/doctrines/united_states_{id}.tres");
            var doctrineButton = new Button { Text = doctrine?.DisplayName ?? id, CustomMinimumSize = new Vector2(220, 40) };
            doctrineButton.Pressed += () => SelectDoctrine(id, doctrine);
            doctrineRow.AddChild(doctrineButton);
        }

        _doctrineLabel = new Label { CustomMinimumSize = new Vector2(740, 60) };
        box.AddChild(_doctrineLabel);
        SelectDoctrine(MissionSession.SelectedDoctrineId,
            GD.Load<DoctrineDefinition>($"res://assets/data/doctrines/united_states_{MissionSession.SelectedDoctrineId}.tres"));

        var button = new Button { Text = "Deploy to Bocage Crossroads", CustomMinimumSize = new Vector2(300, 48) };
        button.Pressed += () => GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn");
        box.AddChild(button);
    }

    private void SelectDoctrine(string id, DoctrineDefinition doctrine)
    {
        MissionSession.SelectedDoctrineId = id;
        _doctrineLabel.Text = doctrine == null ? "" :
            $"{doctrine.DisplayName} — Passive: {doctrine.PassiveDescription}\nAbility [{doctrine.AbilityName}]: {doctrine.AbilityDescription}";
    }
}
