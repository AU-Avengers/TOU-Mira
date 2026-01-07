
using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Modifiers;

[HarmonyPatch]
public static class CrewpostorSabotagePatch
{
    [HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetMapOptions))]
    [HarmonyPriority(Priority.Low)] // make sure it occurs after other patches
    [HarmonyPrefix]
    public static bool GetMapOptions(ref MapOptions __result)
    {
        if (!OptionGroupSingleton<CrewpostorOptions>.Instance.CanAlwaysSabotage.Value)
        {
            return true;
        }
        __result = new MapOptions
        {
            Mode = ((PlayerControl.LocalPlayer.IsImpostorAligned() && !MeetingHud.Instance)
                ? MapOptions.Modes.Sabotage
                : MapOptions.Modes.Normal)
        };
        return false;
    }
}