using System.Collections;
using AmongUs.Data;
using Assets.CoreScripts;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Roles;
using UnityEngine;
using Enum = System.Enum;
using Exception = System.Exception;
using Random = UnityEngine.Random;

namespace TownOfUs.Networking;

public static class CustomTouMurderRpcs
{
    public static float RecordedKillCooldown = -1f;

    /// <summary>
    /// Networked Custom Murder method. Use this if changing from a dictionary is needed.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="targets">The players to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShields">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    public static void RpcSpecialMultiMurder(
        this PlayerControl source,
        List<PlayerControl> targets,
        bool isIndirect = false,
        bool ignoreShields = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        var newTargets = targets.Select(x => new KeyValuePair<byte, string>(x.PlayerId, x.Data.PlayerName))
            .ToDictionary(x => x.Key, x => x.Value);
        RpcSpecialMultiMurder(source, newTargets, isIndirect, ignoreShields, didSucceed, resetKillTimer, createDeadBody,
            teleportMurderer, showKillAnim, playKillSound, causeOfDeath);
    }

    public static void RpcSpecialMultiMurder(
        this PlayerControl source,
        Dictionary<byte, string> targets,
        bool isIndirect = false,
        bool ignoreShields = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        RpcSpecialMultiMurder(source, targets, MeetingCheck.Ignore, isIndirect, ignoreShields, didSucceed,
            resetKillTimer, createDeadBody,
            teleportMurderer, showKillAnim, playKillSound, causeOfDeath);
    }

    /// <summary>
    /// Networked Custom Murder method. Use this if changing from a dictionary is needed.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="targets">The players to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShields">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    /// <param name="inMeeting">Should the murder only work in meetings.</param>
    public static void RpcSpecialMultiMurder(
        this PlayerControl source,
        List<PlayerControl> targets,
        MeetingCheck inMeeting,
        bool isIndirect = false,
        bool ignoreShields = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        var newTargets = targets.Select(x => new KeyValuePair<byte, string>(x.PlayerId, x.Data.PlayerName))
            .ToDictionary(x => x.Key, x => x.Value);
        RpcSpecialMultiMurder(source, newTargets, inMeeting, isIndirect, ignoreShields, didSucceed, resetKillTimer,
            createDeadBody,
            teleportMurderer, showKillAnim, playKillSound, causeOfDeath);
    }

    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="targets">The players to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShields">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    /// <param name="inMeeting">Should the murder only work in meetings.</param>
    [MethodRpc((uint)TownOfUsRpc.SpecialMultiMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSpecialMultiMurder(
        this PlayerControl source,
        Dictionary<byte, string> targets,
        MeetingCheck inMeeting,
        bool isIndirect = false,
        bool ignoreShields = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        Coroutines.Start(CoWaitForMultiIndirect(source, targets, inMeeting, isIndirect, ignoreShields, didSucceed,
            resetKillTimer, createDeadBody, teleportMurderer, showKillAnim, playKillSound, causeOfDeath));
    }

    public static IEnumerator CoWaitForMultiIndirect(
        PlayerControl source,
        Dictionary<byte, string> targets,
        MeetingCheck inMeeting,
        bool isIndirect = false,
        bool ignoreShields = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        // Wait for the modifier component to set up.
        if (isIndirect)
        {
            source.AddModifier<IndirectAttackerModifier>(ignoreShields);
            while (!source.HasModifier<IndirectAttackerModifier>())
            {
                yield return null;
            }
        }

        var firstTarget = true;
        var victims = new Dictionary<byte, string>();
        var survivors = new Dictionary<byte, string>();
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => targets.ContainsKey(x.PlayerId)).ToList();
        foreach (var target in allPlayers)
        {
            var beforeMurderEvent = new BeforeMurderEvent(source, target, inMeeting);
            MiraEventManager.InvokeEvent(beforeMurderEvent);
            var isMeetingActive = MeetingHud.Instance || ExileController.Instance;
            if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) ||
                (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive) ||
                target.ProtectedByGa())
            {
                beforeMurderEvent.Cancel();
            }

            var murderResultFlags = (didSucceed && !beforeMurderEvent.IsCancelled)
                ? MurderResultFlags.Succeeded
                : MurderResultFlags.FailedError;
            if (murderResultFlags is MurderResultFlags.FailedError)
            {
                survivors.Add(target.PlayerId, target.Data.PlayerName);
            }
            else
            {
                victims.Add(target.PlayerId, target.Data.PlayerName);
            }

            // Track kill cooldown before CustomMurder for Time Lord rewind (only for first target to avoid duplicates)
            RecordedKillCooldown = -1f;
            if (firstTarget && resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
            {
                RecordedKillCooldown = source.killTimer;
            }

            firstTarget = false;
        }

        if (victims.HasAny() && source.AmOwner)
        {
            source.isKilling = true;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            yield break;
        }

        if (survivors.Count == 0)
        {
            RpcConfirmSpecialMultiMurder(
                PlayerControl.LocalPlayer,
                source,
                victims,
                MurderResultFlags.Succeeded,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound,
                causeOfDeath);
        }
        else if (victims.Count == 0)
        {
            RpcConfirmSpecialMultiMurder(
                PlayerControl.LocalPlayer,
                source,
                survivors,
                MurderResultFlags.FailedError,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound,
                causeOfDeath);
        }
        else
        {
            RpcConfirmSpecialMultiMurderDouble(
                PlayerControl.LocalPlayer,
                source,
                victims,
                survivors,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound,
                causeOfDeath);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.ConfirmSpecialMultiMurderDouble, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmSpecialMultiMurderDouble(
        this PlayerControl host,
        PlayerControl source,
        Dictionary<byte, string> victims,
        Dictionary<byte, string> survivors,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            source.isKilling = false;
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (!host.IsHost())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        var murderResultFlagsGood = MurderResultFlags.DecisionByHost | MurderResultFlags.Succeeded;
        var murderResultFlagsBad = MurderResultFlags.DecisionByHost | MurderResultFlags.FailedError;

        var allVictims = PlayerControl.AllPlayerControls.ToArray().Where(x => victims.ContainsKey(x.PlayerId)).ToList();
        foreach (var target in allVictims)
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"),
                DeathEventHandlers.CurrentRound,
                (!MeetingHud.Instance && !ExileController.Instance)
                    ? DeathHandlerOverride.SetTrue
                    : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);

            source.CustomMurder(
                target,
                murderResultFlagsGood,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound);
        }

        var allSurvivors = PlayerControl.AllPlayerControls.ToArray().Where(x => survivors.ContainsKey(x.PlayerId))
            .ToList();
        foreach (var target in allSurvivors)
        {
            source.CustomMurder(
                target,
                murderResultFlagsBad,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound);
        }

        // Record kill cooldown change after CustomMurder if it was reset
        if (RecordedKillCooldown > -1f && resetKillTimer && source.AmOwner &&
            source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, RecordedKillCooldown));
        }
    }

    [MethodRpc((uint)TownOfUsRpc.ConfirmSpecialMultiMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmSpecialMultiMurder(
        this PlayerControl host,
        PlayerControl source,
        Dictionary<byte, string> targets,
        MurderResultFlags murderResultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            source.isKilling = false;
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (!host.IsHost())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => targets.ContainsKey(x.PlayerId)).ToList();
        foreach (var target in allPlayers)
        {
            if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
                murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
            {
                DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"),
                    DeathEventHandlers.CurrentRound,
                    (!MeetingHud.Instance && !ExileController.Instance)
                        ? DeathHandlerOverride.SetTrue
                        : DeathHandlerOverride.SetFalse,
                    TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                    lockInfo: DeathHandlerOverride.SetTrue);
            }

            source.CustomMurder(
                target,
                murderResultFlags2,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound);
        }

        // Record kill cooldown change after CustomMurder if it was reset
        if (RecordedKillCooldown > -1f && resetKillTimer && source.AmOwner &&
            source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, RecordedKillCooldown));
        }
    }

    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="framed">The player to frame.</param>
    /// <param name="ignoreShield">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    [MethodRpc((uint)TownOfUsRpc.FramedMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFramedMurder(
        this PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        bool ignoreShield = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        Coroutines.Start(CoWaitForFramedIndirect(source, target, framed, ignoreShield, didSucceed, resetKillTimer,
            createDeadBody, showKillAnim, playKillSound, causeOfDeath));
    }

    public static IEnumerator CoWaitForFramedIndirect(
        PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        bool ignoreShield = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        // Wait for the modifier component to set up.
        source.AddModifier<IndirectAttackerModifier>(ignoreShield);
        while (!source.HasModifier<IndirectAttackerModifier>())
        {
            yield return null;
        }

        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, MeetingCheck.OutsideMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        var isMeetingActive = MeetingHud.Instance || ExileController.Instance;
        if (isMeetingActive)
        {
            beforeMurderEvent.Cancel();
        }

        if (target.ProtectedByGa())
        {
            beforeMurderEvent.Cancel();
            murderResultFlags = MurderResultFlags.FailedProtected;
        }
        else if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }

        // Track kill cooldown before CustomMurder for Time Lord rewind
        RecordedKillCooldown = -1f;
        if (resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            RecordedKillCooldown = source.killTimer;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            yield break;
        }

        RpcConfirmFramedMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            framed,
            murderResultFlags,
            resetKillTimer,
            createDeadBody,
            showKillAnim,
            playKillSound,
            causeOfDeath);
    }

    [MethodRpc((uint)TownOfUsRpc.ConfirmFramedMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmFramedMurder(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        MurderResultFlags murderResultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            source.isKilling = false;
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (!host.IsHost() || target.HasDied())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"),
                DeathEventHandlers.CurrentRound,
                (!MeetingHud.Instance && !ExileController.Instance)
                    ? DeathHandlerOverride.SetTrue
                    : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            false,
            showKillAnim,
            playKillSound);

        Coroutines.Start(CoWaitForJump(murderResultFlags2, framed, target));

        // Record kill cooldown change after CustomMurder if it was reset
        if (RecordedKillCooldown > -1f && resetKillTimer && source.AmOwner &&
            source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, RecordedKillCooldown));
        }
    }

    public static IEnumerator CoWaitForJump(MurderResultFlags flags, PlayerControl framed, PlayerControl target)
    {
        // Wait for CustomMurder to process and get the proper position
        yield return null;
        yield return null;

        var targetPos = target.transform.position;
        if (flags.HasFlag(MurderResultFlags.Succeeded))
        {
            MiscUtils.LungeToPos(framed, targetPos);
        }
    }

    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="framed">The player that will be shown on the sumamry. If they are the same player as the target, then the cause of death will be basic.</param>
    /// <param name="ignoreShield">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    [MethodRpc((uint)TownOfUsRpc.SelfMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSelfMurder(
        this PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        bool ignoreShield = true,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        Coroutines.Start(CoWaitForSelfIndirect(source, target, framed, ignoreShield, didSucceed, resetKillTimer,
            createDeadBody, showKillAnim, playKillSound, causeOfDeath));
    }

    public static IEnumerator CoWaitForSelfIndirect(
        PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        bool ignoreShield = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        // Wait for the modifier component to set up.
        source.AddModifier<IndirectAttackerModifier>(ignoreShield);
        while (!source.HasModifier<IndirectAttackerModifier>())
        {
            yield return null;
        }

        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, MeetingCheck.OutsideMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        var isMeetingActive = MeetingHud.Instance || ExileController.Instance;
        if (isMeetingActive)
        {
            beforeMurderEvent.Cancel();
        }

        if (target.ProtectedByGa())
        {
            beforeMurderEvent.Cancel();
            murderResultFlags = MurderResultFlags.FailedProtected;
        }
        else if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }

        // Track kill cooldown before CustomMurder for Time Lord rewind
        RecordedKillCooldown = -1f;
        if (resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            RecordedKillCooldown = source.killTimer;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            yield break;
        }

        RpcConfirmSelfMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            framed,
            murderResultFlags,
            resetKillTimer,
            createDeadBody,
            showKillAnim,
            playKillSound,
            causeOfDeath);

        // Record kill cooldown change after CustomMurder if it was reset
        if (RecordedKillCooldown > -1f && resetKillTimer && source.AmOwner &&
            source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, RecordedKillCooldown));
        }
    }

    [MethodRpc((uint)TownOfUsRpc.ConfirmSelfMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmSelfMurder(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        MurderResultFlags murderResultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            source.isKilling = false;
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (!host.IsHost() || target.HasDied())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"),
                DeathEventHandlers.CurrentRound,
                (!MeetingHud.Instance && !ExileController.Instance)
                    ? DeathHandlerOverride.SetTrue
                    : DeathHandlerOverride.SetFalse,
                framed != target ? TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", framed.Data.PlayerName) : "",
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            false,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShield">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    public static void RpcSpecialMurder(
        this PlayerControl source,
        PlayerControl target,
        bool isIndirect = false,
        bool ignoreShield = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        RpcSpecialMurder(source, target, MeetingCheck.Ignore, isIndirect, ignoreShield, didSucceed, resetKillTimer,
            createDeadBody,
            teleportMurderer, showKillAnim, playKillSound, causeOfDeath);
    }

    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShield">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    /// <param name="inMeeting">Should the murder only work in meetings.</param>
    [MethodRpc((uint)TownOfUsRpc.SpecialMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSpecialMurder(
        this PlayerControl source,
        PlayerControl target,
        MeetingCheck inMeeting,
        bool isIndirect = false,
        bool ignoreShield = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        Coroutines.Start(CoWaitForIndirect(source, target, inMeeting, isIndirect, ignoreShield, didSucceed,
            resetKillTimer, createDeadBody, teleportMurderer, showKillAnim, playKillSound, causeOfDeath));
    }

    public static IEnumerator CoWaitForIndirect(
        PlayerControl source,
        PlayerControl target,
        MeetingCheck inMeeting,
        bool isIndirect = false,
        bool ignoreShield = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        // Wait for the modifier component to set up.
        if (isIndirect)
        {
            source.AddModifier<IndirectAttackerModifier>(ignoreShield);
            while (!source.HasModifier<IndirectAttackerModifier>())
            {
                yield return null;
            }
        }

        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, inMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        var isMeetingActive = MeetingHud.Instance || ExileController.Instance;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) ||
            (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (target.ProtectedByGa())
        {
            beforeMurderEvent.Cancel();
            murderResultFlags = MurderResultFlags.FailedProtected;
        }
        else if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }

        // Track kill cooldown before CustomMurder for Time Lord rewind
        RecordedKillCooldown = -1f;
        if (resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            RecordedKillCooldown = source.killTimer;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            yield break;
        }

        RpcConfirmSpecialMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            murderResultFlags,
            isIndirect,
            ignoreShield,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound,
            causeOfDeath);
    }

    [MethodRpc((uint)TownOfUsRpc.ConfirmSpecialMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmSpecialMurder(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        MurderResultFlags murderResultFlags,
        bool isIndirect = false,
        bool ignoreShield = false,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            source.isKilling = false;
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (!host.IsHost() || target.HasDied())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"),
                DeathEventHandlers.CurrentRound,
                (!MeetingHud.Instance && !ExileController.Instance)
                    ? DeathHandlerOverride.SetTrue
                    : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);

        // Record kill cooldown change after CustomMurder if it was reset
        if (RecordedKillCooldown > -1f && resetKillTimer && source.AmOwner &&
            source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, RecordedKillCooldown));
        }
    }

    public static IEnumerator CoRecordKillCooldownAfterCustomMurder(PlayerControl player, float cooldownBefore)
    {
        // Wait for CustomMurder to process and SetKillTimer to be called
        yield return null;
        yield return null;

        var cooldownAfter = player.killTimer;
        if (Mathf.Abs(cooldownBefore - cooldownAfter) > 0.01f)
        {
            TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordKillCooldown(player, cooldownBefore, cooldownAfter);
        }

        RecordedKillCooldown = -1f;
    }

    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    [MethodRpc((uint)TownOfUsRpc.GhostRoleMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcGhostRoleMurder(
        this PlayerControl source,
        PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (!source.HasDied() || target.HasDied())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();
        if (source.Data.Role is IGhostRole)
        {
            role = source.Data.Role;
        }

        if (role is not ITownOfUsRole touRole || (touRole.RoleAlignment is not RoleAlignment.NeutralAfterlife && touRole.RoleAlignment is not RoleAlignment.NeutralEvil))
        {
            return;
        }

        source.AddModifier<IndirectAttackerModifier>(true);

        var cod = "Killer";
        if (touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"),
            DeathEventHandlers.CurrentRound,
            DeathHandlerOverride.SetTrue,
            TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue);
        DeathHandlerModifier.UpdateDeathHandlerImmediate(source, "null", -1, DeathHandlerOverride.SetFalse,
            lockInfo: DeathHandlerOverride.SetTrue);
        source.CustomMurder(
            target,
            MurderResultFlags.Succeeded);
    }

    public static int GetRandomMeetingAnim(DeathAnimType type)
    {
        switch (type)
        {
            case DeathAnimType.Nameplate:
                var randomKey = NameplateDeathAnimations.RandomSnapshot().Key;
                return Array.FindIndex(NameplateDeathAnimations, x => x.Key == randomKey);
            case DeathAnimType.Overlay:
                return Random.RandomRangeInt(0, (int)KillAnimType.ViperKill);
        }
        return -1;
    }

    public static readonly KeyValuePair<LoadableAsset<AnimationClip>, LoadableAsset<AnimationClip>>[] NameplateDeathAnimations =
    [
        new(TouAssets.MeetingDeathBloodAnim1, TouAssets.MeetingDeathAnim1),
        new(TouAssets.MeetingDeathBloodAnim2, TouAssets.MeetingDeathAnim2),
        new(TouAssets.MeetingDeathBloodAnim3, TouAssets.MeetingDeathAnim3),
        new(TouAssets.MeetingDeathBloodAnim4, TouAssets.MeetingDeathAnim4)
     ];

    /// <summary>
    /// Networked Custom Murder method for meetings.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    /// <param name="animationType">The type of animation that should play upon death.</param>
    /// <param name="associatedAnimation">The id of whatever animation is to be loaded. If not valid, the first one will be used instead, or will be left unused if not required</param>
    [MethodRpc((uint)TownOfUsRpc.MeetingMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcMeetingMurder(
        this PlayerControl source,
        PlayerControl target,
        MeetingAnimation animationType,
        int associatedAnimation = -1,
        bool didSucceed = true,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, MeetingCheck.ForMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        if (!MeetingHud.Instance && !ExileController.Instance)
        {
            beforeMurderEvent.Cancel();
        }

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }
        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }

        RpcConfirmMeetingMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            murderResultFlags,
            animationType,
            associatedAnimation,
            causeOfDeath);
    }

    [MethodRpc((uint)TownOfUsRpc.ConfirmMeetingMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmMeetingMurder(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        MurderResultFlags murderResultFlags,
        MeetingAnimation animationType,
        int associatedAnimation = -1,
        string causeOfDeath = "null")
    {
        if (LobbyBehaviour.Instance)
        {
            source.isKilling = false;
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (!host.IsHost() || target.HasDied())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"),
                DeathEventHandlers.CurrentRound,
                (!MeetingHud.Instance && !ExileController.Instance)
                    ? DeathHandlerOverride.SetTrue
                    : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        source.MeetingMurder(
            target,
            murderResultFlags2,
            animationType,
            associatedAnimation);
    }
    /// <summary>
    /// Custom Murder method without networking. If you need a networked version, use <see cref="RpcMeetingMurder(PlayerControl, PlayerControl, MeetingAnimation, int, bool, string)"/>.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="resultFlags">Murder result flags.</param>
    /// <param name="animationType">The type of animation that should play upon death.</param>
    /// <param name="associatedAnimation">The id of whatever animation is to be loaded. If not valid, the first one will be used instead, or will be left unused if not required</param>
    public static void MeetingMurder(
        this PlayerControl source,
        PlayerControl target,
        MurderResultFlags resultFlags,
        MeetingAnimation animationType,
        int associatedAnimation)
    {
        source.isKilling = false;
        if (resultFlags.HasFlag(MurderResultFlags.FailedError) || LobbyBehaviour.Instance)
        {
            return;
        }

        if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
            !resultFlags.HasFlag(MurderResultFlags.Succeeded) &&
            !resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
        {
            return;
        }

        DebugAnalytics.Instance.Analytics.Kill(target.Data, source.Data);
        if (source.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(
                GameManager.Instance.IsHideAndSeek()
                    ? StatID.HideAndSeek_ImpostorKills
                    : StatID.ImpostorKills);
        }

        UnityTelemetry.Instance.WriteMurder();
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");

        var properAnim = (ExtendedKillAnimType)associatedAnimation;
        var targetVoteArea = MeetingHud.Instance?.playerStates.First(x => x.TargetPlayerId == target.PlayerId);
        switch (animationType)
        {
            case MeetingAnimation.FullscreenKill:
                if (!Enum.IsDefined(properAnim))
                {
                    properAnim = ExtendedKillAnimType.None;
                }
                try
                {
                    HudManager.Instance.KillOverlay.TriggerKillAnimation(source.Data, target.Data, properAnim);
                }
                catch (Exception e)
                {
                    Error($"Error with kill animation: {e.ToString()}");
                }
                if (targetVoteArea != null)
                {
                    EliminatePlayer(targetVoteArea);
                }
                break;
            case MeetingAnimation.FullscreenSelfKill:
                if (!Enum.IsDefined(properAnim))
                {
                    properAnim = ExtendedKillAnimType.None;
                }
                try
                {
                    HudManager.Instance.KillOverlay.TriggerKillAnimation(target.Data, target.Data, properAnim);
                }
                catch (Exception e)
                {
                    Error($"Error with kill animation: {e.ToString()}");
                }
                if (targetVoteArea != null)
                {
                    EliminatePlayer(targetVoteArea);
                }
                break;
            case MeetingAnimation.PlayerNameplateAnimation:

                if (targetVoteArea == null)
                {
                    break;
                }
                Coroutines.Start(CoAnimateDeath(targetVoteArea, associatedAnimation));
                break;
            case MeetingAnimation.RoleSpecific:
                if (targetVoteArea != null && source.Data.Role is IMeetingKiller role)
                {
                    role.TriggerMeetingAnimation(source, target, targetVoteArea, associatedAnimation);
                }
                break;
        }
        if (target.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                    // ignored
                }
            }

            target.cosmetics.SetNameMask(false);
            target.RpcSetScanner(false);
        }

        AchievementManager.Instance.OnMurder(
            source.AmOwner,
            target.AmOwner,
            source.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
            source.shapeshiftTargetPlayerId,
            target.PlayerId);
        source.MyPhysics.StartCoroutine(CoPerformMeetingKill(source, target));
    }

    public static OverlayKillAnimation[] StoredKillAnimations = [];
    public static void TriggerKillAnimation(this KillOverlay overlay, NetworkedPlayerInfo killer,
        NetworkedPlayerInfo victim, ExtendedKillAnimType animType)
    {
        OverlayKillAnimation killAnimation;
        if (!StoredKillAnimations.HasAny())
        {
            OverlayKillAnimation[] self = overlay.KillAnims;
            StoredKillAnimations = self.AddRangeToArray(overlay.CustomKillAnimations);
        }
        if (animType is ExtendedKillAnimType.HorseWrangler)
        {
            killAnimation = overlay.HorseWrangleAnims.Random()!;
            overlay.ShowKillAnimation(killAnimation, killer, victim);
            return;
        }
        killAnimation = StoredKillAnimations.FirstOrDefault(x => x.KillType == (KillAnimType)animType) ?? StoredKillAnimations[0];
        overlay.ShowKillAnimation(killAnimation, killer, victim);
    }

    public static void EliminatePlayer(PlayerVoteArea voteArea)
    {
        voteArea.PlayerIcon?.gameObject?.SetActive(false);
        voteArea.Overlay?.gameObject?.SetActive(true);
        voteArea.XMark?.gameObject?.SetActive(false);
    }
    /// <summary>
    /// Perform a custom kill animation.
    /// </summary>
    /// <param name="source">The murderer.</param>
    /// <param name="target">The murdered player.</param>
    /// <returns>Coroutine.</returns>
    public static IEnumerator CoPerformMeetingKill(
        PlayerControl source,
        PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            yield break;
        }
        var cam = Camera.main?.GetComponent<FollowerCamera>();
        var isParticipant = source.AmOwner || target.AmOwner;

        KillAnimation.SetMovement(target, false);

        if (isParticipant)
        {
            PlayerControl.LocalPlayer.isKilling = true;
            source.isKilling = true;
        }

        source.Data.Role.KillAnimSpecialSetup(null, source, target);
        target.Data.Role.KillAnimSpecialSetup(null, source, target);

        // no idea if this causes bugs, but innersloth is brain-dead
        // I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF
        PlayerControl.LocalPlayer.Data.Role.KillAnimSpecialSetup(null, source, target);

        if (isParticipant)
        {
            if (cam != null)
            {
                cam.Locked = true;
            }

            ConsoleJoystick.SetMode_Task();
            if (PlayerControl.LocalPlayer.AmOwner)
            {
                PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
            }
        }

        target.Die(DeathReason.Kill, true);

        KillAnimation.SetMovement(target, true);

        var afterMurderEvent = new AfterMurderEvent(source, target, null);
        MiraEventManager.InvokeEvent(afterMurderEvent);

        if (!isParticipant)
        {
            yield break;
        }

        if (cam != null)
        {
            cam.Locked = false;
        }

        PlayerControl.LocalPlayer.isKilling = false;
        source.isKilling = false;
    }
    public static IEnumerator CoAnimateDeath(PlayerVoteArea voteArea, int id, bool instant = false)
    {
        voteArea.Overlay.gameObject.SetActive(false);
        voteArea.XMark.gameObject.SetActive(false);
        voteArea.XMark.transform.localScale = Vector3.one;
        var trueAnim = id != -1
            ? NameplateDeathAnimations[id]
            : NameplateDeathAnimations[GetRandomMeetingAnim(DeathAnimType.Nameplate)];
        var animation = UnityEngine.Object.Instantiate(TouAssets.MeetingDeathPrefab.LoadAsset(), voteArea.transform);
        animation.transform.localPosition = new Vector3(-0.8f, 0, 0);
        animation.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        animation.gameObject.layer = animation.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = animation.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        var r = animationRend.gameObject.GetComponent<RainbowBehaviour>()
             ?? animationRend.gameObject.AddComponent<RainbowBehaviour>();
        r.AddRend(animationRend, voteArea.PlayerIcon.ColorId);

        voteArea.Overlay.gameObject.SetActive(false);
        animation.gameObject.SetActive(false);

        Coroutines.Start(MiscUtils.CoFlash(Palette.ImpostorRed, 0.5f, 0.15f));
        var seconds = Random.RandomRange(0.4f, 1.1f);
        // if there's less than 6 players alive, animation will play instantly
        if (Helpers.GetAlivePlayers().Count <= 5 || instant)
        {
            seconds = 0.01f;
        }

        yield return new WaitForSeconds(seconds);

        voteArea.PlayerIcon.gameObject.SetActive(false);
        animation.gameObject.SetActive(true);
        var bodysAnim = animation.GetComponent<SpriteAnim>();

        var bloodAnim = animation.transform.GetChild(0).GetComponent<SpriteAnim>();

        bloodAnim.Play(trueAnim.Key.LoadAsset(), 1.05f);
        bodysAnim.Play(trueAnim.Value.LoadAsset(), 1.05f);
        var bodyAnimLength = bodysAnim.m_currAnim.length;
        var isRhm = (bloodAnim.Clip == TouAssets.MeetingDeathBloodAnim4.LoadAsset());

        if (isRhm)
        {
            SoundManager.Instance.PlaySound(TouAudio.LaserKillSound.LoadAsset(), false);
            yield return new WaitForSeconds(bodyAnimLength);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            SoundManager.Instance.PlaySound(voteArea.GetPlayer()!.KillSfx, false);
            yield return new WaitForSeconds(bodyAnimLength - 0.25f);
        }

        // For some reason this can just fail? I don't get it either, fails getting the GameObject the component is attached to.
        try
        {
            voteArea.Overlay.gameObject.SetActive(true);
        }
        catch
        {
            // ignored
        }
        animation.Destroy();
        // For some reason this can just fail? I don't get it either, fails getting the GameObject the component is attached to.
        try
        {
            voteArea.XMark.gameObject.SetActive(true);
            Coroutines.Start(MiscUtils.BetterBloop(voteArea.XMark.transform));
        }
        catch
        {
            // ignored
        }
        SoundManager.Instance.PlaySound(MeetingHud.Instance.MeetingIntro.PlayerDeadSound, false);
    }
}

public enum MeetingAnimation
{
    None,
    RoleSpecific,
    PlayerNameplateAnimation,
    FullscreenKill,
    FullscreenSelfKill,
}
public enum DeathAnimType
{
    Nameplate,
    Overlay
}
public enum ExtendedKillAnimType
{
    Stab = 0,
    Tongue = 1,
    Shoot = 2,
    Neck = 3,
    RHM = 4,
    Werewolf_Slash = 5,
    ViperKill = 6,
    HorseWrangler = 9998,
    None = 9999,
}
