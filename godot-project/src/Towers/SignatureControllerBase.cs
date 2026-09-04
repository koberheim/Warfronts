using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Common placement, level, click-to-target, and charge presentation behavior
// for national signatures. Unique mechanics remain in derived controllers.
public abstract partial class SignatureControllerBase : Node2D
{
    [Export] public SignatureDefinition Definition;
    [Export(PropertyHint.Range, "1,3,1")] public int Level = 1;
    [Export] public PadTag PadTag = PadTag.Standard;

    protected PathNetwork Path { get; private set; }
    protected Func<IReadOnlyList<ITargetable>> TargetsProvider { get; private set; }
    protected bool IsTargeting { get; private set; }

    public int CurrentLevel => Mathf.Clamp(Level, 1, 3);
    public int Charges { get; protected set; }
    public int MaxCharges => ChargeCapacity;
    public bool IsFullyCharged => Charges >= MaxCharges;

    // Set each tick by DoctrineSystem's passive pass (GDD §19 prompt 39 —
    // Bomber Command's "RAF Scramble charges regenerate 25% faster",
    // Kampfgruppe's "Blitzkrieg recharge −20%"). Multiplies the regen/
    // recharge interval, so smaller is faster; 1f (no-op) for any mission
    // with no doctrine loaded.
    public float DoctrineRegenMultiplier = 1f;

    public virtual void Initialize(PathNetwork path, Func<IReadOnlyList<ITargetable>> targetsProvider)
    {
        Path = path;
        TargetsProvider = targetsProvider;
        Charges = MaxCharges;
        QueueRedraw();
    }

    protected virtual int ChargeCapacity => GetInt(Definition?.ChargeCaps, 0);

    public bool TryUpgrade(out int cost)
    {
        cost = 0;
        if (Definition?.LevelCosts == null || CurrentLevel >= Definition.LevelCosts.Length) return false;
        cost = Definition.LevelCosts[CurrentLevel];
        Level = CurrentLevel + 1;
        Charges = Mathf.Min(Charges, MaxCharges);
        QueueRedraw();
        return true;
    }

    protected void SetupSignatureClickArea(Action clickAction = null)
    {
        var area = new Area2D { InputPickable = true };
        area.AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 24f } });
        AddChild(area);
        area.InputEvent += (viewport, @event, shapeIdx) =>
        {
            if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                if (clickAction != null) clickAction();
                else IsTargeting = !IsTargeting;
                EventBus.Instance?.Publish(new SignatureClickedEvent(this));
                QueueRedraw();
                GetViewport().SetInputAsHandled();
            }
        };
    }

    protected void EnableSignatureInput() => ProcessMode = ProcessModeEnum.Always;

    protected bool TryGetTargetClick(InputEvent @event, out Vector2 worldPoint)
    {
        worldPoint = Vector2.Zero;
        if (!IsTargeting) return false;
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Escape })
        {
            IsTargeting = false;
            QueueRedraw();
            return true;
        }
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse)
            return false;

        worldPoint = GetViewport().GetCanvasTransform().AffineInverse() * mouse.Position;
        IsTargeting = false;
        QueueRedraw();
        return true;
    }

    protected void BeginTargeting() => IsTargeting = true;

    protected bool TrySpendCharges(int amount)
    {
        if (amount < 1 || Charges < amount) return false;
        Charges -= amount;
        QueueRedraw();
        return true;
    }

    protected int GetInt(int[] values, int fallback)
        => values is { Length: > 0 } ? values[Mathf.Clamp(CurrentLevel - 1, 0, values.Length - 1)] : fallback;

    protected float GetFloat(float[] values, float fallback)
        => values is { Length: > 0 } ? values[Mathf.Clamp(CurrentLevel - 1, 0, values.Length - 1)] : fallback;

    protected void DrawSignatureBase(Color bodyColor, Color accentColor)
    {
        DrawRect(new Rect2(-25f, -25f, 50f, 50f), bodyColor);
        DrawRect(new Rect2(-25f, -25f, 50f, 50f), Colors.White, false, 3f);
        float ratio = MaxCharges > 0 ? (float)Charges / MaxCharges : 0f;
        DrawRect(new Rect2(-20f, 18f, 40f, 5f), new Color(0.08f, 0.1f, 0.12f));
        DrawRect(new Rect2(-20f, 18f, 40f * ratio, 5f), accentColor);
        for (int i = 0; i < MaxCharges; i++)
            DrawCircle(new Vector2(-16f + i * 10f, -34f), 3f,
                i < Charges ? accentColor : new Color(0.2f, 0.22f, 0.24f));
    }

    protected void DrawDashedSegment(float startDistance, float endDistance, Color color, float width = 3f)
    {
        if (Path == null || Path.LengthPixels <= 0f) return;
        const int segments = 12;
        for (int i = 0; i < segments; i += 2)
        {
            float a = Mathf.Lerp(startDistance, endDistance, i / (float)segments);
            float b = Mathf.Lerp(startDistance, endDistance, (i + 1) / (float)segments);
            DrawLine(ToLocal(Path.GetPositionAtDistance(a)), ToLocal(Path.GetPositionAtDistance(b)), color, width);
        }
    }

    protected void DrawTargetingPreview(float lengthTiles, Color color)
    {
        if (!IsTargeting || Path == null) return;
        float center = Path.GetClosestDistance(GetGlobalMousePosition());
        float half = lengthTiles * GameBalanceConfigAutoload.Config.TilePixelSize * 0.5f;
        DrawDashedSegment(Mathf.Max(0f, center - half), Mathf.Min(Path.LengthPixels, center + half), color, 4f);
    }
}
