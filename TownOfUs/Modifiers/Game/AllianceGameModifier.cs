using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Roles.Other;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class AllianceGameModifier : TouBaseGameModifier
{
    public override string IntroInfo => $"{MiraLocaleManager.Get("Alliance")}: {ModifierName}";
    public virtual string Symbol => "?";
    public virtual bool DoesTasks => true;
    public virtual bool GetsPunished => true;
    public virtual bool CrewContinuesGame => true;
    public override ModifierFaction FactionType => ModifierFaction.Alliance;
    // Set to Crewmate, Impostor or Neutral
    public virtual AlliedFaction TrueFactionType => AlliedFaction.Other;
    public virtual bool CountTowardsTrueFaction => false;
    // Only used when the TrueFactionType is set to RoleSpecific
    public virtual RoleBehaviour UnderlyingRole => RoleManager.Instance.GetRole(RoleTypes.Crewmate);

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<AllianceGameModifier>(true) &&
               !role.Player.HasModifier<ExecutionerTargetModifier>() && role is not SpectatorRole;
    }

    public override void AddModifierToStats(PlayerStats stats)
    {
        stats.LastKnownModifiers.Add(this);
    }
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