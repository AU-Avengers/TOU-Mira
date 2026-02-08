using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.MedSpirit;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MediumRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public List<MediatedModifier> MediatedPlayers { get; } = new();

    public DoomableType DoomHintType => DoomableType.Death;
    public static bool IsReworked => OptionGroupSingleton<MediumOptions>.Instance.ReworkToggle.Value;
    public static string ReworkString => IsReworked ? "Alt" : string.Empty;
    public string LocaleKey => "Medium";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}WikiDescription") +
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
                    TouLocale.GetParsed($"TouRole{LocaleKey}Mediate{ReworkString}WikiDescription"),
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

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        MediatedPlayers.ForEach(mod => mod.Player?.GetModifierComponent()?.RemoveModifier(mod));
        if (!Spirit) return;
        Spirit!.StartCoroutine(Spirit.CoDestroy().WrapToIl2Cpp());
    }
    
    public MedSpiritObject? Spirit { get; set; }

    [MethodRpc((uint)TownOfUsRpc.CreateMediumSpirit)]
    public static void RpcCreateMediumSpirit(PlayerControl player)
    {
        if (player.AmOwner && OptionGroupSingleton<MediumOptions>.Instance.HidePlayersWhileMediating.Value)
        {
            foreach (var plr in Helpers.GetAlivePlayers())
            {
                if (plr.AmOwner)
                {
                    continue;
                }

                plr.AddModifier<MediumHiddenModifier>();
            }
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var spirit = Instantiate(TouAssets.MediumSpirit.LoadAsset()).GetComponent<MedSpiritObject>();
        AmongUsClient.Instance.Spawn(spirit, player.OwnerId);
    }

    [MethodRpc((uint)TownOfUsRpc.RemoveMediumSpirit)]
    public static void RpcRemoveMediumSpirit(PlayerControl medium, MedSpiritObject spirit)
    {
        spirit.StartCoroutine(spirit.CoDestroy().WrapToIl2Cpp());
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