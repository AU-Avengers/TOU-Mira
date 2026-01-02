using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class ParasiteOverlayPatch
{
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        if (local.Data?.Role is ParasiteRole parasiteRole && parasiteRole.Controlled != null)
        {
            parasiteRole.TickPiP();
        }

        if (local.TryGetModifier<ParasiteInfectedModifier>(out var mod))
        {
            var shouldClear =
                MeetingHud.Instance != null ||
                ExileController.Instance != null ||
                local.Data == null ||
                local.Data.Disconnected ||
                local.Data.IsDead;

            if (!shouldClear)
            {
                if (!ParasiteControlState.IsControlled(local.PlayerId, out var controllerId))
                {
                    shouldClear = true;
                }
                else
                {
                    var controller = MiscUtils.PlayerById(controllerId);
                    if (controller == null || controller.Data == null || controller.Data.Disconnected || controller.HasDied())
                    {
                        shouldClear = true;
                    }
                }
            }

            if (shouldClear)
            {
                mod.ClearNotification();
                ParasiteControlState.ClearControl(local.PlayerId);
                local.RemoveModifier(mod);
                return;
            }

            mod.UpdateOverlayLayout();
        }
    }
}