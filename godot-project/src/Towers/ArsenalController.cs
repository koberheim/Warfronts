using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;

namespace FrontsOfWar.Towers;

// The US signature tower: continuous production with no direct attack. Its
// output is time and soft blocking, not a second player-controlled army.
public partial class ArsenalController : Node2D
{
    [Export] public ArsenalDefinition Definition;
    [Export(PropertyHint.Range, "1,3,1")] public int Level = 1;

    private FriendlyUnitManager _units;
    private PathNetwork _path;
    private float _productionElapsed;
    private bool _registered;

    public float ProductionProgress => Definition == null ? 0f : _productionElapsed / ProductionInterval;
    public float ProductionInterval => Definition?.ProductionIntervals[Mathf.Clamp(Level - 1, 0, Definition.ProductionIntervals.Length - 1)] ?? 14f;

    public void Initialize(FriendlyUnitManager units, PathNetwork path)
    {
        _units = units;
        _path = path;
        _registered = true;
    }

    public void SimTick(float delta)
    {
        if (!_registered || Definition?.Units is not { Length: > 0 } || Definition.UnitScene == null) return;
        _productionElapsed += delta;
        if (_productionElapsed < ProductionInterval || _units.LivingCount >= 5) return;
        _productionElapsed = 0f;
        var unit = SelectUnlockedUnit();
        if (unit != null)
            _units.Spawn(unit, Definition.UnitScene, _path, _path.GetClosestDistance(GlobalPosition));
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(-25f, -25f, 50f, 50f), new Color(0.3f, 0.55f, 0.72f, 1f));
        DrawRect(new Rect2(-25f, -25f, 50f, 50f), new Color(0.95f, 0.95f, 1f), false, 3f);
        DrawRect(new Rect2(-18f, 18f, 36f, 4f), new Color(0.08f, 0.1f, 0.12f));
        DrawRect(new Rect2(-18f, 18f, 36f * Mathf.Clamp(ProductionProgress, 0f, 1f), 4f), new Color(0.95f, 0.8f, 0.3f));
    }

    private FriendlyUnitDefinition SelectUnlockedUnit()
    {
        int unlocked = 0;
        for (int i = 0; i < Definition.Units.Length; i++)
            if (i < Definition.UnlockLevels.Length && Definition.UnlockLevels[i] <= Level) unlocked = i;
        return Definition.Units[Mathf.PosMod(unlocked, Definition.Units.Length)];
    }
}
