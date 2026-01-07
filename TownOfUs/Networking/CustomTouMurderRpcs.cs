using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
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
        var newTargets = targets.Select(x => new KeyValuePair<byte, string>(x.PlayerId, x.Data.PlayerName)).ToDictionary(x => x.Key, x => x.Value);
        RpcSpecialMultiMurder(source, newTargets, isIndirect, ignoreShields, didSucceed, resetKillTimer, createDeadBody,
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
    [MethodRpc((uint)TownOfUsRpc.SpecialMultiMurder, LocalHandling = RpcLocalHandling.Before)]
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
        var role = source.GetRoleWhenAlive();
        IndirectAttackerModifier? attackerMod = null;
        if (isIndirect)
        {
            attackerMod = source.AddModifier<IndirectAttackerModifier>(ignoreShields);
        }

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var firstTarget = true;
        foreach (var target in targets)
        {
            PlayerControl? newPlayer = null;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.Data == null)
                {
                    continue;
                }

                if (pc.PlayerId == target.Key || pc.Data.PlayerName == target.Value)
                {
                    newPlayer = pc;
                    break;
                }
            }
            if (newPlayer == null)
            {
                continue;
            }
            var beforeMurderEvent = new BeforeMurderEvent(source, newPlayer);
            MiraEventManager.InvokeEvent(beforeMurderEvent);

            if (beforeMurderEvent.IsCancelled)
            {
                murderResultFlags = MurderResultFlags.FailedError;
            }

            var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

            if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
                murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
            {
                DeathHandlerModifier.UpdateDeathHandlerImmediate(newPlayer, TouLocale.Get($"DiedTo{cod}"), DeathEventHandlers.CurrentRound,
                    (MeetingHud.Instance == null && ExileController.Instance == null) ? DeathHandlerOverride.SetTrue : DeathHandlerOverride.SetFalse,
                    TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data?.PlayerName),
                    lockInfo: DeathHandlerOverride.SetTrue);
            }

            // Track kill cooldown before CustomMurder for Time Lord rewind (only for first target to avoid duplicates)
            float? killCooldownBefore = null;
            if (firstTarget && resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
            {
                killCooldownBefore = source.killTimer;
            }

            source.CustomMurder(
                newPlayer,
                murderResultFlags2,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound);

            // Record kill cooldown change after CustomMurder if it was reset (only for first target)
            if (killCooldownBefore.HasValue && firstTarget && resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
            {
                Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, killCooldownBefore.Value));
            }

            firstTarget = false;
        }
        if (attackerMod != null)
        {
            Coroutines.Start(CoRemoveIndirect(source));
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
        var role = source.GetRoleWhenAlive();
        var attackerMod = source.AddModifier<IndirectAttackerModifier>(ignoreShield);

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"), DeathEventHandlers.CurrentRound,
                (MeetingHud.Instance == null && ExileController.Instance == null) ? DeathHandlerOverride.SetTrue : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        // Track kill cooldown before CustomMurder for Time Lord rewind
        float? killCooldownBefore = null;
        if (resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            killCooldownBefore = source.killTimer;
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
        if (target.HasDied())
        {
            MiscUtils.LungeToPos(framed, targetPos);
        }

        // Record kill cooldown change after CustomMurder if it was reset
        if (killCooldownBefore.HasValue && resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, killCooldownBefore.Value));
        }

        if (attackerMod != null)
        {
            Coroutines.Start(CoRemoveIndirect(source));
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
    [MethodRpc((uint)TownOfUsRpc.SpecialMurder, LocalHandling = RpcLocalHandling.Before)]
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
        var role = source.GetRoleWhenAlive();
        IndirectAttackerModifier? attackerMod = null;
        if (isIndirect)
        {
            attackerMod = source.AddModifier<IndirectAttackerModifier>(ignoreShield);
        }

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"), DeathEventHandlers.CurrentRound,
                (MeetingHud.Instance == null && ExileController.Instance == null) ? DeathHandlerOverride.SetTrue : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        // Track kill cooldown before CustomMurder for Time Lord rewind
        float? killCooldownBefore = null;
        if (resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            killCooldownBefore = source.killTimer;
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
        if (killCooldownBefore.HasValue && resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CoRecordKillCooldownAfterCustomMurder(source, killCooldownBefore.Value));
        }

        if (attackerMod != null)
        {
            Coroutines.Start(CoRemoveIndirect(source));
        }
    }

    private static IEnumerator CoRecordKillCooldownAfterCustomMurder(PlayerControl player, float cooldownBefore)
    {
        // Wait for CustomMurder to process and SetKillTimer to be called
        yield return null;
        yield return null;
        
        var cooldownAfter = player.killTimer;
        if (Mathf.Abs(cooldownBefore - cooldownAfter) > 0.01f)
        {
            TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordKillCooldown(player, cooldownBefore, cooldownAfter);
        }
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

        DeathHandlerModifier.UpdateDeathHandlerImmediate(target, TouLocale.Get($"DiedTo{cod}"), DeathEventHandlers.CurrentRound,
            DeathHandlerOverride.SetTrue,
            TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue);
        DeathHandlerModifier.UpdateDeathHandlerImmediate(source, "null", -1, DeathHandlerOverride.SetFalse,
            lockInfo: DeathHandlerOverride.SetTrue);
        source.CustomMurder(
            target,
            MurderResultFlags.Succeeded);

        Coroutines.Start(CoRemoveIndirect(source));
    }

    public static IEnumerator CoRemoveIndirect(PlayerControl source)
    {
        yield return new WaitForEndOfFrame();
        if (source.TryGetModifier<IndirectAttackerModifier>(out var indirectMod))
        {
            source.RemoveModifier(indirectMod);
        }
    }
}