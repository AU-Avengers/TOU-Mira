using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class AmbusherRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SonarRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "Ambusher";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public static string PursuingString = MiraLocaleManager.Get("TouRoleAmbusherTabPursuingPlayer");

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;
    [HideFromIl2Cpp] public PlayerControl? Pursued { get; set; }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Ambusher.LoadAsset(), "TouMira.Role.Impostor.Ambusher", 1.45f),
        Icon = TouRoleIcons.Ambusher,
        IntroSound = TouAudio.SneakyIntro,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        CanUseVent = OptionGroupSingleton<AmbusherOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Pursue", "Pursue"),
                    MiraLocaleManager.Get($"TouRole{IdPart}PursueWikiDescription"),
                    TouImpAssets.PursueSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Ambush", "Ambush"),
                    MiraLocaleManager.Get($"TouRole{IdPart}AmbushWikiDescription"),
                    TouImpAssets.AmbushSprite)
            ];
        }
    }

    public void LobbyStart()
    {
        Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Pursued && Pursued != null)
        {
            stringB.Append(TownOfUsPlugin.Culture,
                $"\n<b>{PursuingString.Replace("<player>", $"{Pursued.Data.Color.ToTextColor()}{Pursued.Data.PlayerName}</color>")}</b>");
        }

        return stringB;
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        Clear();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        PursuingString = MiraLocaleManager.Get("TouRoleAmbusherTabPursuingPlayer");
        CustomButtonSingleton<AmbusherAmbushButton>.Instance.SetActive(false, this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        Clear();
    }

    public void Clear()
    {
        Pursued = null;
    }

    public void CheckDeadPursued()
    {
        if (Pursued != null && Pursued.HasDied())
        {
            Pursued = null;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.AmbushPlayer)]
    public static void RpcAmbushPlayer(PlayerControl ambusher, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(ambusher);
            return;
        }
        if (ambusher.Data.Role is not AmbusherRole)
        {
            Error("RpcAmbushPlayer - Invalid ambusher");
            return;
        }
        var murderResultFlags = MurderResultFlags.Succeeded;

        var beforeMurderEvent = new BeforeMurderEvent(ambusher, target, true);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            GameHistory.UpdatePlayerDeathData(target, MiraLocaleManager.Get("DiedToAmbusherAmbush"), 0, HudManagerHelper.Instance.CurrentRound,
            DeathHandlerOverride.SetTrue,
            MiraLocaleManager.Get("DiedByStringBasic").Replace("<player>", ambusher.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);
        }

        ambusher.CustomMurder(
            target,
            null,
            true,
            false,
            murderResultFlags2,
            true,
            true,
            false);
        ambusher.AddModifier<AmbusherConcealedModifier>(target);
    }
}