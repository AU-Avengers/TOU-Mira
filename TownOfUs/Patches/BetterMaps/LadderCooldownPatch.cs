using HarmonyLib;
using TownOfUs.Options.Maps;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.BetterMaps;

[HarmonyPatch(typeof(ShipStatus))]
public static class MapLadderCooldownPatch
{
    private static void ApplyLadderCooldown()
    {
        if (!TownOfUsMapOptions.AreLadderCooldownsDisabled())
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
public static class MapLadderMaxCooldownPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref float __result)
    {
        if (!TownOfUsMapOptions.AreLadderCooldownsDisabled())
        {
            return true;
        }

        __result = 0.01f;
        return false;
    }
}

[HarmonyPatch(typeof(Ladder), "set_CoolDown")]
public static class MapLadderSetCooldownPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref float value)
    {
        if (!TownOfUsMapOptions.AreLadderCooldownsDisabled())
        {
            return;
        }

        value = 0f;
    }
}

[HarmonyPatch(typeof(Ladder), "CanUse")]
public static class MapLadderCanUseCooldownPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref float __result)
    {
        if (!TownOfUsMapOptions.AreLadderCooldownsDisabled())
        {
            return;
        }

        __result = 0f;
    }
}

[HarmonyPatch(typeof(Ladder), "Use")]
public static class MapLadderUseCooldownPatch
{
    [HarmonyPostfix]
    public static void Postfix(Ladder __instance)
    {
        if (__instance == null || !TownOfUsMapOptions.AreLadderCooldownsDisabled())
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
public static class MapLadderDestinationCooldownPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Ladder __instance)
    {
        if (__instance == null || !TownOfUsMapOptions.AreLadderCooldownsDisabled())
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
public static class MapLadderIsCoolingDownPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref bool __result)
    {
        if (!TownOfUsMapOptions.AreLadderCooldownsDisabled())
        {
            return true;
        }

        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(Ladder), "get_PercentCool")]
public static class MapLadderPercentCoolPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref float __result)
    {
        if (!TownOfUsMapOptions.AreLadderCooldownsDisabled())
        {
            return true;
        }

        __result = 0f;
        return false;
    }
}

