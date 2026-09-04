using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FrontsOfWar.Enemies;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Debug;

// GDD §15.6 item 4 / §19 prompt 45: the Data Validator. Walks res://assets/data
// recursively, loads every .tres, and reports broken references, duplicate
// Ids, out-of-envelope stat leanings, and enemies with no listed counters.
// ValidateProjectData() is the disk-backed entry point used by both the
// EditorPlugin menu command (addons/data_validator) and the headless CLI path
// (Boot.cs `--validate-data`). ValidateResources() is the pure, disk-free
// core so DataValidatorTests can inject synthetic (path, resource) pairs —
// including deliberately broken ones — without writing any files.
// Split across DataValidator.*.cs partial-class files (GDD §15.1 rule 6: no
// gameplay file over ~300 lines).
public static partial class DataValidator
{
    public const string DataRoot = "res://assets/data/";

    public static DataValidationReport ValidateProjectData(string root = DataRoot)
    {
        var report = new DataValidationReport();
        var loaded = LoadAll(root, report);
        report.ResourcesChecked = loaded.Count;
        ValidateResources(loaded, report);
        ValidateArtCatalog(report);
        return report;
    }

    // Loads every .tres under `root`. A resource that fails to load (a
    // broken ExtResource reference, a missing script, etc.) is recorded as
    // an error immediately and excluded from the returned list — this is
    // the direct "catches a deliberately broken reference" acceptance path.
    public static List<(string Path, Resource Resource)> LoadAll(string root, DataValidationReport report)
    {
        var loaded = new List<(string Path, Resource Resource)>();
        foreach (var path in CollectTresPaths(root))
        {
            var resource = ResourceLoader.Load(path, "", ResourceLoader.CacheMode.Ignore);
            if (resource == null)
            {
                report.AddError(path, "Failed to load resource (broken reference or missing dependency).");
                continue;
            }
            loaded.Add((path, resource));
        }
        return loaded;
    }

    public static void ValidateResources(IReadOnlyList<(string Path, Resource Resource)> resources, DataValidationReport report)
    {
        ValidateDuplicateIds(resources, report);

        var towers = resources.Where(r => r.Resource is TowerDefinition)
            .Select(r => (r.Path, Tower: (TowerDefinition)r.Resource)).ToList();
        var enemies = resources.Where(r => r.Resource is EnemyDefinition)
            .Select(r => (r.Path, Enemy: (EnemyDefinition)r.Resource)).ToList();
        var nations = resources.Where(r => r.Resource is NationProfile)
            .Select(r => (r.Path, Nation: (NationProfile)r.Resource)).ToList();
        var signatures = resources.Where(r => r.Resource is SignatureDefinition)
            .Select(r => (r.Path, Sig: (SignatureDefinition)r.Resource)).ToList();
        var arsenals = resources.Where(r => r.Resource is ArsenalDefinition)
            .Select(r => (ArsenalDefinition)r.Resource).ToList();

        foreach (var (path, tower) in towers) ValidateTower(path, tower, report);
        foreach (var (path, enemy) in enemies) ValidateEnemy(path, enemy, report);
        foreach (var (path, sig) in signatures) ValidateSignature(path, sig, report);

        foreach (var (path, resource) in resources)
        {
            switch (resource)
            {
                case WaveSequence sequence: ValidateWaveSequence(path, sequence, report); break;
                case WaveDefinition wave: ValidateWaveGroups(path, wave.WaveNumber, wave.Groups, report); break;
                case SpawnGroup group: ValidateSpawnGroup(path, group, report); break;
            }
        }

        // A NationProfile's SignatureId may point at either a signature
        // tower (SignatureDefinition) or a friendly-unit factory
        // (ArsenalDefinition, e.g. the US Arsenal of Democracy) — both are
        // valid targets per GDD §8.
        var signatureAndArsenalIds = new HashSet<string>(signatures.Select(s => s.Sig.Id));
        foreach (var arsenal in arsenals)
            if (!string.IsNullOrEmpty(arsenal.Id))
                signatureAndArsenalIds.Add(arsenal.Id);

        if (nations.Count > 0)
            ValidateNations(nations, towers.Select(t => t.Tower).ToList(), signatureAndArsenalIds, report);

        if (enemies.Count > 0)
            ValidateCounters(enemies, towers.Select(t => t.Tower).ToList(), report);
    }

    private static List<string> CollectTresPaths(string root)
    {
        var results = new List<string>();
        CollectTresPathsRecursive(root, results);
        results.Sort(StringComparer.Ordinal);
        return results;
    }

    private static void CollectTresPathsRecursive(string dir, List<string> results)
    {
        using var access = DirAccess.Open(dir);
        if (access == null) return;
        if (access.ListDirBegin() != Error.Ok) return;

        string name = access.GetNext();
        while (!string.IsNullOrEmpty(name))
        {
            if (name != "." && name != "..")
            {
                string fullPath = dir.TrimEnd('/') + "/" + name;
                if (access.CurrentIsDir())
                    CollectTresPathsRecursive(fullPath, results);
                else if (name.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
                    results.Add(fullPath);
            }
            name = access.GetNext();
        }
        access.ListDirEnd();
    }

    // Pooled across TowerDefinition, EnemyDefinition, NationProfile,
    // SignatureDefinition, ArsenalDefinition, and FriendlyUnitDefinition —
    // an Id collision between any two of these is a real bug (these are
    // exactly the types looked up by Id elsewhere in the codebase).
    private static void ValidateDuplicateIds(IReadOnlyList<(string Path, Resource Resource)> resources, DataValidationReport report)
    {
        var seen = new Dictionary<string, string>();
        foreach (var (path, resource) in resources)
        {
            string id = resource switch
            {
                TowerDefinition t => t.Id,
                EnemyDefinition e => e.Id,
                NationProfile n => n.Id,
                SignatureDefinition s => s.Id,
                ArsenalDefinition a => a.Id,
                FriendlyUnitDefinition f => f.Id,
                _ => null,
            };
            if (string.IsNullOrEmpty(id)) continue;
            if (seen.TryGetValue(id, out var firstPath))
                report.AddError(path, $"Duplicate Id '{id}' also used by {firstPath}.");
            else
                seen[id] = path;
        }
    }
}
