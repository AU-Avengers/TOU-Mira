using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class ClericRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Protective;
    public string LocaleKey => "Cleric";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Barrier", "Barrier"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}BarrierWikiDescription").Replace("<BarrierCooldown>",
                        $"{OptionGroupSingleton<ClericOptions>.Instance.BarrierCooldown}"),
                    TouCrewAssets.BarrierSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Cleanse", "Cleanse"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}CleanseWikiDescription"),
                    TouCrewAssets.CleanseSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Cleric;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Scientist),
        Icon = TouRoleIcons.Cleric
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    [MethodRpc((uint)TownOfUsRpc.ClericBarrierAttacked, SendImmediately = true)]
    public static void RpcClericBarrierAttacked(PlayerControl cleric, PlayerControl source, PlayerControl shielded)
    {
        if (cleric.Data.Role is not ClericRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcClericBarrierAttacked - Invalid cleric");
            return;
        }

        if (PlayerControl.LocalPlayer.PlayerId == source.PlayerId ||
            (PlayerControl.LocalPlayer.PlayerId == cleric.PlayerId &&
             OptionGroupSingleton<ClericOptions>.Instance.AttackNotif))
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Cleric));
        }
    }
}