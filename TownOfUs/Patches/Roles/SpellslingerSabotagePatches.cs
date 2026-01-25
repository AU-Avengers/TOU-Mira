using HarmonyLib;
using Hazel;
using MiraAPI.GameOptions;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class SpellslingerSabotagePatches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnEnable))]
    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.OnEnable))]
    [HarmonyPatch(typeof(FungleShipStatus), nameof(FungleShipStatus.OnEnable))]
    [HarmonyPostfix]
    public static void AddCustomSabotageSystems(ShipStatus __instance)
    {
        if (!__instance.Systems.TryGetValue((SystemTypes)HexBombSabotageSystem.SabotageId, out _))
        {
            var hexBombSabo = new HexBombSabotageSystem(OptionGroupSingleton<SpellslingerOptions>.Instance.HexBombDuration);
            __instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>().specials
                .Add(hexBombSabo.Cast<IActivatable>());
            __instance.Systems.Add((SystemTypes)HexBombSabotageSystem.SabotageId, hexBombSabo.Cast<ISystemType>());
        }
    }

    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
    [HarmonyPostfix]
    public static void UpdateSystemPatch([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader reader)
    {
        var amount = reader.Buffer[reader.readHead - 1];

        if (AmongUsClient.Instance.AmHost && MeetingHud.Instance == null && ExileController.Instance == null &&
            amount == HexBombSabotageSystem.SabotageId)
        {
            ShipStatus.Instance.UpdateSystem((SystemTypes)HexBombSabotageSystem.SabotageId, player, 1);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AddSystemTask))]
    [HarmonyPrefix]
    public static bool AddSaboTask(PlayerControl __instance, ref SystemTypes system, ref PlayerTask __result)
    {
        if (!__instance.AmOwner) return true;

        if (system == (SystemTypes)HexBombSabotageSystem.SabotageId)
        {
            var task = new GameObject("HexBombTask").AddComponent<HexBombSabotageTask>();
            task.gameObject.transform.SetParent(__instance.gameObject.transform);
            task.Id = 255U;
            task.Owner = __instance;
            task.Initialize();
            __instance.myTasks.Add(task);
            __result = task;
            return false;
        }
        return true;
    }
}