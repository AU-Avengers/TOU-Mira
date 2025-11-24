using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.HnsGame.Crewmate;
using TownOfUs.Options;

namespace TownOfUs.Patches.Options;

[HarmonyPatch(typeof(MedScanMinigame))]
public static class MedScanMinigameFixedUpdatePatch
{
    [HarmonyPatch(nameof(MedScanMinigame.FixedUpdate))]
    public static void Prefix(MedScanMinigame __instance)
    {
        if (OptionGroupSingleton<VanillaTweakOptions>.Instance.ParallelMedbay)
        {
            // Allows multiple medbay scans at once
            __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
            __instance.medscan.UsersList.Clear();
        }
    }

    [HarmonyPatch(nameof(MedScanMinigame.Begin))]
    public static void Postfix(MedScanMinigame __instance)
    {
        if (PlayerControl.LocalPlayer.HasModifier<GiantModifier>() || PlayerControl.LocalPlayer.HasModifier<HnsGiantModifier>())
        {
            __instance.completeString = __instance.completeString.Replace("3' 6\"", "5' 3\"").Replace("92lb", "184lb");
        }
        else if (PlayerControl.LocalPlayer.HasModifier<MiniModifier>() || PlayerControl.LocalPlayer.HasModifier<HnsMiniModifier>())
        {
            __instance.completeString = __instance.completeString.Replace("3' 6\"", "1' 9\"").Replace("92lb", "46lb");
        }
    }
}