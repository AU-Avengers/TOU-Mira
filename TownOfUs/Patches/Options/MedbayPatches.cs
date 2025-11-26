using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
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

    [HarmonyPatch]
    public static class MedscanWalkPadPatch
    {
        public static MethodBase TargetMethod()
        {
            return Helpers.GetStateMachineMoveNext<MedScanMinigame>(nameof(MedScanMinigame.WalkToPad))!;
        }

        public static bool Prefix(Il2CppObjectBase __instance)
        {
            if (OptionGroupSingleton<VanillaTweakOptions>.Instance.MedscanWalk.Value)
            {
                return true;
            }

            var wrapper = new StateMachineWrapper<MedScanMinigame>(__instance);
            MedScanMinigame medScanMinigame = wrapper.Instance;
            var num = medScanMinigame.state;
            var negative = -1;
            switch (num)
            {
                case MedScanMinigame.PositionState.None:
                    medScanMinigame.state = MedScanMinigame.PositionState.WalkingToPad;
                    break;
                case MedScanMinigame.PositionState.WalkingToPad:
                    medScanMinigame.state = MedScanMinigame.PositionState.WalkingToOffset;
                    break;
                case MedScanMinigame.PositionState.WalkingToOffset:
                    medScanMinigame.state = (MedScanMinigame.PositionState)negative;
                    medScanMinigame.walking = null;
                    break;
            }
            return false;
        }
    }
    [HarmonyPatch]
    public static class MedscanWalkOffsetPatch
    {
        public static MethodBase TargetMethod()
        {
            return Helpers.GetStateMachineMoveNext<MedScanMinigame>(nameof(MedScanMinigame.WalkToOffset))!;
        }

        public static bool Prefix(Il2CppObjectBase __instance)
        {
            if (OptionGroupSingleton<VanillaTweakOptions>.Instance.MedscanWalk.Value)
            {
                return true;
            }

            var wrapper = new StateMachineWrapper<MedScanMinigame>(__instance);
            MedScanMinigame medScanMinigame = wrapper.Instance;
            var num = medScanMinigame.state;
            var negative = -1;
            switch (num)
            {
                case MedScanMinigame.PositionState.None:
                    medScanMinigame.state = MedScanMinigame.PositionState.WalkingToPad;
                    break;
                case MedScanMinigame.PositionState.WalkingToPad:
                    medScanMinigame.state = MedScanMinigame.PositionState.WalkingToOffset;
                    break;
                case MedScanMinigame.PositionState.WalkingToOffset:
                    medScanMinigame.state = (MedScanMinigame.PositionState)negative;
                    medScanMinigame.walking = null;
                    break;
            }
            return false;
        }
    }
}