using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;

namespace TownOfUs.Patches.Roles;

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

        if (local.TryGetModifier<ParasiteInfectedModifier>(out var mod))
        {
            mod.UpdateOverlayLayout();
        }
    }
}