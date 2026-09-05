#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FrontsOfWar.Editor.Documents;

public sealed class MapEditorPreferences
{
    public List<string> RecentMaps { get; set; } = new();
    public List<string> RecentAssets { get; set; } = new();
    public bool ListView { get; set; }

    public void RememberMap(string path) => Remember(RecentMaps, path);
    public void RememberAsset(string id) => Remember(RecentAssets, id);

    public void Save(string path = "user://map-editor/preferences.cfg")
    {
        var config = new ConfigFile();
        config.SetValue("editor", "recent_maps", new Godot.Collections.Array<string>(RecentMaps.ToArray()));
        config.SetValue("editor", "recent_assets", new Godot.Collections.Array<string>(RecentAssets.ToArray()));
        config.SetValue("editor", "list_view", ListView);
        config.Save(path);
    }

    public static MapEditorPreferences Load(string path = "user://map-editor/preferences.cfg")
    {
        var preferences = new MapEditorPreferences();
        var config = new ConfigFile();
        if (config.Load(path) != Error.Ok) return preferences;
        preferences.RecentMaps = ToStrings(config.GetValue("editor", "recent_maps", new Godot.Collections.Array<string>()));
        preferences.RecentAssets = ToStrings(config.GetValue("editor", "recent_assets", new Godot.Collections.Array<string>()));
        preferences.ListView = (bool)config.GetValue("editor", "list_view", false);
        return preferences;
    }

    private static void Remember(List<string> values, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        values.RemoveAll(item => string.Equals(item, value, StringComparison.Ordinal));
        values.Insert(0, value);
        if (values.Count > 12) values.RemoveRange(12, values.Count - 12);
    }

    private static List<string> ToStrings(Variant value)
    {
        var result = new List<string>();
        foreach (Variant item in value.AsGodotArray()) result.Add(item.AsString());
        return result;
    }
}
#endif
