using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class PuppeteerOverlayPatch
{
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        var hasModifier = local.TryGetModifier<PuppeteerControlModifier>(out var mod);
        var isControlled = PuppeteerControlState.IsControlled(local.PlayerId, out var controllerId);

        if (hasModifier && !isControlled)
        {
            mod?.ClearNotification();
            if (mod != null)
             local.RemoveModifier(mod);
            return;
        }

        if (hasModifier)
        {
            var shouldClear =
                MeetingHud.Instance != null ||
                ExileController.Instance != null ||
                local.Data == null ||
                local.Data.Disconnected ||
                local.Data.IsDead;

            if (!shouldClear)
            {
                var controller = MiscUtils.PlayerById(controllerId);
                if (controller == null || controller.Data == null || controller.Data.Disconnected || controller.HasDied())
                {
                    shouldClear = true;
                }
            }

            if (shouldClear)
            {
                mod?.ClearNotification();
                if (mod != null)
                {
                    PuppeteerControlState.ClearControl(local.PlayerId);
                    local.RemoveModifier(mod);
                }
            }
        }
    }
}