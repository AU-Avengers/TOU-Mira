using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Roles.Other;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class TouGameModifier : TouBaseGameModifier
{
    public override float IntroSize => 2.8f;
    public virtual bool PreventsOtherModifiers => true;
    public virtual bool AppearsInSummary => true;
    public virtual bool AppearsInIntro => true;
    public virtual bool HideFromGuessing => false;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true, x => x.PreventsOtherModifiers) && role is not SpectatorRole;
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

public enum AlliedFaction
{
    Crewmate,
    CrewmateKiller,
    Neutral,
    NeutralKiller,
    Impostor,
    Lover,
    Recruit,
    RoleSpecific,
    Other,
}