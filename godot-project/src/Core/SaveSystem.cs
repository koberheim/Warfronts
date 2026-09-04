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
    public const int CurrentSchemaVersion = 2;

    public static string SaveDirectory = "user://saves";
    public static string SaveFileName = "profile.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private static string SavePath => $"{SaveDirectory.TrimEnd('/')}/{SaveFileName}";
    private static string TempPath => $"{SaveDirectory.TrimEnd('/')}/{SaveFileName}.tmp";

    public static PlayerProfile Load()
    {
        string path = SavePath;
        if (!GFileAccess.FileExists(path))
            return new PlayerProfile { SchemaVersion = CurrentSchemaVersion };

        string json = GFileAccess.GetFileAsString(path);
        return LoadFromJson(json, path);
    }

    // Exposed separately so tests can migrate a hand-written JSON string
    // in-memory without touching the filesystem at all.
    public static PlayerProfile LoadFromJson(string json, string sourcePathForWarnings = null)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            int fileVersion = document.RootElement.TryGetProperty("SchemaVersion", out var versionProp)
                && versionProp.TryGetInt32(out int v) ? v : 1;

            string migratedJson = MigrateJson(json, fileVersion);
            var profile = JsonSerializer.Deserialize<PlayerProfile>(migratedJson, JsonOptions)
                ?? throw new InvalidDataException("save JSON deserialized to null");
            profile.SchemaVersion = CurrentSchemaVersion;
            return profile;
        }
        catch (Exception e)
        {
            GD.PushWarning($"SaveSystem: corrupt or unmigratable save" +
                (sourcePathForWarnings != null ? $" at {sourcePathForWarnings}" : "") +
                $": {e.Message}. Starting a fresh profile.");
            if (sourcePathForWarnings != null) QuarantineCorruptFile(sourcePathForWarnings);
            return new PlayerProfile { SchemaVersion = CurrentSchemaVersion };
        }
    }

    private static string MigrateJson(string json, int fromVersion)
    {
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
                if (property.NameEquals("SchemaVersion"))
                {
                    writer.WriteNumber("SchemaVersion", 2);
                    continue;
                }
                if (property.NameEquals("MasteryXp")) wroteMasteryXp = true;
                if (property.NameEquals("TutorialCompleted")) wroteTutorialCompleted = true;
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

    // Atomic-ish write: serialize to a temp file, then rename it over the
    // real path, so a crash mid-write never leaves a half-written save.
    public static void Save(PlayerProfile profile)
    {
        profile.SchemaVersion = CurrentSchemaVersion;
        string json = JsonSerializer.Serialize(profile, JsonOptions);

        if (!DirAccess.DirExistsAbsolute(SaveDirectory))
            DirAccess.MakeDirRecursiveAbsolute(SaveDirectory);

        string tempPath = TempPath;
        string finalPath = SavePath;
        using (var file = GFileAccess.Open(tempPath, GFileAccess.ModeFlags.Write))
        {
            if (file == null)
                throw new IOException($"SaveSystem: could not open {tempPath} for writing ({GFileAccess.GetOpenError()})");
            file.StoreString(json);
        }

        if (GFileAccess.FileExists(finalPath))
            DirAccess.RemoveAbsolute(finalPath);

        var renameError = DirAccess.RenameAbsolute(tempPath, finalPath);
        if (renameError != Error.Ok)
            throw new IOException($"SaveSystem: could not rename {tempPath} to {finalPath} ({renameError})");
    }

    private static void QuarantineCorruptFile(string path)
    {
        try
        {
            string corruptPath = path + ".corrupt";
            if (GFileAccess.FileExists(corruptPath)) DirAccess.RemoveAbsolute(corruptPath);
            DirAccess.RenameAbsolute(path, corruptPath);
        }
        catch (Exception e)
        {
            GD.PushWarning($"SaveSystem: could not move aside corrupt save {path}: {e.Message}");
        }
    }
}
