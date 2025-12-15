using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Maps;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.BetterMaps;

[HarmonyPatch(typeof(ShipStatus))]
public static class AirshipLadderCooldownPatch
{
    private static void ApplyLadderCooldown()
    {
        if (!AirshipLadderCooldownUtils.IsAirship())
        {
            return;
        }

        if (!OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            return;
        }

        var ladders = Object.FindObjectsOfType<Ladder>();
        
        foreach (var ladder in ladders)
        {
            if (ladder != null)
            {
                ladder.CoolDown = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ApplyLadderCooldown();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
    public static class ShipStatusFixedUpdatePatch
    {
        private static bool _hasAppliedCooldown;

        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!_hasAppliedCooldown)
            {
                ApplyLadderCooldown();
                _hasAppliedCooldown = true;
            }
        }
    }
}

[HarmonyPatch(typeof(Ladder), "get_MaxCoolDown")]
public static class AirshipLadderMaxCooldownPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref float __result)
    {
        if (!AirshipLadderCooldownUtils.IsAirship())
        {
            return true;
        }

        if (!OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            return true;
        }

        __result = 0.01f;
        return false;
    }
}

public static class AirshipLadderCooldownUtils
{
    public static bool IsAirship()
    {
        const byte AirshipMapId = 4;

        if (GameOptionsManager.Instance != null && GameOptionsManager.Instance.currentGameOptions != null)
        {
            return GameOptionsManager.Instance.currentGameOptions.MapId == AirshipMapId;
        }

        return TutorialManager.InstanceExists && AmongUsClient.Instance != null &&
               AmongUsClient.Instance.TutorialMapId == AirshipMapId;
    }

    public static float GetConfiguredCooldown()
    {
        return OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown ? 0f : 5f;
    }

    public static float Clamp(float value, float max)
    {
        return Mathf.Min(value, max);
    }
}

[HarmonyPatch(typeof(Ladder), "set_CoolDown")]
public static class AirshipLadderSetCooldownPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref float value)
    {
        if (!AirshipLadderCooldownUtils.IsAirship())
        {
            return;
        }

        if (!OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            return;
        }

        value = 0f;
    }
}

[HarmonyPatch(typeof(Ladder), "CanUse")]
public static class AirshipLadderCanUseCooldownPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref float __result)
    {
        if (!AirshipLadderCooldownUtils.IsAirship())
        {
            return;
        }

        if (!OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            return;
        }

        __result = 0f;
    }
}

[HarmonyPatch(typeof(Ladder), "Use")]
public static class AirshipLadderUseCooldownPatch
{
    [HarmonyPostfix]
    public static void Postfix(Ladder __instance)
    {
        if (__instance == null || !AirshipLadderCooldownUtils.IsAirship())
        {
            return;
        }

        if (!OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            return;
        }

        __instance.CoolDown = 0f;
        __instance.Destination?.SetDestinationCooldown();
        if (__instance.Destination != null)
        {
            __instance.Destination.CoolDown = 0f;
        }
    }
}

[HarmonyPatch(typeof(Ladder), "SetDestinationCooldown")]
public static class AirshipLadderDestinationCooldownPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Ladder __instance)
    {
        if (__instance == null || !AirshipLadderCooldownUtils.IsAirship())
        {
            return true;
        }

        if (!OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            return true;
        }

        if (__instance.Destination != null)
        {
            __instance.Destination.CoolDown = 0f;
        }

        return false;
    }
}

[HarmonyPatch(typeof(Ladder), "IsCoolingDown")]
public static class AirshipLadderIsCoolingDownPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref bool __result)
    {
        if (!AirshipLadderCooldownUtils.IsAirship())
        {
            return true;
        }

        if (OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            __result = false;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Ladder), "get_PercentCool")]
public static class AirshipLadderPercentCoolPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref float __result)
    {
        if (!AirshipLadderCooldownUtils.IsAirship())
        {
            return true;
        }

        if (OptionGroupSingleton<BetterAirshipOptions>.Instance.NoLadderCooldown)
        {
            __result = 0f;
            return false;
        }

        return true;
    }
}

