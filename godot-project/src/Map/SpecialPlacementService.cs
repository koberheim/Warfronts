using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Map;

// Minefields are path placements; national signatures use ordinary empty pads.
// Typed dispatch preserves their Resource contracts without casting either to a gun.
public sealed class SpecialPlacementService
{
    private readonly Node2D _parent;
    private readonly SupplyLedger _supply;
    private readonly SignatureManager _signatures;
    private readonly MinefieldManager _minefields;
    private readonly FriendlyUnitManager _friendlies;
    private readonly IReadOnlyList<PathNetwork> _paths;
    private readonly GameBalanceConfig _config;
    private readonly List<ArsenalController> _arsenals = new();
    public Func<TowerDefinition, PadTag, float> CostMultiplier { get; set; }
    public Func<int> ExtraMinefieldCapacity { get; set; }
    public int SignatureCount => _signatures.Signatures.Count + _arsenals.Count;

    // Read by the HUD's visible field counter (GDD §6 T8: "maximum 6 fields
    // on the map at once, enforced with a visible counter, because they are
    // free-placement") so it matches TryPlaceMinefield's own cap check.
    public int EffectiveMaxMinefields => _config.MinefieldMaximumFields + (ExtraMinefieldCapacity?.Invoke() ?? 0);

    public SpecialPlacementService(Node2D parent, SupplyLedger supply, SignatureManager signatures,
        MinefieldManager minefields, FriendlyUnitManager friendlies, IReadOnlyList<PathNetwork> paths, GameBalanceConfig config)
    {
        _parent = parent; _supply = supply; _signatures = signatures; _minefields = minefields;
        _friendlies = friendlies; _paths = paths; _config = config;
    }

    public TowerPlacementOutcome TryPlaceSignature(BuildOption option, BuildPad pad)
    {
        if (option?.IsSignature != true || option.Scene == null || pad == null) return Refuse(TowerPlacementResult.NoControllerScene);
        if (pad.IsOccupied) return Refuse(TowerPlacementResult.PadOccupied);
        if (pad.AllowedArchetypeIds.Length > 0 && !pad.AllowedArchetypeIds.Contains(option.Id)) return Refuse(TowerPlacementResult.ArchetypeNotAllowed);
        if (SignatureCount >= _signatures.Limit) return Refuse(TowerPlacementResult.SignatureLimitReached);
        if (_supply.Balance < option.Cost) return new(TowerPlacementResult.InsufficientSupply, option.Cost - _supply.Balance);
        var instance = option.Scene.Instantiate<Node2D>();
        if (instance is not SignatureControllerBase && instance is not ArsenalController)
        { instance.Free(); return Refuse(TowerPlacementResult.NoControllerScene); }
        var path = ClosestPath(pad.GlobalPosition, out _);
        if (path == null) { instance.Free(); return Refuse(TowerPlacementResult.RequiresPath); }
        instance.Position = _parent.ToLocal(pad.GlobalPosition);
        if (instance is SignatureControllerBase signature)
        {
            signature.Definition = (SignatureDefinition)option.Resource;
            signature.PadTag = pad.Tag;
            _parent.AddChild(signature);
            _signatures.Register(signature, path);
        }
        else if (instance is ArsenalController arsenal)
        {
            arsenal.Definition = (ArsenalDefinition)option.Resource;
            _parent.AddChild(arsenal);
            arsenal.Initialize(_friendlies, path);
            _arsenals.Add(arsenal);
        }
        _supply.TrySpend(option.Cost);
        pad.SetOccupied(true);
        EventBus.Instance?.Publish(new TowerPlacedEvent(instance, null, pad, option.Cost));
        return new(TowerPlacementResult.Success, 0, instance);
    }

    public TowerPlacementOutcome TryPlaceMinefield(TowerDefinition definition, Vector2 worldPoint)
    {
        if (definition?.Archetype != TowerArchetype.Minefield || definition.ControllerScene == null)
            return Refuse(TowerPlacementResult.NoControllerScene);
        var route = ClosestPath(worldPoint, out var snapped);
        if (route == null || snapped.DistanceTo(worldPoint) > _config.MinefieldPlacementToleranceTiles * _config.TilePixelSize)
            return Refuse(TowerPlacementResult.RequiresPath);
        if (_minefields.Fields.Count >= _config.MinefieldMaximumFields + (ExtraMinefieldCapacity?.Invoke() ?? 0))
            return Refuse(TowerPlacementResult.FieldLimitReached);
        if (_minefields.Fields.Any(field => field.GlobalPosition.DistanceTo(snapped) < _config.MinefieldMinimumSpacingTiles * _config.TilePixelSize))
            return Refuse(TowerPlacementResult.TooCloseToMinefield);
        int cost = Mathf.RoundToInt(definition.PreForkStatsForLevel(1).Cost * (CostMultiplier?.Invoke(definition, PadTag.Standard) ?? 1f));
        if (_supply.Balance < cost) return new(TowerPlacementResult.InsufficientSupply, cost - _supply.Balance);
        var instance = definition.ControllerScene.Instantiate<Node2D>();
        if (instance is not MinefieldController fieldInstance)
        { instance.Free(); return Refuse(TowerPlacementResult.NoControllerScene); }
        fieldInstance.Definition = definition;
        fieldInstance.Position = _parent.ToLocal(snapped);
        _parent.AddChild(fieldInstance);
        _minefields.Register(fieldInstance);
        _supply.TrySpend(cost);
        EventBus.Instance?.Publish(new TowerPlacedEvent(fieldInstance, definition, null, cost));
        return new(TowerPlacementResult.Success, 0, fieldInstance);
    }

    public void Tick(float delta)
    {
        foreach (var arsenal in _arsenals) arsenal.SimTick(delta);
    }

    private PathNetwork ClosestPath(Vector2 position, out Vector2 closest)
    {
        closest = Vector2.Zero;
        PathNetwork result = null;
        float best = float.PositiveInfinity;
        foreach (var path in _paths)
        {
            var point = path.GetPositionAtDistance(path.GetClosestDistance(position));
            float distance = point.DistanceSquaredTo(position);
            if (distance >= best) continue;
            result = path; closest = point; best = distance;
        }
        return result;
    }
    private static TowerPlacementOutcome Refuse(TowerPlacementResult result) => new(result);
}
