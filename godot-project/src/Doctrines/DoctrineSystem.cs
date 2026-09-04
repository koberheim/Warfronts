using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using System.Collections.Generic;

namespace FrontsOfWar.Doctrines;

// Owns the mission's one selected doctrine (GDD §8.3, §19 prompt 39):
// applies its passive every tick (like Command Post auras) and executes its
// ability by Kind, reusing SignatureTargeting/SpatialGrid/FriendlyUnitManager/
// EnemyController exactly like the universal AbilitySystem and the national
// signatures do. Constructed once by MapRuntime with every manager it needs —
// mirroring how MapRuntime itself aggregates managers in _Ready — since a
// doctrine's passive alone touches towers, Command Posts, minefields, and
// the signature tower.
public partial class DoctrineSystem
{
    private readonly DoctrineDefinition _doctrine;
    private readonly GameBalanceConfig _config;
    private readonly TowerManager _towers;
    private readonly CommandPostManager _commandPosts;
    private readonly MinefieldManager _minefields;
    private readonly SignatureManager _signatures;
    private readonly FriendlyUnitManager _friendlyUnits;
    private readonly PathNetwork _path;
    private readonly TowerPlacementService _placement;
    private readonly ProjectileManager _projectiles;
    private readonly CommandPointLedger _commandPoints;
    private readonly SupplyLedger _supply;
    private readonly DefenseLineLedger _defenseLine;

    private float _cooldownRemaining;
    private bool _oncePerMissionUsed;
    private float _abilityBuffRemaining;
    private TowerController _abilityBuffSingleTarget;
    private readonly List<PendingLineSweep> _pendingSweeps = new();

    public DoctrineSystem(DoctrineDefinition doctrine, GameBalanceConfig config, TowerManager towers,
        CommandPostManager commandPosts, MinefieldManager minefields, SignatureManager signatures,
        FriendlyUnitManager friendlyUnits, PathNetwork path, TowerPlacementService placement,
        ProjectileManager projectiles, CommandPointLedger commandPoints, SupplyLedger supply,
        DefenseLineLedger defenseLine)
    {
        _doctrine = doctrine;
        _config = config;
        _towers = towers;
        _commandPosts = commandPosts;
        _minefields = minefields;
        _signatures = signatures;
        _friendlyUnits = friendlyUnits;
        _path = path;
        _placement = placement;
        _projectiles = projectiles;
        _commandPoints = commandPoints;
        _supply = supply;
        _defenseLine = defenseLine;
    }

    // Doctrine resource paths are authored as <nation>_<doctrine_id>.tres
    // (GDD §19 prompt 39). Missing/unset resolves to no doctrine rather than
    // an error, so a debug scene with no MissionSession.SelectedDoctrineId
    // still runs — DoctrineSystem no-ops everywhere the doctrine is null.
    public static DoctrineDefinition LoadDoctrine(string nationId, string doctrineId)
    {
        string id = string.IsNullOrEmpty(doctrineId) ? "lend_lease" : doctrineId;
        string path = $"res://assets/data/doctrines/{nationId}_{id}.tres";
        return ResourceLoader.Exists(path) ? GD.Load<DoctrineDefinition>(path) : null;
    }

    public DoctrineDefinition Doctrine => _doctrine;
    public bool IsOnCooldown => _cooldownRemaining > 0f;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
    public int CpCost => _doctrine?.Ability?.CommandPointCost ?? 0;
    public bool IsExhausted => _doctrine?.Ability?.OncePerMission == true && _oncePerMissionUsed;
    public DoctrineTargetingMode TargetingMode => ResolveTargetingMode(_doctrine?.Ability);

    // One-time mission-start effects with no per-tick dependency — called
    // once by MapRuntime right after construction, since Supply/DefenseLine
    // already exist by then (GDD's Home Guard "+6 Integrity" and Lend-Lease/
    // Deep Battle-style income leans are permanent for the mission, not
    // recomputed each tick).
    public void ApplyMissionStart()
    {
        if (_doctrine?.Passive == null) return;
        _defenseLine.RaiseMaxIntegrity(_doctrine.Passive.DefenseLineBonus);
        _supply.DoctrineIncomeMultiplier = _doctrine.Passive.SupplyIncomeMultiplier;
    }

    // Delegate target for TowerPlacementService.DoctrineCostMultiplierProvider.
    public float PlacementCostMultiplier(TowerDefinition definition, PadTag padTag)
        => definition == null ? 1f : DoctrineModifiers.PlacementCostMultiplier(_doctrine?.Passive, definition.Archetype, padTag);

    public void Tick(float tickDeltaSeconds, SpatialGrid grid)
    {
        if (_doctrine?.Passive == null) return;

        if (_cooldownRemaining > 0f) _cooldownRemaining -= tickDeltaSeconds;
        if (_abilityBuffRemaining > 0f) _abilityBuffRemaining -= tickDeltaSeconds;
        else _abilityBuffSingleTarget = null;

        DoctrineModifiers.ApplyToTowers(_doctrine.Passive, _doctrine.Ability, _abilityBuffRemaining > 0f,
            _abilityBuffSingleTarget, _towers, _config);
        DoctrineModifiers.ApplyToCommandPosts(_doctrine.Passive, _commandPosts);
        DoctrineModifiers.ApplyToMinefields(_doctrine.Passive, _minefields);
        DoctrineModifiers.ApplyToSignatures(_doctrine.Passive, _signatures);

        TickPendingSweeps(tickDeltaSeconds, grid);
    }

    // targetPoint is always required (even for None/Tower/TowerThenPad modes
    // it's simply ignored by the executor) so the hotbar's single call shape
    // stays uniform with AbilitySystem.TryActivate.
    public bool TryActivate(Vector2 primaryPoint, SpatialGrid grid, Vector2? secondaryPoint = null,
        TowerController towerTarget = null, BuildPad padTarget = null)
    {
        var ability = _doctrine?.Ability;
        if (ability == null || IsOnCooldown || IsExhausted) return false;
        if (!ValidateTarget(ability, towerTarget, padTarget)) return false;
        if (!_commandPoints.TrySpend(ability.CommandPointCost)) return false;

        switch (ability.Kind)
        {
            case DoctrineAbilityKind.PointBlast: ExecutePointBlast(ability, primaryPoint, grid); break;
            case DoctrineAbilityKind.LineBlast: ExecuteLineBlast(ability, primaryPoint, secondaryPoint, grid); break;
            case DoctrineAbilityKind.AuraBuff: ExecuteAuraBuff(ability, towerTarget); break;
            case DoctrineAbilityKind.SpawnFriendly: ExecuteSpawnFriendly(ability, primaryPoint); break;
            case DoctrineAbilityKind.InstantRefund: ExecuteUtility(ability, primaryPoint, towerTarget, padTarget, grid); break;
            case DoctrineAbilityKind.StatusApplication: ExecuteStatusApplication(ability, primaryPoint, grid); break;
        }

        _cooldownRemaining = ability.CooldownSeconds;
        if (ability.OncePerMission) _oncePerMissionUsed = true;
        return true;
    }

    private static bool ValidateTarget(DoctrineAbility ability, TowerController towerTarget, BuildPad padTarget)
    {
        var mode = ResolveTargetingMode(ability);
        return mode switch
        {
            DoctrineTargetingMode.Tower => towerTarget != null,
            DoctrineTargetingMode.TowerThenPad => towerTarget != null && padTarget != null && !padTarget.IsOccupied,
            _ => true,
        };
    }

    private static DoctrineTargetingMode ResolveTargetingMode(DoctrineAbility ability)
    {
        if (ability == null) return DoctrineTargetingMode.None;
        switch (ability.Kind)
        {
            case DoctrineAbilityKind.PointBlast: return DoctrineTargetingMode.Point;
            case DoctrineAbilityKind.StatusApplication: return DoctrineTargetingMode.Point;
            case DoctrineAbilityKind.SpawnFriendly: return DoctrineTargetingMode.Point;
            case DoctrineAbilityKind.LineBlast:
                return ability.LineMode == DoctrineLineMode.DrawnLine
                    ? DoctrineTargetingMode.TwoPoints : DoctrineTargetingMode.Point;
            case DoctrineAbilityKind.AuraBuff:
                return ability.SingleTarget ? DoctrineTargetingMode.Tower : DoctrineTargetingMode.None;
            case DoctrineAbilityKind.InstantRefund:
                return ability.UtilityId switch
                {
                    DoctrineUtilityId.RefundTower => DoctrineTargetingMode.Tower,
                    DoctrineUtilityId.RelocateTower => DoctrineTargetingMode.TowerThenPad,
                    DoctrineUtilityId.ForceTarget => DoctrineTargetingMode.Enemy,
                    _ => DoctrineTargetingMode.None,
                };
            default: return DoctrineTargetingMode.None;
        }
    }
}
