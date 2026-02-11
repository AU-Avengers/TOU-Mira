using System.Collections;
using HarmonyLib;
using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Networking;

/// <summary>
/// Handles synchronization of player death states across all clients to prevent desync issues. (feel free to remove this if you find the root cause, but this is a bandaid fix for now)
/// </summary>
public static class DeathStateSync
{
    private static readonly Dictionary<byte, float> LastDeathSyncTime = new();
    private static readonly Dictionary<byte, bool> PendingDeathSyncs = new();
    private const float DeathSyncCooldown = 0.5f;
    private const float SyncDelayAfterMurder = 0.1f;

    /// <summary>
    /// Force-syncs the death state of a player across all clients.
    /// Should be called after a kill to ensure all clients have the correct state.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.SyncDeathState, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSyncDeathState(PlayerControl target, bool isDead)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        if (target == null || target.Data == null)
        {
            return;
        }

        if (target.Data.Disconnected)
        {
            return;
        }

        if (target.Data.IsDead == isDead)
        {
            return;
        }

        if (target.Data.Role is IGhostRole ghostRole && ghostRole.GhostActive && !isDead)
        {
            return;
        }

        target.Data.IsDead = isDead;

        if (isDead && !target.HasModifier<DeathHandlerModifier>())
        {
            var deathHandler = new DeathHandlerModifier();
            target.AddModifier(deathHandler);
        }

        PendingDeathSyncs.Remove(target.PlayerId);
    }

    /// <summary>
    /// Requests death state validation from the host.
    /// Non-host clients can call this if they detect a desync.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.RequestDeathStateValidation, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRequestDeathStateValidation(PlayerControl requester)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        if (requester == null || requester.Data == null)
        {
            return;
        }

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        Coroutines.Start(CoValidateAndCorrectDeathStates());
    }

    /// <summary>
    /// Validates death states for all players and corrects any desyncs.
    /// Only runs on host.
    /// </summary>
    private static IEnumerator CoValidateAndCorrectDeathStates()
    {
        yield return new WaitForSeconds(0.1f);

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            yield break;
        }

        var corrections = new List<(byte playerId, bool shouldBeDead)>();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Disconnected)
            {
                continue;
            }

            if (player.Data.Role is IGhostRole ghostRole && ghostRole.GhostActive)
            {
                continue;
            }

            var hostIsDead = player.Data.IsDead;

            if (LastDeathSyncTime.TryGetValue(player.PlayerId, out var lastSync) && Time.time - lastSync < DeathSyncCooldown)
            {
                continue;
            }

            corrections.Add((player.PlayerId, hostIsDead));
        }

        foreach (var (playerId, shouldBeDead) in corrections)
        {
            var player = MiscUtils.PlayerById(playerId);
            if (player != null && player.Data != null && !player.Data.Disconnected)
            {
                if (shouldBeDead && !player.HasDied())
                {
                    continue;
                }

                if (PendingDeathSyncs.TryGetValue(playerId, out var pendingState) && !pendingState)
                {
                    continue;
                }

                RpcSyncDeathState(player, shouldBeDead);
                LastDeathSyncTime[playerId] = Time.time;
            }
        }
    }

    /// <summary>
    /// Schedules a death state sync after a murder.
    /// This ensures the death state is synchronized after CustomMurder completes.
    /// </summary>
    public static void ScheduleDeathStateSync(PlayerControl target, bool isDead)
    {
        if (target == null || target.Data == null)
        {
            return;
        }

        if (PendingDeathSyncs.TryGetValue(target.PlayerId, out var pendingState))
        {
            if (pendingState == isDead)
            {
                return;
            }
            if (!pendingState && isDead)
            {
                // Cancel pending alive sync, proceed with dead sync
            }
            else
            {
                PendingDeathSyncs.Remove(target.PlayerId);
                return;
            }
        }

        if (LastDeathSyncTime.TryGetValue(target.PlayerId, out var lastSync) && Time.time - lastSync < DeathSyncCooldown && target.Data.IsDead == isDead)
        {
            return;
        }

        PendingDeathSyncs[target.PlayerId] = isDead;
        Coroutines.Start(CoDelayedDeathStateSync(target, isDead));
    }

    /// <summary>
    /// Cancels any pending death sync for a player (e.g., when they're revived).
    /// </summary>
    public static void CancelPendingDeathSync(byte playerId)
    {
        PendingDeathSyncs.Remove(playerId);
    }

    private static IEnumerator CoDelayedDeathStateSync(PlayerControl target, bool isDead)
    {
        yield return new WaitForSeconds(SyncDelayAfterMurder);

        if (target.Data == null || target.Data.Disconnected)
        {
            PendingDeathSyncs.Remove(target.PlayerId);
            yield break;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            PendingDeathSyncs.Remove(target.PlayerId);
            yield break;
        }

        if (!PendingDeathSyncs.TryGetValue(target.PlayerId, out var pendingState) || pendingState != isDead)
        {
            PendingDeathSyncs.Remove(target.PlayerId);
            yield break;
        }

        if (isDead && !target.HasDied())
        {
            PendingDeathSyncs.Remove(target.PlayerId);
            yield break;
        }

        if (target.Data.IsDead != isDead && AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            RpcSyncDeathState(target, isDead);
        }

        LastDeathSyncTime[target.PlayerId] = Time.time;
        PendingDeathSyncs.Remove(target.PlayerId);
    }

    /// <summary>
    /// Requests validation after a kill to ensure death state is synchronized.
    /// Called by clients after they perform a kill.
    /// </summary>
    public static void RequestValidationAfterKill(PlayerControl killer)
    {
        if (killer == null || killer.Data == null || killer.Data.Disconnected)
        {
            return;
        }

        if (AmongUsClient.Instance == null || 
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            return;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            Coroutines.Start(CoValidateAndCorrectDeathStates());
        }
        else
        {
            Coroutines.Start(CoDelayedValidationRequest(killer));
        }
    }

    /// <summary>
    /// Clears sync state when a player leaves or disconnects.
    /// </summary>
    public static void ClearPlayerSyncState(byte playerId)
    {
        LastDeathSyncTime.Remove(playerId);
        PendingDeathSyncs.Remove(playerId);
    }

    /// <summary>
    /// Clears all sync state (e.g., on game end).
    /// </summary>
    public static void ClearAllSyncState()
    {
        LastDeathSyncTime.Clear();
        PendingDeathSyncs.Clear();
    }

    /// <summary>
    /// Delays validation request to prevent spam from multiple clients.
    /// </summary>
    private static IEnumerator CoDelayedValidationRequest(PlayerControl requester)
    {
        // Small random delay (0.1-0.3s) to prevent all clients from requesting at once
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));
        
        if (requester != null && requester.Data != null && !requester.Data.Disconnected)
        {
            RpcRequestDeathStateValidation(requester);
        }
    }
}

/// <summary>
/// Patches to integrate death state synchronization into the game loop.
/// </summary>
[HarmonyPatch]
public static class DeathStateSyncPatches
{
    /// <summary>
    /// Clear sync state when player leaves.
    /// </summary>
    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        if (@event.ClientData?.Character != null)
        {
            DeathStateSync.ClearPlayerSyncState(@event.ClientData.Character.PlayerId);
        }
    }

    /// <summary>
    /// Handle revive events - ensure death state is properly synced after revive.
    /// </summary>
    [RegisterEvent]
    public static void PlayerReviveEventHandler(PlayerReviveEvent @event)
    {
        var player = @event.Player;
        if (player == null || player.Data == null)
        {
            return;
        }

        DeathStateSync.CancelPendingDeathSync(player.PlayerId);

        Coroutines.Start(CoSyncAfterRevive(player));
    }

    private static IEnumerator CoSyncAfterRevive(PlayerControl player)
    {
        yield return new WaitForSeconds(0.15f);

        if (player == null || player.Data == null || player.Data.Disconnected)
        {
            yield break;
        }

        if (player.Data.IsDead && !player.HasDied() && AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            DeathStateSync.ScheduleDeathStateSync(player, false);
        }
    }
}