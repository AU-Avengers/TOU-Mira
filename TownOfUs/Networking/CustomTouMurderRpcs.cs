using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

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
            MiscUtils.RunKillWarning(source);
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
            yield return null;
            yield return null;
        }

        var firstTarget = true;
        var victims = new Dictionary<byte, string>();
        var survivors = new Dictionary<byte, string>();
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => targets.ContainsKey(x.PlayerId)).ToList();
        foreach (var target in allPlayers)
        {
            var beforeMurderEvent = new BeforeMurderEvent(source, target, inMeeting);
            MiraEventManager.InvokeEvent(beforeMurderEvent);
            var isMeetingActive = MeetingHud.Instance != null || ExileController.Instance != null;
            if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) ||
                (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
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
            MiscUtils.RunKillWarning(source);
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
                (MeetingHud.Instance == null && ExileController.Instance == null)
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
            MiscUtils.RunKillWarning(source);
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
                    (MeetingHud.Instance == null && ExileController.Instance == null)
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
            MiscUtils.RunKillWarning(source);
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
        yield return null;
        yield return null;

        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, MeetingCheck.OutsideMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
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
            MiscUtils.RunKillWarning(source);
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
                (MeetingHud.Instance == null && ExileController.Instance == null)
                    ? DeathHandlerOverride.SetTrue
                    : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        var targetPos = target.GetTruePosition();
        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            false,
            showKillAnim,
            playKillSound);
        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded))
        {
            MiscUtils.LungeToPos(framed, targetPos);
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
            MiscUtils.RunKillWarning(source);
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
            yield return null;
            yield return null;
        }

        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, inMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        var isMeetingActive = MeetingHud.Instance != null || ExileController.Instance != null;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) ||
            (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
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
            MiscUtils.RunKillWarning(source);
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
                (MeetingHud.Instance == null && ExileController.Instance == null)
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
            MiscUtils.RunKillWarning(source);
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

        var touRole = role as ITownOfUsRole;
        if (touRole == null || touRole.RoleAlignment is not RoleAlignment.NeutralEvil)
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
}
