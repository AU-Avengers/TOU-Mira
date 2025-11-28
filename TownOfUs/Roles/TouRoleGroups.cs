using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Roles;

public static class TouRoleGroups
{
    public static RoleOptionsGroup CrewHider { get; } = new("Hiders (Hide and Seek)", TownOfUsColors.Crewmate, -10);
    public static RoleOptionsGroup ImpSeeker { get; } = new("Seekers (Hide and Seek)", TownOfUsColors.ImpSoft, -9);
    public static RoleOptionsGroup CrewBeliever { get; } = new("Crewmate Believer Roles (Cultist)", TownOfUsColors.Crewmate, -8);
    public static RoleOptionsGroup CrewObstinate { get; } = new("Crewmate Obstinate Roles (Cultist)", TownOfUsColors.Crewmate, -7);
    public static RoleOptionsGroup NeutralObstinate { get; } = new("Neutral Obstinate Roles (Cultist)", Color.gray, -6);
    public static RoleOptionsGroup ImpCultist { get; } = new("Impostor Cultists (Cultist)", TownOfUsColors.ImpSoft, -5);
    public static RoleOptionsGroup ImpFollower { get; } = new("Impostor Followers (Cultist)", TownOfUsColors.ImpSoft, -4);
    public static RoleOptionsGroup CrewInvest { get; } = new("Crewmate Investigative Roles", TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewKiller { get; } = new("Crewmate Killing Roles", TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewProc { get; } = new("Crewmate Protective Roles", TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewPower { get; } = new("Crewmate Power Roles", TownOfUsColors.Crewmate);
    public static RoleOptionsGroup CrewSup { get; } = new("Crewmate Support Roles", TownOfUsColors.Crewmate);
    public static RoleOptionsGroup NeutralBenign { get; } = new("Neutral Benign Roles", Color.gray);
    public static RoleOptionsGroup NeutralEvil { get; } = new("Neutral Evil Roles", Color.gray);
    public static RoleOptionsGroup NeutralOutlier { get; } = new("Neutral Outlier Roles", Color.gray);
    public static RoleOptionsGroup NeutralKiller { get; } = new("Neutral Killing Roles", Color.gray);
    public static RoleOptionsGroup ImpConceal { get; } = new("Impostor Concealing Roles", TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpKiller { get; } = new("Impostor Killing Roles", TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpPower { get; } = new("Impostor Power Roles", TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup ImpSup { get; } = new("Impostor Support Roles", TownOfUsColors.ImpSoft);
    public static RoleOptionsGroup Other { get; } = new("Other Roles", TownOfUsColors.Other);
}