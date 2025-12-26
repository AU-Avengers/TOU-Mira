using HarmonyLib;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

public static class SentryCameraPortablePatch
{
    private static bool _portableBlinkLastState;
    private static int _portableBlinkLastCameraCount = -1;

    public static void ApplyPortableBlinkState()
    {
        if (!IsMapWithoutCameras()) return;

        var ship = ShipStatus.Instance;

        SurvCamera[] cameras;
        if (ship != null && ship.AllCameras != null && ship.AllCameras.Length > 0)
        {
            cameras = ship.AllCameras.Where(x => x != null).ToArray();
        }
        else
        {
            cameras = SentryRole.Cameras.Select(x => x.Key).Where(x => x != null).ToArray();
        }

        var desired = SentryRole.AnyPortableCamsInUse;
        var cameraCount = cameras.Length;

        if (desired == _portableBlinkLastState && cameraCount == _portableBlinkLastCameraCount) return;
        _portableBlinkLastState = desired;
        _portableBlinkLastCameraCount = cameraCount;

        if (cameras.Length == 0) return;

        foreach (var cam in cameras)
        {
            if (cam == null || cam.gameObject == null) continue;
            cam.SetAnimation(desired);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePortableBlinkPostfix()
    {
        ApplyPortableBlinkState();
    }

    private static bool IsMapWithoutCameras()
    {
        try
        {
            var mapId = MiscUtils.GetCurrentMap;
            return mapId is ExpandedMapNames.MiraHq or ExpandedMapNames.Fungle or ExpandedMapNames.Submerged or ExpandedMapNames.LevelImpostor;
        }
        catch
        {
            return false;
        }
    }
}