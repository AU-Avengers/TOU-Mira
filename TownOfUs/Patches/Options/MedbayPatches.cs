using System.Collections;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.HnsGame.Crewmate;
using TownOfUs.Options;

namespace TownOfUs.Patches.Options;

[HarmonyPatch(typeof(MedScanMinigame))]
public static class MedScanMinigameFixedUpdatePatch
{
    [HarmonyPatch(nameof(MedScanMinigame.FixedUpdate))]
    [HarmonyPrefix]
    public static void MedscanUpdatePrefix(MedScanMinigame __instance)
    {
        if (OptionGroupSingleton<VanillaTweakOptions>.Instance.ParallelMedbay.Value)
        {
            // Allows multiple medbay scans at once
            __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
            __instance.medscan.UsersList.Clear();
        }
    }

    [HarmonyPatch(nameof(MedScanMinigame.Begin))]
    [HarmonyPostfix]
    public static void MedscanBeginPostfix(MedScanMinigame __instance)
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

    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.WalkToPad))]
    public static class MedscanWalkPadPatch
    {
        public static bool Prefix(MedScanMinigame __instance, ref IEnumerator __result)
        {
            if (OptionGroupSingleton<VanillaTweakOptions>.Instance.MedscanWalk.Value)
            {
                return true;
            }

            __result = Helpers.CreateWrapper(__result, () => 
            {
                var num = __instance.state;
                var negative = -1;
                switch (num)
                {
                    case MedScanMinigame.PositionState.None:
                        __instance.state = MedScanMinigame.PositionState.WalkingToPad;
                        break;
                    case MedScanMinigame.PositionState.WalkingToPad:
                        __instance.state = MedScanMinigame.PositionState.WalkingToOffset;
                        break;
                    case MedScanMinigame.PositionState.WalkingToOffset:
                        __instance.state = (MedScanMinigame.PositionState)negative;
                        __instance.walking = null;
                        break;
                }
            });
            return false;
        }
    }
    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.WalkToOffset))]
    public static class MedscanWalkOffsetPatch
    {
        public static bool Prefix(MedScanMinigame __instance, ref IEnumerator __result)
        {
            if (OptionGroupSingleton<VanillaTweakOptions>.Instance.MedscanWalk.Value)
            {
                return true;
            }

            __result = Helpers.CreateWrapper(__result, () => 
            {
                var num = __instance.state;
                var negative = -1;
                switch (num)
                {
                    case MedScanMinigame.PositionState.None:
                        __instance.state = MedScanMinigame.PositionState.WalkingToPad;
                        break;
                    case MedScanMinigame.PositionState.WalkingToPad:
                        __instance.state = MedScanMinigame.PositionState.WalkingToOffset;
                        break;
                    case MedScanMinigame.PositionState.WalkingToOffset:
                        __instance.state = (MedScanMinigame.PositionState)negative;
                        __instance.walking = null;
                        break;
                }
            });
            return false;
        }
    }
}