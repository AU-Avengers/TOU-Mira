using AmongUs.GameOptions;
using HarmonyLib;
using TownOfUs.Events.Crewmate;
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
            if (GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) <= 0f)
            {
                return false;
            }

            // Record kill cooldown change for Time Lord rewind
            var cdDefault = PlayerControl.LocalPlayer.GetKillCooldown();
            var cooldownBefore = __instance.killTimer;
            var maxvalue = time > cdDefault
                ? time + 1f
                : cdDefault;
            var cooldownAfter = Mathf.Clamp(time, 0, maxvalue);
            
            // Only record if the cooldown actually changed
            if (Mathf.Abs(cooldownBefore - cooldownAfter) > 0.01f)
            {
                TimeLordEventHandlers.RecordKillCooldown(__instance, cooldownBefore, cooldownAfter);
            }
            
            __instance.killTimer = cooldownAfter;
            if (HudManager.InstanceExists && HudManager.Instance.KillButton != null)
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