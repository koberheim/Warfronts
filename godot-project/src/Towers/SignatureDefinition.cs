using Godot;

namespace FrontsOfWar.Towers;

public enum SignatureKind
{
    RafScramble,
    KatyushaStorm,
    BlitzkriegCommand,
    BersaglieriCharge,
    SpecialAttackAirfield,
}

// Shared data surface for the six national signature entries. The behavior
// remains in one small controller per signature; balance values stay in
// Resources so authoring a national variant does not require code changes.
[GlobalClass]
public partial class SignatureDefinition : Resource
{
    [Export] public string Id = "";
    [Export] public string DisplayName = "";
    [Export] public string NationId = "";
    [Export] public SignatureKind Kind;
    [Export] public int[] LevelCosts = { 650, 423, 748 };
    [Export] public int[] ChargeCaps = { 2, 3, 4 };
    [Export] public float[] ChargeRegenSeconds = { 22f, 18f, 14f };

    [ExportGroup("RAF Scramble")]
    [Export] public int ElevatedExtraCharges = 1;
    [Export] public float RafCorridorWidthTiles = 3f;
    [Export] public float RafCorridorLengthTiles = 8f;
    [Export] public int RafPassCount = 3;
    [Export] public float RafStrikeDurationSeconds = 4f;
    [Export] public float[] RafSmallArmsDamagePerPass = { 55f, 66f, 77f };
    [Export] public float[] RafExplosiveDamagePerPass = { 20f, 24f, 28f };
    [Export] public float[] RafAntiAirDamage = { 200f, 220f, 240f };
    [Export] public int RafAirTargetLimit = 3;
    [Export] public float RafAutoScrambleRadiusTiles = 6f;

    [ExportGroup("Katyusha")]
    [Export] public float KatyushaChargePerSecond = 1f;
    [Export] public float KatyushaChargePerKill = 3f;
    [Export] public float[] KatyushaFullCharge = { 240f, 200f, 170f };
    [Export] public int[] KatyushaRocketCounts = { 24, 30, 36 };
    [Export] public float KatyushaRocketDamage = 60f;
    [Export] public float KatyushaBlastRadiusTiles = 1.8f;
    [Export] public float KatyushaImpactDurationSeconds = 3.5f;
    [Export] public int KatyushaCommandPointCost = 4;
    [Export] public float KatyushaCommandPointCharge = 40f;

    [ExportGroup("Blitzkrieg")]
    [Export] public float[] BlitzActiveDurations = { 12f, 15f, 15f };
    [Export] public float[] BlitzRechargeDurations = { 30f, 30f, 22f };
    [Export] public float[] BlitzRadiusTiles = { 7.5f, 9f, 9f };
    [Export] public float BlitzRateOfFireMultiplier = 1.45f;
    [Export] public float BlitzProjectileVelocityMultiplier = 1.60f;
    [Export] public float BlitzTraverseSpeedMultiplier = 1.70f;
    [Export] public float BlitzIndirectDelayReductionSeconds = 0.3f;
    [Export] public float BlitzPassiveRateOfFireMultiplier = 1.08f;
    [Export] public float BlitzSpottedDurationSeconds = 4f;

    [ExportGroup("Bersaglieri")]
    [Export] public float[] BersaglieriDeploymentIntervals = { 18f, 14f, 11f };
    [Export] public int[] BersaglieriSquadSizes = { 4, 5, 5 };
    [Export] public float BersaglieriSpeedMultiplier = 2.5f;
    [Export] public float BersaglieriGrenadeDamage = 35f;
    [Export] public float BersaglieriGrenadeBlastRadiusTiles = 1.4f;
    [Export] public float BersaglieriSuppressionRadiusTiles = 1.5f;
    [Export] public float BersaglieriLifetimeSeconds = 20f;
    [Export] public int BersaglieriMaxUnits = 8;
    [Export] public float BersaglieriSpottedDurationSeconds = 4f;

    [ExportGroup("Special Attack Airfield")]
    [Export] public float[] AirfieldChargeIntervals = { 40f, 32f, 25f };
    [Export] public int[] AirfieldStoredChargeCaps = { 2, 2, 3 };
    [Export] public int AirfieldCommandPointCost = 3;
    [Export] public float AirfieldCommandPointProgress = 12f;
    [Export] public float AirfieldStrikeDamage = 420f;
    [Export] public float AirfieldArmorPiercingFraction = 0.60f;
    [Export] public float AirfieldExplosiveFraction = 0.40f;
    [Export] public float AirfieldBlastRadiusTiles = 2.6f;
    [Export] public float AirfieldApproachSeconds = 2f;
    [Export] public float AirfieldLevelTwoDamageBonus = 60f;
    [Export] public float AirfieldLevelThreeBlastBonusTiles = 0.6f;
    [Export] public float AirfieldSpottedDurationSeconds = 8f;
}
