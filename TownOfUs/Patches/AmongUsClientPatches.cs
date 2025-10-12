using HarmonyLib;
using Reactor.Utilities;
using TownOfUs.Modules.Components;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class AmongUsClientPatches
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    [HarmonyPostfix]
    public static void StartPatch(AmongUsClient __instance)
    {
        if (AmongUsClient.Instance != __instance)
        {
            Logger<TownOfUsPlugin>.Error("AmongUsClient duplicate detected.");
        }

        SystemTypeHelpers.AllTypes = SystemTypeHelpers.AllTypes.Concat([(SystemTypes)HexBombSabotageSystem.SabotageId]).ToArray();
    }
}