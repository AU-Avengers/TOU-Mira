﻿using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SpyRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Spy";
    public string RoleDescription => "Snoop Around And Find Stuff Out";
    public string RoleLongDescription => "Gain extra information on the Admin Table";
    public Color RoleColor => TownOfUsColors.Spy;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;
    public DoomableType DoomHintType => DoomableType.Perception;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Spy,
        IntroSound = TouAudio.SpyIntroSound,
    };
    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);
        if (Player.AmOwner)
        {
            CustomButtonSingleton<SpyAdminTableRoleButton>.Instance.AvailableCharge = OptionGroupSingleton<SpyOptions>.Instance.StartingCharge.Value;
        }
    }
    public static void OnRoundStart()
    {
        CustomButtonSingleton<SpyAdminTableRoleButton>.Instance.AvailableCharge += OptionGroupSingleton<SpyOptions>.Instance.RoundCharge.Value;
    }
    public static void OnTaskComplete()
    {
        CustomButtonSingleton<SpyAdminTableRoleButton>.Instance.AvailableCharge += OptionGroupSingleton<SpyOptions>.Instance.TaskCharge.Value;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    
    public string GetAdvancedDescription()
    {
        return
            "The Spy is a Crewmate Investigative role that gains extra information on the admin table. They not only see how many people are in a room, but will also see who is in every room."
            + MiscUtils.AppendOptionsText(GetType());
    }
}
