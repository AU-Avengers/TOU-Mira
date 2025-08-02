using HarmonyLib;
using InnerNet;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch(typeof(LongBoiPlayerBody))]
public static class LongBoiPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LongBoiPlayerBody), nameof(LongBoiPlayerBody.Awake))]
    public static bool LongBodyAwakePatch(LongBoiPlayerBody __instance)
    {
        __instance.cosmeticLayer.OnSetBodyAsGhost += (Action)__instance.SetPoolableGhost;
        __instance.cosmeticLayer.OnColorChange += (Action<int>)__instance.SetHeightFromColor;
        __instance.cosmeticLayer.OnCosmeticSet += (Action<string, int, CosmeticsLayer.CosmeticKind>)__instance.OnCosmeticSet;
        __instance.gameObject.layer = 8;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LongBoiPlayerBody), nameof(LongBoiPlayerBody.SetHeightFromColor))]
    public static bool SetHeightColorPatch(LongBoiPlayerBody __instance)
    {
        if (!__instance.isPoolablePlayer)
        {
            if (GameManager.Instance.IsHideAndSeek() &&
                AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started &&
                __instance.myPlayerControl.Data.Role != null &&
                __instance.myPlayerControl.Data.Role.TeamType == RoleTeamTypes.Impostor)
            {
                return false;
            }

            __instance.targetHeight = __instance.heightsPerColor[0] - 0.5f;
            if (LobbyBehaviour.Instance)
            {
                __instance.SetupNeckGrowth(false, false);
                return false;
            }

            __instance.SetupNeckGrowth(true, false);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LongBoiPlayerBody), nameof(LongBoiPlayerBody.Start))]
    public static bool LongBodyStartPatch(LongBoiPlayerBody __instance)
    {
        __instance.ShouldLongAround = true;
        __instance.skipNeckAnim = true;
        if (__instance.hideCosmeticsQC)
        {
            __instance.cosmeticLayer.SetHatVisorVisible(false);
        }

        __instance.SetupNeckGrowth(true);
        if (__instance.isExiledPlayer)
        {
            var instance = ShipStatus.Instance;
            if (instance == null || instance.Type != ShipStatus.MapType.Fungle)
            {
                __instance.cosmeticLayer.AdjustCosmeticRotations(-17.75f);
            }
        }

        if (!__instance.isPoolablePlayer)
        {
            __instance.cosmeticLayer.ValidateCosmetics();
        }

        if (__instance.myPlayerControl)
        {
            __instance.StopAllCoroutines();
            __instance.SetHeightFromColor(__instance.myPlayerControl.Data.DefaultOutfit.ColorId);
        }

        return false;
    }
}