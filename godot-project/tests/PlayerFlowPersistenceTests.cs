using System;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;

namespace FrontsOfWar.Tests;

// Guards the boundary between a finished mission and ResultsController's
// persistent progression write. A scene recreation must not turn one win
// into multiple completions, stars, XP grants, or unlock evaluations.
public class PlayerFlowPersistenceTests : TestClass
{
    public PlayerFlowPersistenceTests(Node testScene) : base(testScene) { }

    [Test]
    public void CompletedResultCanBeClaimedOnlyOnceUntilTheMissionIsReset()
    {
        MissionSession.ResetMission();
        try
        {
            var completed = new MissionStatsSnapshot { Victory = true, WaveReached = 12 };
            MissionSession.StoreCompletedResult(completed);

            Require(MissionSession.TryClaimResultForPersistence(out var first), "first Results entry claims the completed run");
            Require(ReferenceEquals(first, completed), "claim returns the exact completed snapshot");
            Require(!MissionSession.TryClaimResultForPersistence(out _), "re-entering Results cannot claim the same run twice");

            MissionSession.ResetMission();
            Require(!MissionSession.TryClaimResultForPersistence(out _), "reset clears a prior completed run");
        }
        finally
        {
            MissionSession.ResetMission();
        }
    }

    [Test]
    public void TutorialCompletionPersistsToAScratchProfile()
    {
        WithScratchSaveDirectory("tutorial", () =>
        {
            ProfileStore.ResetForTests(new PlayerProfile());
            Require(ProfileStore.TryCompleteTutorial(out string error), $"tutorial completion saves: {error}");

            var loaded = SaveSystem.Load();
            Require(loaded.TutorialCompleted, "tutorial completion survives profile reload");
        });
        ProfileStore.ResetForTests();
    }

    [Test]
    public void FutureVersionSaveIsPreservedAndCannotBeOverwritten()
    {
        WithScratchSaveDirectory("future_version", () =>
        {
            string path = $"{SaveSystem.SaveDirectory}/{SaveSystem.SaveFileName}";
            Require(DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(SaveSystem.SaveDirectory)) == Error.Ok,
                "creates scratch save directory");
            using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Write))
                file.StoreString("{\"SchemaVersion\": 99, \"TutorialCompleted\": true}");

            var loaded = SaveSystem.Load();
            Require(!loaded.TutorialCompleted, "unsupported future profile is rejected instead of interpreted");
            var pendingProfile = new PlayerProfile { TutorialCompleted = true };
            ProfileStore.ResetForTests(pendingProfile);
            Require(!ProfileStore.TrySave(out string saveError), "save failure is returned instead of thrown through player flow");
            Require(!string.IsNullOrEmpty(saveError), "failed save reports an actionable error");
            Require(pendingProfile.TutorialCompleted, "unsaved in-memory profile changes remain available for a later retry");
            Require(FileAccess.GetFileAsString(path).Contains("99"), "future-version bytes remain intact");
        });
        ProfileStore.ResetForTests();
    }

    [Test]
    public void InterruptedReplacementRestoresTheBackupBeforeLoading()
    {
        WithScratchSaveDirectory("backup_recovery", () =>
        {
            var original = new PlayerProfile { TutorialCompleted = true };
            SaveSystem.Save(original);
            string path = $"{SaveSystem.SaveDirectory}/{SaveSystem.SaveFileName}";
            Require(DirAccess.RenameAbsolute(ProjectSettings.GlobalizePath(path), ProjectSettings.GlobalizePath(path + ".bak")) == Error.Ok,
                "stages a known-good backup");

            var recovered = SaveSystem.Load();
            Require(recovered.TutorialCompleted, "backup is recovered instead of starting a fresh profile");
            Require(FileAccess.FileExists(path), "recovered profile is restored to its normal path");
        });
    }

    [Test]
    public void LowercaseSchemaVersionUsesTheSameCaseInsensitiveSaveContract()
    {
        var loaded = SaveSystem.LoadFromJson("{\"schemaVersion\": 2, \"tutorialCompleted\": true}");
        Require(loaded.SchemaVersion == SaveSystem.CurrentSchemaVersion, "lowercase schema version is accepted");
        Require(loaded.TutorialCompleted, "lowercase profile fields deserialize consistently");
    }

    [Test]
    public void NonObjectJsonUsesTheCorruptSaveFallbackWithoutThrowing()
    {
        var arrayProfile = SaveSystem.LoadFromJson("[]");
        var nullProfile = SaveSystem.LoadFromJson("null");

        Require(arrayProfile.SchemaVersion == SaveSystem.CurrentSchemaVersion, "array root falls back to a fresh profile");
        Require(nullProfile.SchemaVersion == SaveSystem.CurrentSchemaVersion, "null root falls back to a fresh profile");
    }

    private static void WithScratchSaveDirectory(string subfolder, Action action)
    {
        string originalDirectory = SaveSystem.SaveDirectory;
        string originalFileName = SaveSystem.SaveFileName;
        try
        {
            SaveSystem.SaveDirectory = $"user://player_flow_persistence_{subfolder}";
            SaveSystem.SaveFileName = "profile.json";
            action();
        }
        finally
        {
            SaveSystem.SaveDirectory = originalDirectory;
            SaveSystem.SaveFileName = originalFileName;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }
}
