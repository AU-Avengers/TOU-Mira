using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities.Appearances;
using static ExileController;
using Object = Il2CppSystem.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class OracleExilePatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.HandleText))]
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.HandleText))]
    [HarmonyPrefix]
    public static void TranslationControllerGetStringPostfix(ExileController __instance)
    {
        if (__instance.initData.networkedPlayer != null)
        {
            return;
        }

        var blessedPlayers = new List<string>();

        foreach (var mod in ModifierUtils.GetActiveModifiers<OracleBlessedModifier>())
        {
            if (!mod.SavedFromExile)
                continue;

            mod.SavedFromExile = false;
            blessedPlayers.Add(mod.Player.GetDefaultAppearance().PlayerName);
        }

        if (blessedPlayers.Count == 1)
        {
            __instance.completeString = $"{blessedPlayers[0]} was blessed by an Oracle!";
        }
        else if (blessedPlayers.Count > 1)
        {
            var allButLast = string.Join(", ", blessedPlayers.Take(blessedPlayers.Count - 1));
            var last = blessedPlayers[^1];
            __instance.completeString = $"{allButLast}, and {last} were blessed by an Oracle!";
        }
    }
}