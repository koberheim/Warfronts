#if DEBUG
using System;
using FrontsOfWar.Editor.Editing;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Documents;

public enum UnsavedChangesChoice { Save, Discard, Cancel }

// Editor document state is deliberately separate from MapDefinition so dirty
// state, file paths, and UI prompts never leak into canonical map resources.
public sealed class MapDocument
{
    public MapDefinition Current { get; private set; }
    public string FilePath { get; private set; } = "";
    public bool IsOpen => Current != null;
    public bool IsDirty { get; private set; }
    public event Action StateChanged;

    public bool TryNew(
        MapDefinition map,
        Func<UnsavedChangesChoice> confirmUnsaved = null,
        Func<bool> saveCurrent = null)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (!CanReplace(confirmUnsaved, saveCurrent)) return false;
        SetCurrent(map, "", dirty: true);
        return true;
    }

    public bool TryOpen(
        string path,
        Func<UnsavedChangesChoice> confirmUnsaved = null,
        Func<bool> saveCurrent = null)
    {
        // Load first so an unreadable replacement can never discard a valid
        // dirty document, even when the user already chose Discard.
        MapDefinition loaded = MapSerializer.Load(path);
        if (!CanReplace(confirmUnsaved, saveCurrent)) return false;
        SetCurrent(loaded, path, dirty: false);
        return true;
    }

    public void MarkDirty()
    {
        if (!IsOpen || IsDirty) return;
        IsDirty = true;
        StateChanged?.Invoke();
    }

    public void Save()
    {
        if (!IsOpen) throw new InvalidOperationException("No map document is open.");
        if (string.IsNullOrEmpty(FilePath)) throw new InvalidOperationException("Untitled map requires Save As.");
        MapSerializer.SaveAs(Current, FilePath);
        IsDirty = false;
        StateChanged?.Invoke();
    }

    public void SaveAs(string path)
    {
        if (!IsOpen) throw new InvalidOperationException("No map document is open.");
        MapSerializer.SaveAs(Current, path);
        FilePath = path;
        IsDirty = false;
        StateChanged?.Invoke();
    }

    public bool TryClose(
        Func<UnsavedChangesChoice> confirmUnsaved = null,
        Func<bool> saveCurrent = null)
    {
        if (!IsOpen) return true;
        if (!CanReplace(confirmUnsaved, saveCurrent)) return false;
        Current = null;
        FilePath = "";
        IsDirty = false;
        StateChanged?.Invoke();
        return true;
    }

    private bool CanReplace(Func<UnsavedChangesChoice> confirmUnsaved, Func<bool> saveCurrent)
    {
        if (!IsDirty) return true;
        if (confirmUnsaved == null) return false;

        return confirmUnsaved() switch
        {
            UnsavedChangesChoice.Discard => true,
            UnsavedChangesChoice.Save => saveCurrent != null && saveCurrent() && !IsDirty,
            _ => false,
        };
    }

    private readonly CommandHistory _history = new();
    public bool CanUndo => _history.CanUndo;
    public bool CanRedo => _history.CanRedo;

    public void Apply(IMapEditCommand command)
    {
        if (!IsOpen) throw new InvalidOperationException("No map document is open.");
        if (command == null) throw new ArgumentNullException(nameof(command));
        _history.Execute(Current, command);
        IsDirty = true;
        StateChanged?.Invoke();
    }

    public bool Undo()
    {
        if (!IsOpen || !_history.Undo(Current)) return false;
        IsDirty = true;
        StateChanged?.Invoke();
        return true;
    }

    public bool Redo()
    {
        if (!IsOpen || !_history.Redo(Current)) return false;
        IsDirty = true;
        StateChanged?.Invoke();
        return true;
    }

    private void SetCurrent(MapDefinition map, string path, bool dirty)
    {
        _history.Clear();
        Current = map;
        FilePath = path;
        IsDirty = dirty;
        StateChanged?.Invoke();
    }
}
#endif
