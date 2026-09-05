using System;
using System.Collections.Generic;
using System.IO;

namespace FrontsOfWar.Map.Authoring;

public sealed class MapRegistryException : Exception
{
    public MapRegistryException(string message) : base(message) { }
}

// Repository-relative map discovery. The registry reads IDs from resources so
// callers never need to hardcode absolute paths or duplicate map metadata.
public static class MapRegistry
{
    public static string ResolvePath(string mapId, string root = "res://assets/data/maps")
    {
        if (string.IsNullOrWhiteSpace(mapId))
            throw new MapRegistryException("A map ID is required.");

        var matches = new List<string>();
        CollectTresPaths(root, matches);
        matches.Sort(StringComparer.Ordinal);

        var found = new List<string>();
        foreach (string path in matches)
        {
            try
            {
                if (MapSerializer.Load(path).Metadata.Id == mapId) found.Add(path);
            }
            catch (Exception exception) when (exception is MapSerializationException or MapSchemaException or FileNotFoundException) { }
        }

        if (found.Count == 0)
            throw new MapRegistryException($"No map with ID '{mapId}' was found under '{root}'.");
        if (found.Count > 1)
            throw new MapRegistryException($"Map ID '{mapId}' is ambiguous: {string.Join(", ", found)}.");
        return found[0];
    }

    private static void CollectTresPaths(string directory, List<string> results)
    {
        // Exports may remap .tres to binary resources. Ask the resource loader
        // for original names so ID lookup behaves the same in a player PCK.
        if (directory.StartsWith("res://", StringComparison.Ordinal))
        {
            foreach (string resourceName in Godot.ResourceLoader.ListDirectory(directory))
            {
                string path = directory.TrimEnd('/') + "/" + resourceName.TrimEnd('/');
                if (resourceName.EndsWith("/", StringComparison.Ordinal)) CollectTresPaths(path, results);
                else if (resourceName.EndsWith(".tres", StringComparison.OrdinalIgnoreCase)) results.Add(path);
            }
            return;
        }
        using var access = Godot.DirAccess.Open(directory);
        if (access == null || access.ListDirBegin() != Godot.Error.Ok) return;
        string name = access.GetNext();
        while (!string.IsNullOrEmpty(name))
        {
            if (name != "." && name != "..")
            {
                string path = directory.TrimEnd('/') + "/" + name;
                if (access.CurrentIsDir()) CollectTresPaths(path, results);
                else if (name.EndsWith(".tres", StringComparison.OrdinalIgnoreCase)) results.Add(path);
            }
            name = access.GetNext();
        }
        access.ListDirEnd();
    }
}
