using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Patches.ControlSystem;

public static class ParasiteOverlayPatch
{
    public static void RunOvertakeOverlay()
    {
        var local = PlayerControl.LocalPlayer;

        if (local.Data?.Role is ParasiteRole { Controlled: not null } parasiteRole)
        {
            parasiteRole.TickPiP();
        }

        if (local.TryGetModifier<ParasiteInfectedModifier>(out var mod))
        {
            var shouldClear =
                MeetingHud.Instance ||
                ExileController.Instance ||
                local.Data == null ||
                local.Data.Disconnected ||
                local.Data.IsDead;

            if (!shouldClear)
            {
                if (!ParasiteRole.ControlState.IsControlled(local.PlayerId, out var controllerId))
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
                ParasiteRole.ControlState.ClearControl(local.PlayerId);
                local.RemoveModifier(mod);
                return;
            }

            mod.UpdateOverlayLayout();
        }
    }
}