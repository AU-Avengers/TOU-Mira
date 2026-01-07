using MiraAPI.Modifiers;
using TownOfUs.Patches;
using TownOfUs.Patches.Options;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modules;

/// <summary>
/// Stores original roles/modifiers for Freeplay (Tutorial) so debug actions can be reset cleanly.
/// </summary>
public static class FreeplayDebugState
{
    private sealed record BaselineSnapshot(ushort RoleType, List<Type> ModifierTypes);

    private static readonly Dictionary<byte, BaselineSnapshot> Baseline = new();
    private static bool _captured;

    public static void CaptureBaselineIfNeeded()
    {
        if (_captured || !TutorialManager.InstanceExists)
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
            Baseline[player.PlayerId] = new BaselineSnapshot(roleType, mods);
        }

        _captured = true;
    }

    public static void RestoreBaseline()
    {
        CaptureBaselineIfNeeded();

        // Clear global trackers that tend to affect Freeplay debug runs.
        GameHistory.ClearAll();
        FirstDeadPatch.PlayerNames = [];
        FirstDeadPatch.FirstRoundPlayerNames = [];

        // Ensure chat UI is not left in a custom state.
        TeamChatPatches.TeamChatActive = false;
        TeamChatPatches.ForceReset = true;
        TeamChatPatches.ForceNormalChat();

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

            // Restore role.
            if (Baseline.TryGetValue(player.PlayerId, out var baseline))
            {
                player.RpcChangeRole(baseline.RoleType);

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
            }

            // Clear cosmetic/appearance overrides from debug actions.
            player.ResetAppearance(override_checks: true, fullReset: true);
        }
    }
}


