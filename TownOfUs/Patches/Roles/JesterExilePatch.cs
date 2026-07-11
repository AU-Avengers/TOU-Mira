using HarmonyLib;
using TownOfUs.Modules;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class JesterExilePatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.HandleText))]
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.HandleText))]
    [HarmonyPostfix]
    public static void JesterEmphasisPostfix(ExileController __instance)
    {
        var exiled = __instance.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.LogicOptions == null ||
            !GameManager.Instance.LogicOptions.GetConfirmImpostor())
        {
            return;
        }

        if (exiled.GetRoleWhenAlive() is not JesterRole)
        {
            return;
        }

        var text = __instance.completeString;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var lastDot = text.LastIndexOf('.');
        if (lastDot >= 0)
        {
            __instance.completeString = text[..lastDot] + "!" + text[(lastDot + 1)..];
        }
    }
}
