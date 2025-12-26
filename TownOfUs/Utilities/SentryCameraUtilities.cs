using Il2CppInterop.Runtime;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.PrefabChanging;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;
using BepInEx.Logging;
using MiraAPI.GameOptions;

namespace TownOfUs.Utilities;

public static class SentryCameraUtilities
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("SentryCameraUtilities");
    public static bool IsMapWithoutCameras(ExpandedMapNames mapId)
    {
        return mapId is ExpandedMapNames.MiraHq or ExpandedMapNames.Fungle or ExpandedMapNames.Submerged;
    }

    public static bool IsPendingCamera(SurvCamera cam)
    {
        if (cam == null) return false;
        try
        {
            if (cam.gameObject == null) return false;
            var sr = cam.gameObject.GetComponent<SpriteRenderer>();
            var alphaPending = sr != null && sr.color.a < 0.99f;
            var inactivePending = !cam.gameObject.activeSelf;
            return alphaPending || inactivePending;
        }
        catch
        {
            return false;
        }
    }

    public static SurvCamera? FindCameraTemplate()
    {
        SurvCamera? polusTemplateCamera = null;
        try
        {
            polusTemplateCamera = PrefabLoader.Polus != null
                ? PrefabLoader.Polus.GetComponentsInChildren<SurvCamera>(true).FirstOrDefault()
                : null;
        }
        catch
        {
            polusTemplateCamera = null;
        }

        SurvCamera? resourceTemplateCamera = null;
        try
        {
            var all = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(SurvCamera)));
            resourceTemplateCamera = all
                .FirstOrDefault(x =>
                {
                    var cam = x != null ? x.TryCast<SurvCamera>() : null;
                    if (cam == null || cam.gameObject == null) return false;
                    var sr = cam.gameObject.GetComponent<SpriteRenderer>();
                    return sr != null && sr.sprite != null;
                })
                ?.TryCast<SurvCamera>();
        }
        catch
        {
            resourceTemplateCamera = null;
        }

        var referenceCamera =
            polusTemplateCamera ??
            resourceTemplateCamera ??
            Object.FindObjectOfType<SurvCamera>() ??
            (PrefabLoader.Skeld != null ? PrefabLoader.Skeld.GetComponentsInChildren<SurvCamera>(true).FirstOrDefault() : null);

        return referenceCamera;
    }

    public static SurvCamera? CreateCameraAtPosition(Vector2 position, float zAxis, PlayerControl placer)
    {
        var referenceCamera = FindCameraTemplate();
        if (referenceCamera == null)
        {
            Logger.LogError("RpcRevealCamera - No SurvCamera template found (scene and prefabs) - cannot place camera");
            return null;
        }

        var vent = Object.FindObjectOfType<Vent>();
        if (vent == null)
        {
            Logger.LogError("No vent found to copy render settings");
            return null;
        }

        var ventRenderer = vent.GetComponent<SpriteRenderer>();

        var camera = Object.Instantiate(referenceCamera);

        var camRenderer = camera.GetComponent<SpriteRenderer>();
        if (camRenderer != null && ventRenderer != null)
        {
            camRenderer.sortingLayerID = ventRenderer.sortingLayerID;
            camRenderer.sortingOrder = ventRenderer.sortingOrder;
            camRenderer.sharedMaterial = ventRenderer.sharedMaterial;
        }

        camera.transform.position = new Vector3(
            position.x,
            position.y,
            vent.transform.position.z
        );

        camera.transform.localRotation = new Quaternion(0, 0, 1, 1);

        camera.Offset = new Vector3(0f, 0f, camera.Offset.z);

        camera.NewName = StringNames.None;
        var detectedRoomName = MiscUtils.GetRoomName(new Vector3(position.x, position.y, zAxis));
        camera.CamName = detectedRoomName;

        var spriteRenderer = camera.gameObject.GetComponent<SpriteRenderer>();
        var legacy = OptionGroupSingleton<SentryOptions>.Instance.LegacyMode;
        if (legacy)
        {
            var isPlacerClient = PlayerControl.LocalPlayer != null && placer != null &&
                                 PlayerControl.LocalPlayer.PlayerId == placer.PlayerId;
            if (spriteRenderer != null && isPlacerClient)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            }
            camera.gameObject.SetActive(isPlacerClient);
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
            camera.gameObject.SetActive(true);
        }

        if (ShipStatus.Instance == null)
        {
            Logger.LogError("RpcRevealCamera - ShipStatus.Instance is null");
            return null;
        }

        var allCameras = ShipStatus.Instance.AllCameras != null
            ? ShipStatus.Instance.AllCameras.ToList()
            : new List<SurvCamera>();
        allCameras.Add(camera);
        ShipStatus.Instance.AllCameras = allCameras.ToArray();

        return camera;
    }

    public static void ClearAllCameras()
    {
        if (SentryRole.Cameras.Count > 0)
        {
            foreach (var cameraPair in SentryRole.Cameras.Select(x => x.Key))
            {
                if (cameraPair == null)
                {
                    continue;
                }

                Object.Destroy(cameraPair.gameObject);
            }
        }

        SentryRole.Cameras.Clear();
    }
}