using HarmonyLib;

namespace TownOfUs.Patches.AprilFools;
/// <remakrs>
/// Taken from <see href="https://github.com/Tommy-XL/Unlock-dlekS-ehT/blob/main/Patches/AutoSelectDleksPatch.cs"/>
/// </remakrs>
[HarmonyPatch(typeof(StringOption), nameof(StringOption.Start))]
public static class DleksClampPatch
{
    [HarmonyPostfix]
    private static void Postfix(StringOption __instance)
    {
        if (__instance.Title == StringNames.GameMapName)
        {
            // vanilla clamps this to not auto select dlekS
            __instance.Value = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        }
    }
}