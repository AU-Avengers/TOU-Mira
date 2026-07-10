using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;

namespace TownOfUs.Patches.Options;

[HarmonyPatch]
public static class ExileImpostorCountPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.HandleText))]
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.HandleText))]
    [HarmonyPostfix]
    public static void HideImpostorCountPostfix(ExileController __instance)
    {
        if (!OptionGroupSingleton<VanillaTweakOptions>.Instance.HideRemainingImpostorCount.Value)
        {
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.LogicOptions == null ||
            !GameManager.Instance.LogicOptions.GetConfirmImpostor())
        {
            return;
        }

        if (__instance.ImpostorText != null)
        {
            __instance.ImpostorText.text = string.Empty;
            __instance.ImpostorText.gameObject.SetActive(false);
        }
    }
}
