using HarmonyLib;
using MiraAPI.Events;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class PlayerRevivePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    public static void Postfix(PlayerControl __instance)
    {
        // Ensure collider is enabled after revive to prevent walking through doors
        if (__instance.Collider != null)
        {
            __instance.Collider.enabled = true;
        }
        
        // Ensure player is moveable after revive
        __instance.moveable = true;

        var reviveEvent = new PlayerReviveEvent(__instance);
        MiraEventManager.InvokeEvent(reviveEvent);
    }
}