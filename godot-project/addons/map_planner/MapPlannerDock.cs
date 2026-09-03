using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using FrontsOfWar.Map.Planning;

[Tool]
public partial class MapPlannerDock : VBoxContainer
{
    private MapLayoutCatalog _catalog;
    private MapLayoutTemplate _template;
    private MapPlanDefinition _plan;
    private MapPlannerCanvas _canvas;
    private ItemList _templates;
    private ItemList _candidates;
    private Label _score;
    private Label _diagnostics;
    private SpinBox _seed;
    private OptionButton _overlay;
    private List<MapPlanDefinition> _candidatePlans = new();

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(980f, 620f);
        _catalog = MapLayoutCatalog.LoadFromProject();
        BuildUi();
        PopulateTemplates();
        SelectTemplate(0);
    }

    private void BuildUi()
    {
        var workspace = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        _templates = new ItemList { CustomMinimumSize = new Vector2(215f, 0) };
        _templates.ItemSelected += SelectTemplate;
        workspace.AddChild(_templates);
        _canvas = new MapPlannerCanvas { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        workspace.AddChild(_canvas);

        var inspector = new VBoxContainer { CustomMinimumSize = new Vector2(255f, 0) };
        _score = new Label { Text = "Score: -" };
        _diagnostics = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsVertical = SizeFlags.ExpandFill };
        _overlay = new OptionButton();
        foreach (var name in new[] { "Route Exposure", "Shared Coverage", "Pad Score", "Air Coverage", "Gameplay Clear Zones", "Art Density Zones" }) _overlay.AddItem(name);
        _overlay.ItemSelected += index => { _canvas.Overlay = _overlay.GetItemText((int)index); _canvas.QueueRedraw(); };
        inspector.AddChild(new Label { Text = "Score / diagnostics" });
        inspector.AddChild(_score);
        inspector.AddChild(_overlay);
        inspector.AddChild(_diagnostics);
        workspace.AddChild(inspector);
        AddChild(workspace);

        var footer = new HBoxContainer();
        _seed = new SpinBox { MinValue = 1, MaxValue = 999999999, Value = 1001, Step = 1, CustomMinimumSize = new Vector2(110f, 0) };
        footer.AddChild(new Label { Text = "Seed" }); footer.AddChild(_seed);
        AddButton(footer, "Generate 12", GenerateCandidates);
        AddButton(footer, "New Manual Plan", NewManualPlan);
        AddButton(footer, "Save Draft", SaveDraft);
        AddButton(footer, "Load Saved Plan", LoadSavedPlan);
        AddButton(footer, "Accept + Export", AcceptAndExport);
        _candidates = new ItemList { CustomMinimumSize = new Vector2(0f, 105f) };
        _candidates.ItemSelected += SelectCandidate;
        AddChild(_candidates);
        AddChild(footer);
    }

    private void PopulateTemplates()
    {
        _templates.Clear();
        foreach (var template in _catalog.Filter()) _templates.AddItem($"{template.Id}  {template.Family}  {template.TemplateName}");
    }

    private void SelectTemplate(long index)
    {
        if (index < 0 || index >= _catalog.Records.Count) return;
        _template = _catalog.Records[(int)index];
        _diagnostics.Text = $"{_template.Family}: {_template.Planner.PrimaryDesignLesson}\nPads: {string.Join("-", _template.Planner.RecommendedPadCount)}";
    }

    private void GenerateCandidates()
    {
        if (_template == null) return;
        _candidatePlans = MapCandidateGenerator.Generate(_template, (ulong)_seed.Value, 12);
        _candidates.Clear();
        foreach (var candidate in _candidatePlans) _candidates.AddItem($"{candidate.Id}  {candidate.Metrics.Score:0.0}");
        SelectCandidate(0);
    }

    private void SelectCandidate(long index)
    {
        if (index < 0 || index >= _candidatePlans.Count) return;
        SetPlan(_candidatePlans[(int)index]);
    }

    private void NewManualPlan()
    {
        _plan = new MapPlanDefinition
        {
            Id = "manual_plan",
            DisplayName = "Manual Map Plan",
            Entries = new() { new PlanPoint(5f, 28f) },
            Objective = new PlanPoint(94f, 28f),
            Paths = new() { new PathPlan { Points = new() { new PlanPoint(5f, 28f), new PlanPoint(94f, 28f) } } },
        };
        SetPlan(_plan);
    }

    private void SetPlan(MapPlanDefinition plan)
    {
        _plan = plan;
        var report = MapPlanScorer.Score(_plan, _template);
        _score.Text = $"Score: {report.Total:0.0}/100  Status: {_plan.Status}";
        _diagnostics.Text = report.Diagnostics.Count == 0 ? "No diagnostics." : string.Join("\n", report.Diagnostics);
        _canvas.Plan = _plan;
        _canvas.QueueRedraw();
    }

    private void SaveDraft()
    {
        if (_plan == null) return;
        _plan.Status = MapPlanStatus.Draft;
        SavePlan(_plan, "drafts");
    }

    private void AcceptAndExport()
    {
        if (_plan == null || !_plan.Validation.IsValid) return;
        _plan.Status = MapPlanStatus.Accepted;
        SavePlan(_plan, "maps/plans");
        SetPlan(_plan);
    }

    private void LoadSavedPlan()
    {
        foreach (var subfolder in new[] { "drafts", "maps/plans" })
        {
            string folder = ProjectSettings.GlobalizePath($"res://assets/data/{subfolder}");
            if (!Directory.Exists(folder)) continue;
            var files = Directory.GetFiles(folder, "*.json");
            if (files.Length == 0) continue;
            SetPlan(MapPlanSerializer.LoadFile(files[0]));
            return;
        }
        _diagnostics.Text = "No saved draft or accepted plan found.";
    }

    private static void SavePlan(MapPlanDefinition plan, string subfolder)
    {
        string folder = ProjectSettings.GlobalizePath($"res://assets/data/{subfolder}");
        DirAccess.MakeDirRecursiveAbsolute(folder);
        MapPlanSerializer.SaveFile(plan, Path.Combine(folder, $"{plan.Id}.json"));
    }

    private static void AddButton(HBoxContainer parent, string text, Action action)
    {
        var button = new Button { Text = text };
        button.Pressed += action;
        parent.AddChild(button);
    }
}
