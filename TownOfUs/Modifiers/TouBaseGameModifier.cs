using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class TouBaseGameModifier : GameModifier
{
    public virtual string LocaleKey => "KEY_MISS";
    public virtual string IntroInfo => $"{TouLocale.Get("Modifier")}: {ModifierName}";
    public virtual float IntroSize => 4f;
    public virtual ModifierFaction FactionType => ModifierFaction.Universal;

    public virtual int CustomAmount => GetAmountPerGame();
    public virtual int CustomChance => GetAssignmentChance();

    public override bool HideOnUi => false;

    public override int GetAmountPerGame()
    {
        return 1;
    }
}

public enum ModifierFaction
{
    Alliance,
    Universal,
    Crewmate,
    Neutral,
    Impostor,
    CrewmateAlliance,
    CrewmateUtility,
    CrewmateVisibility,
    CrewmatePostmortem,
    CrewmatePassive,
    NeutralAlliance,
    NeutralUtility,
    NeutralVisibility,
    NeutralPostmortem,
    NeutralPassive,
    ImpostorAlliance,
    ImpostorUtility,
    ImpostorVisibility,
    ImpostorPostmortem,
    ImpostorPassive,
    UniversalUtility,
    UniversalVisibility,
    UniversalPostmortem,
    UniversalPassive,
    AssailantUtility,
    AssailantVisibility,
    AssailantPostmortem,
    AssailantPassive,
    NonCrewmate,
    NonCrewUtility,
    NonCrewVisibility,
    NonCrewPostmortem,
    NonCrewPassive,
    NonNeutral,
    NonNeutUtility,
    NonNeutVisibility,
    NonNeutPostmortem,
    NonNeutPassive,
    NonImpostor,
    NonImpUtility,
    NonImpVisibility,
    NonImpPostmortem,
    NonImpPassive,
    HiderUtility,
    HiderVisibility,
    HiderPostmortem,
    HiderPassive,
    SeekerUtility,
    SeekerVisibility,
    SeekerPostmortem,
    SeekerPassive,
    External,
    Other
}