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
    public string RoleName => TouLocale.Get(TouNames.Cleric, "Cleric");
    public string RoleDescription => "Save The Crewmates";
    public string RoleLongDescription => "Barrier and Cleanse crewmates";
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

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Protective that can protect crewmates by negating their negative effects, as well as placing barriers on them to prevent interactions." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Barrier",
            $"Prevent a Crewmate from being interacted with. The shield will last for {OptionGroupSingleton<ClericOptions>.Instance.BarrierCooldown} seconds.",
            TouCrewAssets.BarrierSprite),
        new("Cleanse",
            "Remove all negative effects on a player. (Douse, Hack, Infect, Blackmail, Blind, Flash, and Hypnosis)",
            TouCrewAssets.CleanseSprite)
    ];

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