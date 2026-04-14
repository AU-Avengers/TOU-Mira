using HarmonyLib;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class VanillaSystemCheckPatches
{
    public static MushroomMixupSabotageSystem? ShroomSabotageSystem;
    public static HqHudSystemType? HqCommsSystem;
    public static HudOverrideSystemType? HudCommsSystem;
    public static VentilationSystem? VentSystem;

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    [HarmonyPostfix]
    public static void ShipStatusPostfix(ShipStatus __instance)
    {
        if (__instance.Systems.TryGetValue(SystemTypes.Ventilation, out var comms))
        {
            var ventilationSystem = comms.TryCast<VentilationSystem>();
            VentSystem = ventilationSystem;
        }

        if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var commsSystem))
        {
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq ||
                ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
            {
                var hqSystem = commsSystem.Cast<HqHudSystemType>();
                HqCommsSystem = hqSystem;
            }
            else
            {
                var hudSystem = commsSystem.Cast<HudOverrideSystemType>();
                HudCommsSystem = hudSystem;
            }
        }

        ShroomSabotageSystem = UnityEngine.Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        var foundVentSys = VentSystem != null;
        var foundHqSys = HqCommsSystem != null;
        var foundHudSys = HudCommsSystem != null;
        var foundMixUpSys = ShroomSabotageSystem != null;
        Warning(
            $"Found: {(foundMixUpSys ? "Mix-Up System" : "No Mix-Up System")}, {(foundVentSys ? "Vent System" : "No Vent System")}, {(foundHqSys ? "Hq Comms System" : "No Hq Comms System")}, {(foundHudSys ? "Hud Comms System" : "No Hud Comms System")}");
    }
}
