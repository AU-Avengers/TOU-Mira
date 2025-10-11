using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Roles;

public static class TouRoleGroups
{
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