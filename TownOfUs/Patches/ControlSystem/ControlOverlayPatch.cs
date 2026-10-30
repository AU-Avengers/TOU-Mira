using MiraAPI.Modifiers;
using TownOfUs.Interfaces;
using TownOfUs.Utilities.ControlSystem;

namespace TownOfUs.Patches.ControlSystem;

public static class ControlOverlayPatch
{
    public static void RunControlOverlay()
    {
        var local = PlayerControl.LocalPlayer;

        if (local.GetRemoteControl<IController>() is { Controlled: not null } control)
        {
            control.UpdateOverlay();
        }

        if (!local.TryGetModifierOfType<IControlledModifier>(out var mod))
        {
            return;
        }

        if (!mod.State.IsControlled(local.PlayerId, out var controllerId))
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
            if (controller == null || controller.Data == null || controller.Data.Disconnected || 
                (mod.ControllerIsDead is { } isDead && controller.HasDied() != isDead))
            {
                shouldClear = true;
            }
        }

        if (shouldClear)
        {
            mod.ClearNotification();
            mod.State.ClearControl(local.PlayerId);
            mod.RemoveSelf();
            return;
        }

        mod.UpdateOverlayLayout();
    }
}
