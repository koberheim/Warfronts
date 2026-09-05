using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Towers;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Map;

// Reasons a placement attempt can be refused (GDD §7.4, §7.5). Shortfall is
// only meaningful for InsufficientSupply.
public enum TowerPlacementResult
{
    Success,
    PadOccupied,
    NoControllerScene,
    InsufficientSupply,
    ArchetypeNotAllowed,
    RequiresPath,
    FieldLimitReached,
    TooCloseToMinefield,
    SignatureLimitReached,
}

public readonly struct TowerPlacementOutcome
{
    public readonly TowerPlacementResult Result;
    public readonly int SupplyShortfall;
    public readonly Node2D PlacedInstance;

    public TowerPlacementOutcome(TowerPlacementResult result, int supplyShortfall = 0, Node2D placedInstance = null)
    {
        Result = result;
        SupplyShortfall = supplyShortfall;
        PlacedInstance = placedInstance;
    }

    public bool Success => Result == TowerPlacementResult.Success;
}

// Turns a build-bar selection + a BuildPad click into a placed tower (GDD
// §7.4, §7.5, §13.4, §19 prompts 18-19). Plain C# — no Node lifecycle of its
// own — owned and constructed by MapRuntime once its ledgers/managers exist
// (see MapRuntime._Ready). T9 Command Post is the one archetype that
// registers with CommandPostManager instead of TowerManager, mirroring how
// MapRuntime._Ready registers the mission's pre-placed towers/posts.
public class TowerPlacementService
{
    private readonly Node _towerContainer;
    private readonly Node _commandPostContainer;
    private readonly SupplyLedger _supply;
    private readonly TowerManager _towers;
    private readonly CommandPostManager _commandPosts;

    // Remembers which pad a placed instance came from so selling it can free
    // that pad back up — BuildPad itself has no reference to whatever is
    // standing on it (Claude decision: keeping that link here rather than on
    // BuildPad keeps BuildPad a dumb, presentation-only node).
    private readonly Dictionary<Node2D, BuildPad> _padByInstance = new();

    // Set once by MapRuntime after DoctrineSystem exists (GDD §19 prompt 39,
    // e.g. Lend-Lease's "all towers cost −6%"). Null before then, or for any
    // mission with no doctrine loaded — TryPlace treats that as a 1x no-op.
    public Func<TowerDefinition, PadTag, float> DoctrineCostMultiplierProvider { get; set; }

    public TowerPlacementService(Node towerContainer, Node commandPostContainer,
        SupplyLedger supply, TowerManager towers, CommandPostManager commandPosts)
    {
        _towerContainer = towerContainer;
        _commandPostContainer = commandPostContainer;
        _supply = supply;
        _towers = towers;
        _commandPosts = commandPosts;
    }

    public TowerPlacementOutcome TryPlace(TowerDefinition definition, BuildPad pad)
    {
        if (pad == null || definition == null) return new TowerPlacementOutcome(TowerPlacementResult.NoControllerScene);
        if (definition.Archetype == TowerArchetype.Minefield) return new TowerPlacementOutcome(TowerPlacementResult.RequiresPath);
        if (pad.IsOccupied) return new TowerPlacementOutcome(TowerPlacementResult.PadOccupied);
        if (!pad.Allows(definition)) return new TowerPlacementOutcome(TowerPlacementResult.ArchetypeNotAllowed);
        if (definition.ControllerScene == null) return new TowerPlacementOutcome(TowerPlacementResult.NoControllerScene);

        float multiplier = DoctrineCostMultiplierProvider?.Invoke(definition, pad.Tag) ?? 1f;
        int cost = Mathf.RoundToInt(definition.PreForkStatsForLevel(1).Cost * multiplier);
        if (_supply.Balance < cost)
            return new TowerPlacementOutcome(TowerPlacementResult.InsufficientSupply, cost - _supply.Balance);

        _supply.TrySpend(cost);

        Node2D instance = definition.Archetype == TowerArchetype.CommandPost
            ? PlaceCommandPost(definition, pad)
            : PlaceTower(definition, pad);

        pad.SetOccupied(true);
        _padByInstance[instance] = pad;

        EventBus.Instance?.Publish(new TowerPlacedEvent(instance, definition, pad, cost));
        return new TowerPlacementOutcome(TowerPlacementResult.Success, 0, instance);
    }

    // Definition/PadTag must be set before AddChild — TowerController reads
    // Definition in its own _Ready(). Position is written before the node
    // enters the tree (assumes TowerContainer sits at world origin, true of
    // every mission scene today — see scenes_root/mission.tscn), since
    // GlobalPosition needs a live parent transform chain to convert through.
    private TowerController PlaceTower(TowerDefinition definition, BuildPad pad)
    {
        var tower = definition.ControllerScene.Instantiate<TowerController>();
        tower.Definition = definition;
        tower.PadTag = pad.Tag;
        tower.ArcFacingDegrees = pad.ArcFacingDegrees;
        tower.ArcHalfAngleDegrees = pad.ArcHalfAngleDegrees;
        tower.Position = _towerContainer is Node2D parent ? parent.ToLocal(pad.GlobalPosition) : pad.GlobalPosition;
        _towerContainer.AddChild(tower);
        _towers.Register(tower);
        return tower;
    }

    private CommandPostController PlaceCommandPost(TowerDefinition definition, BuildPad pad)
    {
        var post = definition.ControllerScene.Instantiate<CommandPostController>();
        post.Definition = definition;
        post.Position = _commandPostContainer is Node2D parent ? parent.ToLocal(pad.GlobalPosition) : pad.GlobalPosition;
        _commandPostContainer.AddChild(post);
        _commandPosts.Register(post);
        return post;
    }

    // Called when a placed instance is sold; frees its pad for reuse. No-op
    // (returns false) for a node this service never placed — e.g. one of the
    // mission's pre-placed grey-box towers, which aren't tied to any pad.
    public bool ReleasePad(Node2D placedInstance)
    {
        if (placedInstance == null || !_padByInstance.TryGetValue(placedInstance, out var pad)) return false;
        pad.SetOccupied(false);
        _padByInstance.Remove(placedInstance);
        return true;
    }

    // A doctrine's relocate_tower utility (GDD §8.2.5 Celere's Redeploy —
    // "instantly move any tower to any empty pad"). Only moves a tower this
    // service itself placed (i.e. one tracked in _padByInstance); a
    // scene-authored pre-placed tower has no pad to release.
    public bool TryRelocate(TowerController tower, BuildPad destinationPad)
    {
        if (tower == null || destinationPad == null || destinationPad.IsOccupied) return false;
        if (!destinationPad.Allows(tower.Definition)) return false;
        if (!_padByInstance.TryGetValue(tower, out var sourcePad) || sourcePad == destinationPad) return false;

        sourcePad.SetOccupied(false);
        tower.GlobalPosition = destinationPad.GlobalPosition;
        tower.PadTag = destinationPad.Tag;
        destinationPad.SetOccupied(true);
        _padByInstance[tower] = destinationPad;
        return true;
    }
}
