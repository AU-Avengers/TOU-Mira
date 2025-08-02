using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MediumRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public List<MediatedModifier> MediatedPlayers { get; } = new();

    public DoomableType DoomHintType => DoomableType.Death;
    public string RoleName => TouLocale.Get(TouNames.Medium, "Medium");
    public string RoleDescription => "Watch The Spooky Ghosts";
    public string RoleLongDescription => "Follow ghosts to get clues from them";
    public Color RoleColor => TownOfUsColors.Medium;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Medium,
        IntroSound = TouAudio.MediumIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Support role who can Mediate to see one ghost per use. Both the Medium and Ghost then have an arrow showing them where each other are at all times."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Mediate",
            "Communicate with the dead, which may lead you to the killers.",
            TouCrewAssets.MediateSprite)
    ];

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        MediatedPlayers.ForEach(mod => mod.Player?.GetModifierComponent()?.RemoveModifier(mod));
    }

    [MethodRpc((uint)TownOfUsRpc.Mediate, LocalHandling = RpcLocalHandling.Before, SendImmediately = true)]
    public static void RpcMediate(PlayerControl source, PlayerControl target)
    {
        if ((!source.AmOwner && !target.AmOwner) || (source.Data.Role is not MediumRole && !target.Data.IsDead))
        {
            return;
        }

        var modifier = new MediatedModifier(source.PlayerId);
        target.GetModifierComponent()?.AddModifier(modifier);
    }
}