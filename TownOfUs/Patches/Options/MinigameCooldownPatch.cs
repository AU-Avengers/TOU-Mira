using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Patches.Options;

[HarmonyPatch]
public static class MinigameCooldownPatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPrefix]
    public static void FreezeKillCooldownInMinigame(PlayerControl __instance)
    {
        if (!__instance.AmOwner)
            return;

        if (!OptionGroupSingleton<VanillaTweakOptions>.Instance.CanPauseCooldown)
            return;

        if (__instance.Data?.Role?.CanUseKillButton != true)
            return;

        __instance.killTimer += Time.fixedDeltaTime;
    }
}