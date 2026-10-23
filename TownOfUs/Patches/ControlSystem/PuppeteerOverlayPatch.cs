using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Patches.ControlSystem;

public static class PuppeteerOverlayPatch
{
    public static void RunPuppetOverlay()
    {
        var local = PlayerControl.LocalPlayer;

        var hasModifier = local.TryGetModifier<PuppeteerControlModifier>(out var mod);
        var isControlled = PuppeteerRole.ControlState.IsControlled(local.PlayerId, out var controllerId);

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
                MeetingHud.Instance ||
                ExileController.Instance ||
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
                    PuppeteerRole.ControlState.ClearControl(local.PlayerId);
                    local.RemoveModifier(mod);
                }
            }
        }
    }
}