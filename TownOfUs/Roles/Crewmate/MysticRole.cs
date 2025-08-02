using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MysticRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string RoleName => TouLocale.Get(TouNames.Mystic, "Mystic");
    public string RoleDescription => "Know When and Where Kills Happen";
    public string RoleLongDescription => "Understand when and where kills happen";
    public Color RoleColor => TownOfUsColors.Mystic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Mystic,
        IntroSound = TouAudio.MediumIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is a Crewmate Investigative role that gets an alert when someone dies."
               + MiscUtils.AppendOptionsText(GetType());
    }
}