using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using FrontsOfWar.Enemies;
using FrontsOfWar.Waves;

[Tool]
public partial class WaveEditorDock : VBoxContainer
{
    private WaveSequence _sequence;
    private ItemList _waves;
    private GraphEdit _graph;
    private Label _details;
    private Label _diagnostics;
    private int _selectedWave;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(980f, 620f);
        BuildUi();
        CallDeferred(nameof(LoadSequenceAndRefresh));
    }

    private void LoadSequenceAndRefresh()
    {
        try
        {
            _sequence = GD.Load<WaveSequence>("res://assets/data/missions/m2_wave_sequence.tres");
        }
        catch (InvalidCastException)
        {
            // During the editor's initial C# domain scan, script-backed
            // Resources may arrive as generic Resources. Read their exported
            // fields into the same typed model once the dock is available.
            _sequence = LoadGenericSequence();
        }
        if (_sequence == null)
        {
            _diagnostics.Text = "Unable to load the authored wave sequence.";
            return;
        }
        Refresh();
    }

    private static WaveSequence LoadGenericSequence()
    {
        var rawSequence = GD.Load<Resource>("res://assets/data/missions/m2_wave_sequence.tres");
        var sequence = new WaveSequence();
        var waves = new List<WaveDefinition>();
        foreach (var waveValue in rawSequence.Get("Waves").AsGodotArray())
        {
            var rawWave = waveValue.AsGodotObject() as Resource;
            var wave = new WaveDefinition
            {
                WaveNumber = rawWave.Get("WaveNumber").AsInt32(),
                BuildTimeSeconds = rawWave.Get("BuildTimeSeconds").AsSingle(),
                IsBossWave = rawWave.Get("IsBossWave").AsBool(),
                IsAirWave = rawWave.Get("IsAirWave").AsBool(),
                EarlyCallBonusMultiplier = rawWave.Get("EarlyCallBonusMultiplier").AsSingle(),
            };
            var groups = new List<SpawnGroup>();
            foreach (var groupValue in rawWave.Get("Groups").AsGodotArray())
            {
                var rawGroup = groupValue.AsGodotObject() as Resource;
                groups.Add(new SpawnGroup
                {
                    Enemy = ReadEnemy(rawGroup.Get("Enemy").AsGodotObject() as Resource),
                    Count = rawGroup.Get("Count").AsInt32(),
                    StartDelaySeconds = rawGroup.Get("StartDelaySeconds").AsSingle(),
                    IntervalSeconds = rawGroup.Get("IntervalSeconds").AsSingle(),
                    SpawnPointId = rawGroup.Get("SpawnPointId").AsString(),
                    PathId = rawGroup.Get("PathId").AsString(),
                    EliteFlag = rawGroup.Get("EliteFlag").AsBool(),
                    HpMultiplierOverride = rawGroup.Get("HpMultiplierOverride").AsSingle(),
                });
            }
            wave.Groups = groups.ToArray();
            waves.Add(wave);
        }
        sequence.Waves = waves.ToArray();
        return sequence;
    }

    private static EnemyDefinition ReadEnemy(Resource raw)
    {
        if (raw == null) return null;
        return new EnemyDefinition
        {
            Id = raw.Get("Id").AsString(),
            Archetype = (EnemyArchetype)raw.Get("Archetype").AsInt32(),
            ArmorClass = (FrontsOfWar.Combat.ArmorClass)raw.Get("ArmorClass").AsInt32(),
            IsAir = raw.Get("IsAir").AsBool(),
            BaseHp = raw.Get("BaseHp").AsSingle(),
            MoveSpeedTilesPerSec = raw.Get("MoveSpeedTilesPerSec").AsSingle(),
            LeakCost = raw.Get("LeakCost").AsInt32(),
            SpecialAbilityId = raw.Get("SpecialAbilityId").AsString(),
        };
    }

    private void BuildUi()
    {
        var workspace = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        _waves = new ItemList { CustomMinimumSize = new Vector2(130f, 0) };
        _waves.ItemSelected += SelectWave;
        workspace.AddChild(_waves);

        _graph = new GraphEdit { CustomMinimumSize = new Vector2(520f, 0), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _graph.ShowMenu = false;
        workspace.AddChild(_graph);

        var inspector = new VBoxContainer { CustomMinimumSize = new Vector2(290f, 0) };
        inspector.AddChild(new Label { Text = "Wave diagnostics" });
        _details = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _diagnostics = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsVertical = SizeFlags.ExpandFill };
        inspector.AddChild(_details);
        inspector.AddChild(_diagnostics);
        workspace.AddChild(inspector);
        AddChild(workspace);

        var footer = new HBoxContainer();
        AddButton(footer, "Reload", Refresh);
        AddButton(footer, "Export JSON", ExportJson);
        AddButton(footer, "Playtest from Wave", PlaytestFromWave);
        AddChild(footer);
    }

    private void Refresh()
    {
        if (_sequence == null) return;
        _waves.Clear();
        foreach (var wave in _sequence.Waves)
            _waves.AddItem($"Wave {wave.WaveNumber}  {ThreatValueCalculator.Calculate(wave):0.0} TV");
        SelectWave(Mathf.Clamp(_selectedWave, 0, Mathf.Max(0, _sequence.Waves.Length - 1)));
    }

    private void SelectWave(long index)
    {
        _selectedWave = (int)index;
        if (_sequence?.Waves == null || _sequence.Waves.Length == 0) return;
        if (_selectedWave < 0 || _selectedWave >= _sequence.Waves.Length) return;
        _waves.Select(_selectedWave);
        BuildGraph();
        var wave = _sequence.Waves[_selectedWave];
        var report = WavePacingAnalyzer.Analyze(wave,
            _selectedWave > 0 ? _sequence.Waves[_selectedWave - 1] : null, _selectedWave);
        _details.Text = $"Wave {wave.WaveNumber}\nThreat Value: {report.ThreatValue:0.0}\nEstimated duration: {report.EstimatedSeconds:0.0}s\nGroups: {wave.Groups.Length}";
        _diagnostics.Text = report.Warnings.Count == 0 ? "No pacing warnings." : string.Join("\n", report.Warnings);
    }

    private void BuildGraph()
    {
        foreach (var child in _graph.GetChildren())
            if (child is GraphNode) child.QueueFree();
        var wave = _sequence.Waves[_selectedWave];

        // The top row is the mission Threat Value curve; the selected wave's
        // spawn groups are shown below it as timeline blocks.
        for (int i = 0; i < _sequence.Waves.Length; i++)
        {
            var curveNode = new GraphNode
            {
                Name = $"ThreatWave{i + 1}",
                Title = $"W{_sequence.Waves[i].WaveNumber}  TV {ThreatValueCalculator.Calculate(_sequence.Waves[i]):0.0}",
                PositionOffset = new Vector2(i * 120f, 0f),
                CustomMinimumSize = new Vector2(110f, 58f),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            _graph.AddChild(curveNode);
        }

        for (int i = 0; i < wave.Groups.Length; i++)
        {
            var group = wave.Groups[i];
            var node = new GraphNode
            {
                Name = $"Group{i + 1}",
                Title = group.Enemy?.Id ?? "Missing enemy",
                PositionOffset = new Vector2(group.StartDelaySeconds * 8f, 110f + i * 110f),
                CustomMinimumSize = new Vector2(185f, 82f),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            node.AddChild(new Label { Text = $"Count: {group.Count}\nInterval: {group.IntervalSeconds:0.0}s" });
            _graph.AddChild(node);
        }
    }

    private void ExportJson()
    {
        if (_sequence == null) return;
        var snapshots = new List<object>();
        foreach (var wave in _sequence.Waves)
        {
            snapshots.Add(new
            {
                wave = wave.WaveNumber,
                threat_value = ThreatValueCalculator.Calculate(wave),
                estimated_seconds = WavePacingAnalyzer.EstimateDuration(wave),
                groups = wave.Groups.Select(group => new { enemy = group.Enemy?.Id, count = group.Count, start = group.StartDelaySeconds, interval = group.IntervalSeconds }).ToArray(),
            });
        }
        string path = ProjectSettings.GlobalizePath("res://assets/data/missions/m2_wave_sequence.editor.json");
        File.WriteAllText(path, JsonSerializer.Serialize(snapshots, new JsonSerializerOptions { WriteIndented = true }));
        _diagnostics.Text = $"Exported {path}";
    }

    private void PlaytestFromWave()
    {
        if (_sequence?.Waves == null || _sequence.Waves.Length == 0) return;
        string projectPath = ProjectSettings.GlobalizePath("res://");
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = OS.GetExecutablePath(),
            Arguments = $"--path \"{projectPath}\" --wave {_sequence.Waves[_selectedWave].WaveNumber}",
            UseShellExecute = false,
        };
        System.Diagnostics.Process.Start(startInfo);
        _diagnostics.Text = $"Started mission playtest from wave {_sequence.Waves[_selectedWave].WaveNumber}.";
    }

    private static void AddButton(HBoxContainer parent, string text, Action action)
    {
        var button = new Button { Text = text };
        button.Pressed += action;
        parent.AddChild(button);
    }
}
