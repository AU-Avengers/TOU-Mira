using HarmonyLib;
using TownOfUs.Events.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class ImpostorKillTimerPatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    [HarmonyPrefix]
    public static bool SetKillTimerPatch(PlayerControl __instance, ref float time)
    {
        if (MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek)
        {
            return true;
        }
        if (__instance.Data?.Role?.CanUseKillButton == true)
        {
            if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown <= 0f)
            {
                return false;
            }

            // Record kill cooldown change for Time Lord rewind
            var cooldownBefore = __instance.killTimer;
            var maxvalue = time > GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown
                ? time + 1f
                : GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
            var cooldownAfter = Mathf.Clamp(time, 0, maxvalue);
            
            // Only record if the cooldown actually changed
            if (Mathf.Abs(cooldownBefore - cooldownAfter) > 0.01f)
            {
                TimeLordEventHandlers.RecordKillCooldown(__instance, cooldownBefore, cooldownAfter);
            }
            
            __instance.killTimer = cooldownAfter;
            if (HudManager.Instance != null && HudManager.Instance.KillButton != null)
            {
                HudManager.Instance.KillButton.SetCoolDown(__instance.killTimer, maxvalue);
            }
        }
        else
        {
            // If CanUseKillButton is false, let the original method run
            return true;
        }

        return false;
    }
}