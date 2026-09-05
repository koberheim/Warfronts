#if DEBUG
using System;
using System.Collections.Generic;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public interface IMapEditCommand
{
    string Description { get; }
    void Execute(MapDefinition map);
    void Undo(MapDefinition map);
}

public sealed class CommandHistory
{
    private readonly Stack<IMapEditCommand> _undo = new();
    private readonly Stack<IMapEditCommand> _redo = new();
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void Execute(MapDefinition map, IMapEditCommand command)
    {
        command.Execute(map);
        _undo.Push(command);
        _redo.Clear();
    }

    public bool Undo(MapDefinition map)
    {
        if (_undo.Count == 0) return false;
        var command = _undo.Pop(); command.Undo(map); _redo.Push(command); return true;
    }

    public bool Redo(MapDefinition map)
    {
        if (_redo.Count == 0) return false;
        var command = _redo.Pop(); command.Execute(map); _undo.Push(command); return true;
    }

    public void Clear() { _undo.Clear(); _redo.Clear(); }

    public void ExecuteCompound(MapDefinition map, string description, IEnumerable<IMapEditCommand> commands)
    {
        var compound = new CompoundMapEditCommand(description, commands);
        Execute(map, compound);
    }
}

public sealed class CompoundMapEditCommand : IMapEditCommand
{
    private readonly IMapEditCommand[] _commands;
    public CompoundMapEditCommand(string description, IEnumerable<IMapEditCommand> commands)
    {
        Description = description; _commands = commands is IMapEditCommand[] array ? array : new List<IMapEditCommand>(commands).ToArray();
    }
    public string Description { get; }
    public void Execute(MapDefinition map)
    {
        int completed = 0;
        try
        {
            foreach (var command in _commands) { command.Execute(map); completed++; }
        }
        catch
        {
            for (int i = completed - 1; i >= 0; i--) _commands[i].Undo(map);
            throw;
        }
    }
    public void Undo(MapDefinition map) { for (int i = _commands.Length - 1; i >= 0; i--) _commands[i].Undo(map); }
}

public class MapSnapshotCommand : IMapEditCommand
{
    private readonly Action<MapDefinition> _mutation;
    private MapDefinition _before;
    private MapDefinition _after;
    public MapSnapshotCommand(string description, Action<MapDefinition> mutation) { Description = description; _mutation = mutation; }
    public string Description { get; }

    public void Execute(MapDefinition map)
    {
        if (_after != null) { MapResourceCopy.Restore(_after, map); return; }
        _before = MapResourceCopy.Clone(map);
        try
        {
            _mutation(map);
            var validation = MapDefinitionValidator.Validate(map);
            if (!validation.IsValid) throw new InvalidOperationException(string.Join("; ", validation.Errors));
            _after = MapResourceCopy.Clone(map);
        }
        catch
        {
            MapResourceCopy.Restore(_before, map);
            throw;
        }
    }

    public void Undo(MapDefinition map) => MapResourceCopy.Restore(_before, map);
}

internal static class MapResourceCopy
{
    public static MapDefinition Clone(MapDefinition source)
        => source.Duplicate(true) as MapDefinition ?? throw new InvalidOperationException("Could not duplicate map state.");

    public static void Restore(MapDefinition source, MapDefinition target)
    {
        target.SchemaVersion = source.SchemaVersion;
        target.Metadata = source.Metadata;
        target.Terrain = source.Terrain;
        target.Assets = source.Assets;
        target.Clusters = source.Clusters;
        target.Paths = source.Paths;
        target.AirCorridors = source.AirCorridors;
        target.TowerNodes = source.TowerNodes;
        target.Markers = source.Markers;
        target.Zones = source.Zones;
        target.Gimmicks = source.Gimmicks;
        target.Provenance = source.Provenance;
    }
}
#endif
