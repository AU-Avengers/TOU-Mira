using MiraAPI.Events;
using UnityEngine;

namespace TownOfUs.Events.TouEvents;

/// <summary>
///     Event that is invoked after a player uses specific abilities. This event is not cancelable.
/// </summary>
public class TouAbilityEvent : MiraEvent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TouAbilityEvent" /> class.
    /// </summary>
    /// <param name="ability">The player's ability that was used.</param>
    /// <param name="player">The player who used the ability.</param>
    /// <param name="target">The player's target, if available.</param>
    /// <param name="target2">The player's second target, if available.</param>
    public TouAbilityEvent(AbilityType ability, PlayerControl player, MonoBehaviour? target = null,
        MonoBehaviour? target2 = null)
    {
        AbilityType = ability;
        Player = player;
        Target = target;
        Target2 = target2;
    }

    /// <summary>
    ///     Gets the player who used the ability.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    ///     Gets the target of the ability, if any.
    /// </summary>
    public MonoBehaviour? Target { get; set; }

    /// <summary>
    ///     Gets the second target of the ability, if any.
    /// </summary>
    public MonoBehaviour? Target2 { get; set; }

    /// <summary>
    ///     Gets the ability used by the player.
    /// </summary>
    public AbilityType AbilityType { get; }
}

public enum AbilityType
{
    AltruistRevive,
    ClericBarrier,
    ClericCleanse,
    DeputyCamp,

    // DetectiveExamine,
    // DetectiveInspect,
    EngineerFix,

    // EngineerVent,
    HunterStalk,
    JailorJail,
    LookoutWatch,
    MedicShield,
    MagicMirror,
    MediumMediate,
    OracleBless,
    OracleConfess,
    PlumberBlock,
    PlumberFlush,
    PoliticianCampaign,

    // SeerReveal,
    // SheriffShoot,
    // TrackerTrack,
    TransporterTransport,

    // TrapperTrap,
    VeteranAlert,
    WardenFortify,
    BlackmailerBlackmail,
    BomberPlant,
    EclipsalBlind,
    EscapistMark,
    EscapistRecall,
    GrenadierFlash,
    HypnotistHypno,
    HypnotistHysteria,
    JanitorClean,
    MinerPlaceVent,
    MinerRevealVent,

    // MorphlingSample,
    MorphlingMorph,
    MorphlingUnmorph,
    SwooperSwoop,
    SwooperUnswoop,
    TraitorChangeRole,
    UndertakerDrag,
    UndertakerDrop,
    VenererCamoAbility,
    VenererSprintAbility,
    VenererFreezeAbility,

    // WarlockBurstKill,
    AmnesiacPreRemember,
    AmnesiacPostRemember,
    ArsonistDouse,
    SpellslingerHex,

    // ArsonistIgnite,
    // DoomsayerObserve,
    GlitchInitialHack,
    GlitchHackTrigger,
    GlitchMimic,
    GlitchUnmimic,
    GuardianAngelProtect,

    // InquisitorInquire,
    MercenaryGuard,
    MercenaryBribe,
    PlaguebearerInfect,
    SurvivorVest,

    VampireBite
    // WerewolfRampage
}