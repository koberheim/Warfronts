using System;

namespace FrontsOfWar.Map.Authoring;

public enum MapSchemaErrorKind { MissingVersion, FutureVersion, UnsupportedVersion }

public sealed class MapSchemaException : Exception
{
    public MapSchemaErrorKind Kind { get; }
    public int FileVersion { get; }

    public MapSchemaException(MapSchemaErrorKind kind, int fileVersion, string message) : base(message)
    {
        Kind = kind;
        FileVersion = fileVersion;
    }
}

// Migration is explicit and in-memory. Version 1 is the first schema, so no
// transforming step exists yet; this loop is the hook future versions extend.
public static class MapSchemaMigrator
{
    public const int CurrentSchemaVersion = 1;

    public static MapDefinition ValidateAndMigrate(MapDefinition map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (map.SchemaVersion <= 0)
            throw new MapSchemaException(
                MapSchemaErrorKind.MissingVersion,
                map.SchemaVersion,
                "Map resource has no SchemaVersion and cannot be opened safely.");
        if (map.SchemaVersion > CurrentSchemaVersion)
            throw new MapSchemaException(
                MapSchemaErrorKind.FutureVersion,
                map.SchemaVersion,
                $"Map schema {map.SchemaVersion} is newer than supported schema {CurrentSchemaVersion}.");

        while (map.SchemaVersion < CurrentSchemaVersion)
        {
            throw new MapSchemaException(
                MapSchemaErrorKind.UnsupportedVersion,
                map.SchemaVersion,
                $"No migration step is registered from map schema {map.SchemaVersion}.");
        }

        return map;
    }
}
