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
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class AmbusherRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SonarRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Ambusher";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public static string PursuingString = TouLocale.GetParsed("TouRoleAmbusherTabPursuingPlayer");

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;
    [HideFromIl2Cpp] public PlayerControl? Pursued { get; set; }

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Ambusher,
        CanUseVent = OptionGroupSingleton<AmbusherOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Pursue", "Pursue"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}PursueWikiDescription"),
                    TouImpAssets.PursueSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Ambush", "Ambush"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}AmbushWikiDescription"),
                    TouImpAssets.AmbushSprite)
            };
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
        PursuingString = TouLocale.GetParsed("TouRoleAmbusherTabPursuingPlayer");
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

        ambusher.AddModifier<IndirectAttackerModifier>(false);

        var murderResultFlags = MurderResultFlags.Succeeded;

        var beforeMurderEvent = new BeforeMurderEvent(ambusher, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get("DiedToAmbusherAmbush"),
                DeathEventHandlers.CurrentRound,
                DeathHandlerOverride.SetTrue,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", ambusher.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        ambusher.CustomMurder(
            target,
            murderResultFlags2,
            true,
            true,
            false);
        ambusher.AddModifier<AmbusherConcealedModifier>(target);
    }
}