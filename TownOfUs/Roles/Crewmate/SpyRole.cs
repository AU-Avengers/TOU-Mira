using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SpyRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string LocaleKey => "Spy";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Spy;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Spy,
        IntroSound = TouAudio.SpyIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            CustomButtonSingleton<SpyAdminTableRoleButton>.Instance.AvailableCharge =
                OptionGroupSingleton<SpyOptions>.Instance.StartingCharge.Value;
        }
    }

    public static void OnRoundStart()
    {
        CustomButtonSingleton<SpyAdminTableRoleButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<SpyOptions>.Instance.RoundCharge.Value;
    }

    public static void OnTaskComplete()
    {
        CustomButtonSingleton<SpyAdminTableRoleButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<SpyOptions>.Instance.TaskCharge.Value;
    }
}