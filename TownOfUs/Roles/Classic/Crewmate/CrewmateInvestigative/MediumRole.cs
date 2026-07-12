using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.MedSpirit;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MediumRole : CrewmateRole, ITownOfUsRole, IWikiDiscoverable, IDoomable, IRewindImmune
{
    public override bool IsAffectedByComms => false;
    public bool IgnoredByRewind => false;
    public bool IgnoredByRecording => Spirit != null;
    public List<MediatedModifier> MediatedPlayers { get; } = [];
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

    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Mediate", "Mediate"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}MediateWikiDescription"),
                    TouCrewAssets.MediateSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Medium;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Medium.LoadAsset(), "TouMira.Role.Crewmate.Medium", 1.45f),
        Icon = TouRoleIcons.Medium,
        OptionsScreenshot = TouBanners.MediumRoleBanner,
        IntroSound = TouAudio.MediumIntroSound
    };

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        MediatedPlayers.ForEach(mod => mod.Player.RemoveModifier(mod));
        if (Spirit == null) return;
        Spirit.DestroyImmediate();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        MediatedPlayers.ForEach(mod => mod.Player.RemoveModifier(mod));
        if (Spirit == null) return;
        Spirit.DestroyImmediate();
    }

    public MedSpiritObject? Spirit { get; set; }

    [MethodRpc((uint)TownOfUsRpc.Mediate)]
    public static void RpcMediate(PlayerControl player)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
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

    public static void RpcMultiMediate(
        PlayerControl source,
        List<PlayerControl> targets)
    {
        var newTargets = targets.Count == 0
            ? []
            : targets.Select(x => new KeyValuePair<byte, string>(x.PlayerId, x.Data.PlayerName))
                .ToDictionary(x => x.Key, x => x.Value);
        RpcMultiMediate(source, newTargets);
    }

    [MethodRpc((uint)TownOfUsRpc.MultiMediate)]
    public static void RpcMultiMediate(PlayerControl player, Dictionary<byte, string> targets)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (AmongUsClient.Instance.AmHost)
        {
            var spirit = Instantiate(TouAssets.MediumSpirit.LoadAsset()).GetComponent<MedSpiritObject>();
            AmongUsClient.Instance.Spawn(spirit, player.OwnerId);
        }
        if (targets.Count != 0)
        {
            AmongUsClient.Instance.StartCoroutine(CoShowGhosts(player, targets));
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
    }

    public static IEnumerator CoShowGhosts(PlayerControl player, Dictionary<byte, string> targets)
    {
        // This must be a coroutine for it to show the arrow to everyone besides the host.
        yield return new WaitForSeconds(0.5f);
        var allPlayers = PlayerControl.AllPlayerControls.ToList();
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
                newPlayer.AddModifier<MediatedModifier>(player.PlayerId);
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.RemoveMediumSpirit)]
    public static void RpcRemoveMediumSpirit(PlayerControl medium, MedSpiritObject spirit)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(medium);
            return;
        }
        spirit.StartCoroutine(spirit.CoDestroy());
    }
}