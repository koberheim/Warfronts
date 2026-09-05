#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.Editor.Editing;

public enum MapEditorTool { Select, Move, Rotate, Scale }

public sealed class SelectionService
{
    private readonly HashSet<string> _selected = new(StringComparer.Ordinal);
    public IReadOnlyCollection<string> SelectedIds => _selected;
    public string PrimaryId { get; private set; } = "";
    public MapEditorTool Tool { get; set; } = MapEditorTool.Select;
    public event Action Changed;

    public bool Contains(string id) => _selected.Contains(id);

    public void Set(string id, bool additive = false)
    {
        if (string.IsNullOrEmpty(id)) { Clear(); return; }
        if (!additive) _selected.Clear();
        _selected.Add(id);
        PrimaryId = id;
        Changed?.Invoke();
    }

    public void Toggle(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!_selected.Add(id))
        {
            _selected.Remove(id);
            PrimaryId = _selected.FirstOrDefault() ?? "";
        }
        else PrimaryId = id;
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (_selected.Count == 0 && string.IsNullOrEmpty(PrimaryId)) return;
        _selected.Clear();
        PrimaryId = "";
        Changed?.Invoke();
    }
}
#endif
