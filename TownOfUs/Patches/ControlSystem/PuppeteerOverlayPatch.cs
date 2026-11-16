using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;

namespace TownOfUs.Patches.ControlSystem;

public static class PuppeteerOverlayPatch
{
    public static void RunPuppetOverlay()
    {
        var local = PlayerControl.LocalPlayer;

        if (!local.TryGetModifier<PuppeteerControlModifier>(out var mod))
        {
            return;
        }

        var isControlled = PuppeteerControlState.IsControlled(local.PlayerId, out var controllerId);

        if (!isControlled)
        {
            mod.ClearNotification();
            mod.RemoveSelf();
            return;
        }

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
            mod.ClearNotification();
            PuppeteerControlState.ClearControl(local.PlayerId);
            mod.RemoveSelf();
        }
    }
}