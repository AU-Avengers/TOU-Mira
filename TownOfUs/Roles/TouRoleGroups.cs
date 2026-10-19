using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Roles;

public static class TouRoleGroups
{
   public static RoleOptionsGroup CrewHider { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewHider", "Hiders (Hide and Seek)"), TownOfUsColors.Crewmate, -10);
    public static RoleOptionsGroup ImpSeeker { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpSeeker", "Seekers (Hide and Seek)"), TownOfUsColors.ImpSoft, -9);
    public static RoleOptionsGroup CrewBeliever { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewBeliever", "Crewmate Believer Roles (Cultist)"), TownOfUsColors.Crewmate, -8);
    public static RoleOptionsGroup CrewObstinate { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewObstinate", "Crewmate Obstinate Roles (Cultist)"), TownOfUsColors.Crewmate, -7);
    public static RoleOptionsGroup NeutralObstinate { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutralObstinate", "Neutral Obstinate Roles (Cultist)"), Color.gray, -6);
    public static RoleOptionsGroup ImpCultist { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpCultist", "Impostor Cultists (Cultist)"), TownOfUsColors.ImpSoft, -5);
    public static RoleOptionsGroup ImpFollower { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpFollower", "Impostor Followers (Cultist)"), TownOfUsColors.ImpSoft, -4);

    public static RoleOptionsGroup FrenzyKiller { get; } = new(MiraLocaleManager.Get("TouRoleGroupFrenzyKiller", "Kill Frenzy Roles"), Color.gray);

    public static RoleOptionsGroup CrewInvest { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewInvestigative", "Crewmate Investigative Roles"), TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewKiller { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewKilling", "Crewmate Killing Roles"), TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewProc { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewProtective", "Crewmate Protective Roles"), TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewPower { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewPower", "Crewmate Power Roles"), TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewSup { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewSupport", "Crewmate Support Roles"), TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewGhost { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewGhost", "Crewmate Ghost Roles"), TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewAfterlife { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewAfterlife", "Crewmate Afterlife Roles"), TownOfUsColors.Crewmate);

    public static RoleOptionsGroup NeutralBenign { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutralBenign", "Neutral Benign Roles"), Color.gray);
    public static RoleOptionsGroup NeutralEvil { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutralEvil", "Neutral Evil Roles"), Color.gray);
    public static RoleOptionsGroup NeutralOutlier { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutralOutlier", "Neutral Outlier Roles"), Color.gray);
    public static RoleOptionsGroup NeutralKiller { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutralKilling", "Neutral Killing Roles"), Color.gray);
    public static RoleOptionsGroup NeutralGhost { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutralGhost", "Neutral Ghost Roles"), Color.gray);
    public static RoleOptionsGroup NeutralAfterlife { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutralAfterlife", "Neutral Afterlife Roles"), Color.gray);

    public static RoleOptionsGroup ImpConceal { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpConcealing", "Impostor Concealing Roles"), TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpKiller { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpKilling", "Impostor Killing Roles"), TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpPower { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpPower", "Impostor Power Roles"), TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpSup { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpSupport", "Impostor Support Roles"), TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpGhost { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpGhost", "Impostor Ghost Roles"), TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpAfterlife { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpAfterlife", "Impostor Afterlife Roles"), TownOfUsColors.ImpSoft);

    public static RoleOptionsGroup Other { get; } = new(MiraLocaleManager.Get("TouRoleGroupOther", "Other Roles"), TownOfUsColors.Other);

    public static RoleOptionsGroup TownOfPolusCrewmate { get; } = new(MiraLocaleManager.Get("TouRoleGroupCrewmate", "Crewmate Roles"), TownOfUsColors.Crewmate);
    public static RoleOptionsGroup TownOfPolusNeutral { get; } = new(MiraLocaleManager.Get("TouRoleGroupNeutral", "Neutral Roles"), Color.gray);
    public static RoleOptionsGroup TownOfPolusImpostor { get; } = new(MiraLocaleManager.Get("TouRoleGroupImpostor", "Impostor Roles"), TownOfUsColors.ImpSoft);
}