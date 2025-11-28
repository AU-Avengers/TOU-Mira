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
    public string LocaleKey => "Medium";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Mediate", "Mediate"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}MediateWikiDescription"),
                    TouCrewAssets.MediateSprite)
            };
        }
    }

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

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        MediatedPlayers.ForEach(mod => mod.Player?.GetModifierComponent()?.RemoveModifier(mod));
    }

    [MethodRpc((uint)TownOfUsRpc.Mediate, LocalHandling = RpcLocalHandling.Before)]
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