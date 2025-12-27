using HarmonyLib;
using MiraAPI.Events;
using TownOfUs.Events.TouEvents;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class PlayerRevivePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    public static void Postfix(PlayerControl __instance)
    {
        // Ensure collider is enabled after revive to prevent walking through walls
        if (__instance.Collider != null)
        {
            __instance.Collider.enabled = true;
        }
        
        // Ensure player is moveable after revive
        __instance.moveable = true;

        // Reset physics state to ensure proper collision detection
        if (__instance.MyPhysics != null)
        {
            __instance.MyPhysics.ResetMoveState();
            
            // Sync physics body position with transform to ensure collision works correctly
            if (__instance.MyPhysics.body != null)
            {
                __instance.MyPhysics.body.position = __instance.transform.position;
                __instance.MyPhysics.body.velocity = Vector2.zero;
            }
        }

        // Sync Unity's physics system with transform changes
        Physics2D.SyncTransforms();

        var reviveEvent = new PlayerReviveEvent(__instance);
        MiraEventManager.InvokeEvent(reviveEvent);
    }
}