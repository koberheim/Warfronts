using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;

namespace FrontsOfWar.Tests;

// GDD §13.8-13.9 / R12 (docs/RELEASE_COMPLETION.md). UserSettings/
// PlayerSettings had no dedicated test coverage before the Settings screen
// was wired up (D75/D80) - these exercise the pure logic that a live screen
// can't easily assert on (SettingsPanel itself needs a rendered UI).
public class SettingsTests : TestClass
{
    public SettingsTests(Node testScene) : base(testScene) { }

    [Test]
    public void FreshProfileDefaultsToFullscreenAndPopulatesEveryBindingDefinition()
    {
        ProfileStore.ResetForTests(new PlayerProfile());
        var settings = UserSettings.Current;

        Require(settings.Fullscreen, "a fresh profile launches fullscreen, matching project.godot's window/size/mode default");
        foreach (var definition in UserSettings.BindingDefinitions)
            Require(settings.InputBindings.ContainsKey(definition.Action), $"{definition.Action} gets a default binding");

        ProfileStore.ResetForTests();
    }

    [Test]
    public void NormalizeClampsOutOfRangeValues()
    {
        var settings = new PlayerSettings { UiScale = 5f, MasterVolume = -3f, MusicVolume = 9f };

        settings.Normalize();

        Require(settings.UiScale == 1.5f, "UI scale clamps to the 150% ceiling");
        Require(settings.MasterVolume == 0f, "volume clamps to 0 at the floor");
        Require(settings.MusicVolume == 1f, "volume clamps to 1 at the ceiling");
    }

    [Test]
    public void RebindingToAnAlreadyUsedInputIsRefusedWithTheConflictingActionLabel()
    {
        ProfileStore.ResetForTests(new PlayerProfile());
        var freeKey = new InputBindingData { Device = "key", Code = (int)Key.F9 };
        Require(UserSettings.TrySetBinding(UserSettings.PauseAction, freeKey, out _), "an unused key binds cleanly");

        bool refused = !UserSettings.TrySetBinding(UserSettings.CancelAction, freeKey, out string conflict);

        Require(refused, "binding Cancel to Pause's newly-assigned key is refused");
        Require(conflict == "Pause", "the refusal names the conflicting action by its label");
        ProfileStore.ResetForTests();
    }

    [Test]
    public void ResetBindingsRestoresEveryDefault()
    {
        ProfileStore.ResetForTests(new PlayerProfile());
        UserSettings.TrySetBinding(UserSettings.PauseAction, new InputBindingData { Device = "key", Code = (int)Key.F9 }, out _);

        UserSettings.ResetBindings();

        Require(UserSettings.BindingLabel(UserSettings.PauseAction) == Key.P.ToString(), "Pause returns to its default key after reset");
        ProfileStore.ResetForTests();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.InvalidOperationException($"Assertion failed: {message}");
    }
}
