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
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Medium,
        OptionsScreenshot = TouCrewAssets.MediumRoleBanner,
        IntroSound = TouAudio.MediumIntroSound
    };

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        MediatedPlayers.ForEach(mod => mod.Player?.GetModifierComponent()?.RemoveModifier(mod));
        if (Spirit == null) return;
        Spirit.DestroyImmediate();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        MediatedPlayers.ForEach(mod => mod.Player?.GetModifierComponent()?.RemoveModifier(mod));
        if (Spirit == null) return;
        Spirit.DestroyImmediate();
    }

    public MedSpiritObject? Spirit { get; set; }

    [MethodRpc((uint)TownOfUsRpc.Mediate)]
    public static void RpcMediate(PlayerControl player)
    {
        var hidden =
            (AppearanceVisibility)OptionGroupSingleton<MediumOptions>.Instance.PlayerVisibility.Value is
            AppearanceVisibility.None or AppearanceVisibility.Ghosts;
        if (player.AmOwner && hidden)
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

    public static void RpcMultiMediate(
        PlayerControl source,
        List<PlayerControl> targets)
    {
        var newTargets = targets.Count == 0
            ? new Dictionary<byte, string>()
            : targets.Select(x => new KeyValuePair<byte, string>(x.PlayerId, x.Data.PlayerName))
                .ToDictionary(x => x.Key, x => x.Value);
        RpcMultiMediate(source, newTargets);
    }

    [MethodRpc((uint)TownOfUsRpc.MultiMediate)]
    public static void RpcMultiMediate(PlayerControl player, Dictionary<byte, string> targets)
    {
        if (targets.Count != 0)
        {
            var allPlayers = PlayerControl.AllPlayerControls.ToArray().ToList();
            allPlayers.Remove(player);
            foreach (var target in targets)
            {
                var newPlayer =
                    allPlayers.FirstOrDefault(x => x.PlayerId == target.Key || x.Data.PlayerName == target.Value);
                if (newPlayer == null)
                {
                    continue;
                }

                allPlayers.Remove(newPlayer);
                if (player.AmOwner || newPlayer.AmOwner)
                {
                    var modifier = new MediatedModifier(player.PlayerId);
                    newPlayer.GetModifierComponent()?.AddModifier(modifier);
                }
            }
        }

        var hidden =
            (AppearanceVisibility)OptionGroupSingleton<MediumOptions>.Instance.PlayerVisibility.Value is
            AppearanceVisibility.None or AppearanceVisibility.Ghosts;
        if (player.AmOwner && hidden)
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
}
