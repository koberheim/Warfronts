using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Meta;

// Runtime-facing discovery for authored selection data. It scans only .tres
// resources and filters by their actual Resource type, so a WaveSequence file
// beside a MissionDefinition can never become a campaign mission by accident.
public static class MissionCatalog
{
    public sealed class Entry<T> where T : Resource
    {
        public Entry(string path, T resource) { Path = path; Resource = resource; }
        public string Path { get; }
        public T Resource { get; }
    }

    public static IReadOnlyList<Entry<MissionDefinition>> DiscoverMissions()
        => Discover<MissionDefinition>("res://assets/data/missions")
            .OrderBy(entry => entry.Resource.Act)
            .ThenBy(entry => entry.Resource.Id, StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyList<Entry<NationProfile>> DiscoverNations()
        => Discover<NationProfile>("res://assets/data/nations")
            .OrderBy(entry => entry.Resource.DisplayName, StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyList<Entry<NationProfile>> NationsForAlliance(string alliance)
        => DiscoverNations()
            .Where(entry => string.Equals(entry.Resource.Alliance, alliance, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public static IReadOnlyList<Entry<TowerDefinition>> DiscoverBuildTowers()
        => Discover<TowerDefinition>("res://assets/data/towers")
            .OrderBy(entry => entry.Resource.Id, StringComparer.Ordinal)
            .ToList();

    public static string ResolveSignatureResourcePath(string signatureId)
    {
        if (string.IsNullOrWhiteSpace(signatureId)) return "";
        foreach (string path in CollectTresPaths("res://assets/data/towers"))
        {
            Resource resource = ResourceLoader.Load(path);
            if (resource is not SignatureDefinition && resource is not ArsenalDefinition) continue;
            if (string.Equals(resource.Get("Id").AsString(), signatureId, StringComparison.Ordinal)) return path;
        }
        return "";
    }

    public static bool IsCampaignMissionUnlocked(PlayerProfile profile, Entry<MissionDefinition> mission,
        IReadOnlyList<Entry<MissionDefinition>> orderedMissions = null)
    {
        var ordered = orderedMissions ?? DiscoverMissions();
        int index = ordered.ToList().FindIndex(entry => entry.Path == mission.Path);
        if (index <= 0) return index == 0;
        return profile?.CampaignMissionsCompleted.Contains(ordered[index - 1].Resource.Id) == true;
    }

    private static List<Entry<T>> Discover<T>(string root) where T : Resource
    {
        var entries = new List<Entry<T>>();
        foreach (string path in CollectTresPaths(root))
        {
            if (ResourceLoader.Load(path) is T resource) entries.Add(new Entry<T>(path, resource));
        }
        return entries;
    }

    private static List<string> CollectTresPaths(string directory)
    {
        var paths = new List<string>();
        CollectTresPaths(directory, paths);
        paths.Sort(StringComparer.Ordinal);
        return paths;
    }

    private static void CollectTresPaths(string directory, List<string> paths)
    {
        foreach (string name in ResourceLoader.ListDirectory(directory))
        {
            string path = directory.TrimEnd('/') + "/" + name.TrimEnd('/');
            if (name.EndsWith("/", StringComparison.Ordinal)) CollectTresPaths(path, paths);
            else if (name.EndsWith(".tres", StringComparison.OrdinalIgnoreCase)) paths.Add(path);
        }
    }
}
