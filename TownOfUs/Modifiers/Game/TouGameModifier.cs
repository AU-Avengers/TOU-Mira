using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class TouGameModifier : GameModifier
{
    public virtual string IntroInfo => $"Modifier: {ModifierName}";
    public virtual float IntroSize => 2.8f;
    public virtual ModifierFaction FactionType => ModifierFaction.Universal;

    public virtual int CustomAmount => GetAmountPerGame();
    public virtual int CustomChance => GetAssignmentChance();

    public override bool HideOnUi => false;

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true);
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
    CrewmateHarmful,
    NeutralAlliance,
    NeutralUtility,
    NeutralVisibility,
    NeutralPostmortem,
    NeutralPassive,
    NeutralHarmful,
    ImpostorAlliance,
    ImpostorUtility,
    ImpostorVisibility,
    ImpostorPostmortem,
    ImpostorPassive,
    ImpostorHarmful,
    UniversalUtility,
    UniversalVisibility,
    UniversalPostmortem,
    UniversalPassive,
    UniversalHarmful,
    AssailantUtility,
    AssailantVisibility,
    AssailantPostmortem,
    AssailantPassive,
    AssailantHarmful,
    NonCrewmate,
    NonCrewUtility,
    NonCrewVisibility,
    NonCrewPostmortem,
    NonCrewPassive,
    NonCrewHarmful,
    NonNeutral,
    NonNeutUtility,
    NonNeutVisibility,
    NonNeutPostmortem,
    NonNeutPassive,
    NonNeutHarmful,
    NonImpostor,
    NonImpUtility,
    NonImpVisibility,
    NonImpPostmortem,
    NonImpPassive,
    NonImpHarmful,
    External,
    Other
}