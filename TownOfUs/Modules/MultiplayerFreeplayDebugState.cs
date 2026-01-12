using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using TownOfUs.Networking;
using TownOfUs.Patches;
using TownOfUs.Patches.Options;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modules;

/// <summary>
/// Stores the round's initial roles/modifiers so Multiplayer Freeplay can "Reset" back to a baseline.
/// </summary>
public static class MultiplayerFreeplayDebugState
{
    private sealed record BaselineSnapshot(ushort RoleType, List<Type> ModifierTypes, bool WasDead, Vector2 Position);

    private static readonly Dictionary<byte, BaselineSnapshot> Baseline = new();
    private static bool _captured;

    public static void CaptureBaselineIfNeeded()
    {
        if (_captured || !MultiplayerFreeplayMode.Enabled)
        {
            return;
        }

        Baseline.Clear();

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.Data?.Role == null)
            {
                continue;
            }

            var roleType = (ushort)player.Data.Role.Role;
            var mods = player.GetModifiers<BaseModifier>().Select(m => m.GetType()).Distinct().ToList();
            var pos = player.transform.position;
            Baseline[player.PlayerId] = new BaselineSnapshot(roleType, mods, player.Data.IsDead, pos);
        }

        _captured = true;
    }

    public static void RestoreBaseline()
    {
        CaptureBaselineIfNeeded();

        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        // Clear global trackers that often affect "sandbox" testing.
        GameHistory.ClearAll();
        FirstDeadPatch.PlayerNames = [];
        FirstDeadPatch.FirstRoundPlayerNames = [];

        TeamChatPatches.TeamChatActive = false;
        TeamChatPatches.ForceReset = true;
        TeamChatPatches.ForceNormalChat();

        // Remove any lingering bodies.
        foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            try { UnityEngine.Object.Destroy(body.gameObject); } catch { /* ignored */ }
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.Data?.Role == null)
            {
                continue;
            }

            // Remove all modifiers (best effort).
            var modComp = player.GetModifierComponent();
            if (modComp != null)
            {
                foreach (var mod in player.GetModifiers<BaseModifier>().ToList())
                {
                    modComp.RemoveModifier(mod);
                }
            }

            if (!Baseline.TryGetValue(player.PlayerId, out var baseline))
            {
                continue;
            }

            // Revive/dead state restore.
            if (baseline.WasDead)
            {
                // If baseline was dead, keep them dead.
                if (!player.Data.IsDead)
                {
                    player.RpcCustomMurder(player);
                }
            }
            else
            {
                if (player.Data.IsDead)
                {
                    GameHistory.ClearMurder(player);
                    player.Revive();
                    DeathStateSync.RpcSyncDeathState(player, false);
                }
            }

            player.RpcChangeRole(baseline.RoleType);
            player.RpcSetPos(baseline.Position);

            // Restore baseline modifiers that have parameterless ctors (best effort).
            if (modComp != null)
            {
                foreach (var modType in baseline.ModifierTypes)
                {
                    if (modType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        continue;
                    }

                    if (player.GetModifiers<BaseModifier>().Any(x => x.GetType() == modType))
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(modType) is BaseModifier instance)
                    {
                        modComp.AddModifier(instance);
                    }
                }
            }

            player.ResetAppearance(override_checks: true, fullReset: true);
        }
    }

    public static void ResetCapturedBaseline()
    {
        _captured = false;
        Baseline.Clear();
    }
}