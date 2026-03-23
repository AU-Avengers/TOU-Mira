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
    /// <param name="result">The ability's result in text, used for detailed logging.</param>
    /// <param name="player">The player who used the ability.</param>
    /// <param name="target">The player's target, if available.</param>
    /// <param name="target2">The player's second target, if available.</param>
    public TouAbilityEvent(AbilityType ability, string result, PlayerControl player, MonoBehaviour? target = null,
        MonoBehaviour? target2 = null)
    {
        AbilityType = ability;
        Player = player;
        Target = target;
        Target2 = target2;
        Result = result;
    }
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
        Result = "No Information";
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

    /// <summary>
    ///     Gets the detailed results from the ability, if any.
    /// </summary>
    public string Result { get; }
}

public enum AbilityType
{
    TaskCompleted,
    TaskUndone,
    TaskInProgress,
    Kill,
    Ejected,
    Tie,
    Vent,
    Report,
    Meeting,
    Vote,
    Skip,
    SendChat,
    SendPrivateChat,

    HaunterRevealed,
    HaunterRevealsEvils,
    HaunterClicked,

    ForensicExamine,
    ForensicInspect,
    ForensicFeedback,
    LookoutWatch,
    LookoutFeedback,
    MediumMediate,
    MediumUnmediate,
    SeerReveal,
    SeerGaze,
    SeerIntuit,
    SeerComparisonResult,
    SnitchRevealed,
    SnitchRevealsEvils,
    SonarTrack,
    TrapperPlaceTrap,
    TrapperAddEntry,
    TrapperFeedback,

    DeputyCamp,
    DeputyBlast,
    HunterStalk,
    HunterRetribution,
    SheriffShoot,
    SheriffMisfire,
    VeteranAlert,
    VigilanteGuess,
    VigilanteMisguess,

    JailorJail,
    JailorBadExecute,
    JailorGoodExecute,
    JailorFailedExecute,
    MarshalTribunal,
    MarshalEjectionResults,
    MonarchKnight,
    MonarchProtectionChange,
    MonarchAttacked,
    MonarchKnightDies,
    PoliticianCampaign,
    PoliticianRevealFail,
    PoliticianRevealSuccess,
    ProsecutorProsecutes,
    SwapperSwap,
    TimeLordRewind,
    TimeLordRevive,

    AltruistRevive,
    AltruistMultiRevive,
    BenefactorAegisFail,
    BenefactorAegisSuccess,
    BenefactorAegisSelf,
    BenefactorAegisProtected,
    ClericBarrier,
    ClericCleanse,
    MedicShield,
    MedicShieldProtect,
    MagicMirror,
    OracleBless,
    OracleConfess,
    WardenFortify,
    WardenProtect,

    Roleblock,
    EngineerFix,
    ImitatorImitateCrew,
    ImitatorImitateImp,
    ImitatorImitateNeut,
    PlumberBlock,
    PlumberFlush,
    SentryPlaceCam,
    TransporterTransport,

    EclipsalBlind,
    EscapistMark,
    EscapistRecall,
    GrenadierFlash,
    MorphlingSample,
    MorphlingMorph,
    MorphlingUnmorph,
    SwooperSwoop,
    SwooperUnswoop,
    VenererCamoAbility,
    VenererSprintAbility,
    VenererFreezeAbility,

    AmbusherPursue,
    AmbusherAmbush,
    BomberPlant,
    BomberMultiKill,
    ParasiteOvertake,
    ParasiteKill,
    ScavengerScavenge,
    ScavengerCorrectKill,
    ScavengerIncorrectKill,
    WarlockBurstKill,

    AmbassadorRetrainSelect,
    AmbassadorRetrainAccepted,
    AmbassadorRetrainDenied,
    HerbalistExpose,
    HerbalistConfuse,
    HerbalistProtect,
    HerbalistProtectAttacked,
    PuppeteerControl,
    PuppeteerControlKill,
    SpellslingerHex,
    SpellslingerHexBombStart,
    SpellslingerHexBombCancel,
    SpellslingerHexBombComplete,
    TraitorSelected,
    TraitorChangeRole,

    BlackmailerBlackmail,
    HypnotistHypno,
    HypnotistHysteria,
    JanitorClean,
    MinerPlaceVent,
    MinerRevealVent,
    UndertakerDrag,
    UndertakerDrop,

    SpectreCompletedTasks,
    SpectreSpook,
    SpectreClicked,

    AmnesiacPreRemember,
    AmnesiacPostRemember,
    FairyProtect,
    FairyProtected,
    MercenaryGuard,
    MercenaryBribe,
    SurvivorVest,

    DoomsayerObserve,
    DoomsayerFeedback,
    DoomsayerGuessSuccess,
    DoomsayerGuessFail,
    ExecutionerVotedTarget,
    ExecutionerTorment,
    JesterVotedOut,
    JesterHaunt,

    ArsonistDouse,
    ArsonistIgnite,
    GlitchInitialHack,
    GlitchHackTrigger,
    GlitchMimic,
    GlitchUnmimic,
    PlaguebearerInfect,
    PlaguebearerTransform,

    VampireBite,
    WerewolfRampage,

    ChefCook,
    ChefServe,
    ChefWin,
    InquisitorInquire,
    InqusitorVanquishSuccess,
    InquisitorVanquishFail,
    InquisitorInquireFeedback,
    InquisitorWin,

    AssassinGuess,
    AssassinMisguess
}