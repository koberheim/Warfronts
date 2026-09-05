using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Godot;
using FrontsOfWar.Meta;
using GFileAccess = Godot.FileAccess;

namespace FrontsOfWar.Core;

// GDD §12.8 — "Save format: JSON, versioned, with a migration hook from day
// one." Godot's user:// filesystem is used for the real path, but the
// directory/file name are overridable so tests can point at a scratch
// location under user:// without ever touching a developer's real save
// (§19 prompt 41's accept criterion: "a v1 save loads in a v2 build").
public static class SaveSystem
{
    private sealed class FutureSaveVersionException : IOException
    {
        public FutureSaveVersionException(int foundVersion)
            : base($"save schema {foundVersion} is newer than supported schema {CurrentSchemaVersion}") { }
    }

    public const int CurrentSchemaVersion = 2;

    public static string SaveDirectory = "user://saves";
    public static string SaveFileName = "profile.json";

    // A newer build's profile must never be replaced by this older build.
    // The block applies only to the exact save path that was rejected, so
    // scratch saves and a later successful load remain independent.
    private static string _futureVersionBlockedSavePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private static string SavePath => $"{SaveDirectory.TrimEnd('/')}/{SaveFileName}";
    private static string TempPath => $"{SaveDirectory.TrimEnd('/')}/{SaveFileName}.tmp";
    private static string BackupPath => $"{SaveDirectory.TrimEnd('/')}/{SaveFileName}.bak";

    public static PlayerProfile Load()
    {
        string path = SavePath;
        if (_futureVersionBlockedSavePath == path) _futureVersionBlockedSavePath = null;

        // If a process stopped after moving the previous save aside but before
        // promoting its replacement, recover the known-good backup first.
        if (!GFileAccess.FileExists(path) && GFileAccess.FileExists(BackupPath))
        {
            Error restoreError = DirAccess.RenameAbsolute(ToAbsolute(BackupPath), ToAbsolute(path));
            if (restoreError != Error.Ok)
            {
                GD.PushWarning($"SaveSystem: could not restore interrupted save backup ({restoreError}); reading the backup directly.");
                path = BackupPath;
            }
        }
        if (!GFileAccess.FileExists(path))
            return new PlayerProfile { SchemaVersion = CurrentSchemaVersion };

        string json = GFileAccess.GetFileAsString(path);
        if (TryGetSchemaVersion(json, out int fileVersion) && fileVersion > CurrentSchemaVersion)
        {
            _futureVersionBlockedSavePath = SavePath;
            GD.PushWarning($"SaveSystem: save schema {fileVersion} is newer than this build supports. Preserving the existing save.");
            return new PlayerProfile { SchemaVersion = CurrentSchemaVersion };
        }
        return LoadFromJson(json, path);
    }

    // Exposed separately so tests can migrate a hand-written JSON string
    // in-memory without touching the filesystem at all.
    public static PlayerProfile LoadFromJson(string json, string sourcePathForWarnings = null)
    {
        try
        {
            if (!TryGetSchemaVersion(json, out int fileVersion))
                throw new InvalidDataException("save JSON root must be an object");

            string migratedJson = MigrateJson(json, fileVersion);
            var profile = JsonSerializer.Deserialize<PlayerProfile>(migratedJson, JsonOptions)
                ?? throw new InvalidDataException("save JSON deserialized to null");
            profile.SchemaVersion = CurrentSchemaVersion;
            return profile;
        }
        catch (Exception e)
        {
            bool futureVersion = e is FutureSaveVersionException;
            GD.PushWarning($"SaveSystem: {(futureVersion ? "unsupported future-version" : "corrupt or unmigratable")} save" +
                (sourcePathForWarnings != null ? $" at {sourcePathForWarnings}" : "") +
                $": {e.Message}. Starting a fresh profile.");
            if (sourcePathForWarnings != null && !futureVersion) QuarantineCorruptFile(sourcePathForWarnings);
            return new PlayerProfile { SchemaVersion = CurrentSchemaVersion };
        }
    }

    private static string MigrateJson(string json, int fromVersion)
    {
        if (fromVersion > CurrentSchemaVersion)
            throw new FutureSaveVersionException(fromVersion);

        string current = json;
        for (int version = fromVersion; version < CurrentSchemaVersion; version++)
        {
            current = version switch
            {
                1 => MigrateV1ToV2(current),
                _ => throw new InvalidDataException($"no migration step defined from schema version {version}"),
            };
        }
        return current;
    }

    // v1 had no MasteryXp or TutorialCompleted fields; v2 adds both with
    // defaults (empty XP map, tutorial not completed). Everything else
    // passes through unchanged.
    private static string MigrateV1ToV2(string json)
    {
        using var document = JsonDocument.Parse(json);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            bool wroteMasteryXp = false;
            bool wroteTutorialCompleted = false;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (string.Equals(property.Name, "SchemaVersion", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteNumber("SchemaVersion", 2);
                    continue;
                }
                if (string.Equals(property.Name, "MasteryXp", StringComparison.OrdinalIgnoreCase)) wroteMasteryXp = true;
                if (string.Equals(property.Name, "TutorialCompleted", StringComparison.OrdinalIgnoreCase)) wroteTutorialCompleted = true;
                property.WriteTo(writer);
            }
            if (!wroteMasteryXp)
            {
                writer.WriteStartObject("MasteryXp");
                writer.WriteEndObject();
            }
            if (!wroteTutorialCompleted)
                writer.WriteBoolean("TutorialCompleted", false);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Recoverable replacement: serialize to a temp file, stage the previous
    // save as a backup, then promote the temp file. Load restores that backup
    // if a process stops between the two renames.
    public static void Save(PlayerProfile profile)
    {
        string finalPath = SavePath;
        if (_futureVersionBlockedSavePath == finalPath)
            throw new InvalidOperationException("SaveSystem: refusing to overwrite a save created by a newer build.");

        profile.SchemaVersion = CurrentSchemaVersion;
        string json = JsonSerializer.Serialize(profile, JsonOptions);

        string absoluteDirectory = ToAbsolute(SaveDirectory);
        if (!DirAccess.DirExistsAbsolute(absoluteDirectory))
        {
            Error directoryError = DirAccess.MakeDirRecursiveAbsolute(absoluteDirectory);
            if (directoryError != Error.Ok)
                throw new IOException($"SaveSystem: could not create save directory '{SaveDirectory}' ({directoryError})");
        }

        string tempPath = TempPath;
        using (var file = GFileAccess.Open(tempPath, GFileAccess.ModeFlags.Write))
        {
            if (file == null)
                throw new IOException($"SaveSystem: could not open {tempPath} for writing ({GFileAccess.GetOpenError()})");
            file.StoreString(json);
            file.Flush();
        }

        bool previousSaveStaged = false;
        if (GFileAccess.FileExists(finalPath))
        {
            RemoveFileIfPresent(BackupPath, "stale backup");
            Error stageError = DirAccess.RenameAbsolute(ToAbsolute(finalPath), ToAbsolute(BackupPath));
            if (stageError != Error.Ok)
                throw new IOException($"SaveSystem: could not stage the existing save for replacement ({stageError})");
            previousSaveStaged = true;
        }

        Error renameError = DirAccess.RenameAbsolute(ToAbsolute(tempPath), ToAbsolute(finalPath));
        if (renameError != Error.Ok)
        {
            if (previousSaveStaged && !GFileAccess.FileExists(finalPath) && GFileAccess.FileExists(BackupPath))
            {
                Error restoreError = DirAccess.RenameAbsolute(ToAbsolute(BackupPath), ToAbsolute(finalPath));
                if (restoreError != Error.Ok)
                    throw new IOException($"SaveSystem: could not restore the previous save after replacement failed ({restoreError})");
            }
            throw new IOException($"SaveSystem: could not rename {tempPath} to {finalPath} ({renameError})");
        }

        if (previousSaveStaged && GFileAccess.FileExists(BackupPath))
            RemoveFileIfPresent(BackupPath, "replacement backup");
    }

    private static bool TryGetSchemaVersion(string json, out int version)
    {
        version = 1;
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!string.Equals(property.Name, "SchemaVersion", StringComparison.OrdinalIgnoreCase)) continue;
                if (property.Value.TryGetInt32(out int parsed)) version = parsed;
                break;
            }
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void RemoveFileIfPresent(string path, string purpose)
    {
        if (!GFileAccess.FileExists(path)) return;
        Error removeError = DirAccess.RemoveAbsolute(ToAbsolute(path));
        if (removeError != Error.Ok)
            throw new IOException($"SaveSystem: could not remove {purpose} '{path}' ({removeError})");
    }

    private static string ToAbsolute(string path) => ProjectSettings.GlobalizePath(path);

    private static void QuarantineCorruptFile(string path)
    {
        try
        {
            string corruptPath = path + ".corrupt";
            RemoveFileIfPresent(corruptPath, "older corrupt save");
            Error quarantineError = DirAccess.RenameAbsolute(ToAbsolute(path), ToAbsolute(corruptPath));
            if (quarantineError != Error.Ok)
                throw new IOException($"could not quarantine corrupt save '{path}' ({quarantineError})");
        }
        catch (Exception e)
        {
            GD.PushWarning($"SaveSystem: could not move aside corrupt save {path}: {e.Message}");
        }
    }
}
