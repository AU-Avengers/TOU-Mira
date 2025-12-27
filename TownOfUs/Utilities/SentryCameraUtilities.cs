using Il2CppInterop.Runtime;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.PrefabChanging;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Modules;
using UnityEngine;
using Object = UnityEngine.Object;
using BepInEx.Logging;
using MiraAPI.GameOptions;

namespace TownOfUs.Utilities;

public static class SentryCameraUtilities
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("SentryCameraUtilities");
    private static bool _submergedConsoleSwappedThisRound;
    
    /// <summary>
    /// Reset the Submerged console swap flag on round start.
    /// </summary>
    public static void ResetSubmergedConsoleSwap()
    {
        _submergedConsoleSwappedThisRound = false;
    }

    /// <summary>
    /// On Submerged, once Sentry cameras exist, force the map's SecurityConsole to use Polus' vanilla surveillance minigame.
    /// This avoids fragile Submerged UI compatibility while still allowing Sentry cameras to be viewed.
    /// </summary>
    public static void EnsureSubmergedUsesPolusSurveillanceIfSentryCamsExist()
    {
        try
        {
            if (_submergedConsoleSwappedThisRound)
            {
                return;
            }

            if (!ModCompatibility.IsSubmerged())
            {
                return;
            }

            if (MiscUtils.GetCurrentMap != ExpandedMapNames.Submerged)
            {
                return;
            }

            // Only do this if there are actually Sentry cameras in the game.
            if (SentryRole.Cameras.Count <= 0)
            {
                return;
            }

            SystemConsole? polusSurvConsole = null;
            try
            {
                var polus = PrefabLoader.Polus;
                if (polus != null)
                {
                    polusSurvConsole = polus.GetComponentsInChildren<SystemConsole>(true)
                        .FirstOrDefault(x => x != null && x.gameObject != null && x.gameObject.name.Contains("Surv_Panel"));
                }
            }
            catch
            {
                polusSurvConsole = null;
            }

            if (polusSurvConsole == null || polusSurvConsole.MinigamePrefab == null)
            {
                return;
            }

            var submergedConsoles = Object.FindObjectsOfType<SystemConsole>()
                .Where(x => x != null && x.gameObject != null && x.gameObject.name.Contains("SecurityConsole"))
                .ToArray();

            if (submergedConsoles.Length == 0)
            {
                return;
            }

            foreach (var c in submergedConsoles)
            {
                if (c == null) continue;
                c.MinigamePrefab = polusSurvConsole.MinigamePrefab;
            }

            _submergedConsoleSwappedThisRound = true;
            Logger.LogInfo($"[Submerged] Swapped {submergedConsoles.Length} SecurityConsole(s) to use Polus Surv_Panel minigame (SentryCams={SentryRole.Cameras.Count})");
        }
        catch (System.Exception ex)
        {
            Logger.LogWarning($"Failed to swap Submerged SecurityConsole to Polus surveillance: {ex.Message}");
        }
    }
    
    public static bool IsMapWithoutCameras(ExpandedMapNames mapId)
    {
        // Special handling for Submerged: it has cameras but they need to be turned on
        // Check for SecurityConsole to determine if cameras exist
        if (ModCompatibility.IsSubmerged() && ShipStatus.Instance != null)
        {
            // Submerged has cameras - check if SecurityConsole exists
            try
            {
                var securityConsole = Object.FindObjectsOfType<SystemConsole>()
                    .FirstOrDefault(x =>
                        x != null && x.gameObject != null && x.gameObject.name.Contains("SecurityConsole"));
                if (securityConsole != null)
                {
                    // SecurityConsole exists, so Submerged has cameras
                    return false;
                }
            }
            catch
            {
                // If we can't find SecurityConsole, fall through to enumeration check
            }
        }

        // Check if cameras exist by enumerating ShipStatus.Instance.AllCameras
        // This works for custom maps like LevelImpostor (which may or may not have cameras)
        if (ShipStatus.Instance == null)
        {
            // Fallback to hardcoded list if ShipStatus is not available yet
            // Submerged is excluded because it has cameras (they just need to be turned on)
            return mapId is ExpandedMapNames.MiraHq or ExpandedMapNames.Fungle;
        }

        var allCameras = ShipStatus.Instance.AllCameras;
        if (allCameras == null || allCameras.Length == 0)
        {
            // If AllCameras is empty, check if it's Submerged (which has cameras that need activation)
            if (mapId == ExpandedMapNames.Submerged && ModCompatibility.IsSubmerged())
            {
                // Submerged has cameras even if AllCameras is empty initially
                // They're activated via SecurityConsole
                return false;
            }
            return true;
        }

        // Check if there are any original cameras (cameras that existed before sentry placed any)
        // Sentry cameras are added to AllCameras, so we need to filter them out
        // We'll check if there are any cameras that aren't from SentryRole.Cameras
        var sentryCameraIds = new HashSet<int>();
        foreach (var cameraPair in SentryRole.Cameras)
        {
            if (cameraPair.Key != null)
            {
                sentryCameraIds.Add(cameraPair.Key.GetInstanceID());
            }
        }
        
        // Check if any camera in AllCameras is not a sentry camera
        foreach (var cam in allCameras)
        {
            if (cam != null && !sentryCameraIds.Contains(cam.GetInstanceID()))
            {
                // Found an original camera, so the map has cameras
                return false;
            }
        }
        
        // No original cameras found in AllCameras
        // For Submerged, cameras exist but may not be in AllCameras until activated
        if (mapId == ExpandedMapNames.Submerged && ModCompatibility.IsSubmerged())
        {
            return false; // Submerged has cameras
        }
        
        return true;
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

        camera.transform.localRotation = Quaternion.identity;

        camera.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        camera.Offset = new Vector3(0f, 0f, camera.Offset.z);

        camera.NewName = StringNames.None;
        var detectedRoomName = MiscUtils.GetRoomName(new Vector3(position.x, position.y, zAxis));
        camera.CamName = detectedRoomName;

        try
        {
            var offAnim = TouAssets.SentryCamOffAnim.LoadAsset();
            var onAnim = TouAssets.SentryCamOnAnim.LoadAsset();
            if (offAnim != null)
            {
                camera.OffAnim = offAnim;
            }
            if (onAnim != null)
            {
                camera.OnAnim = onAnim;
            }
            
            camera.SetAnimation(false);
        }
        catch (System.Exception ex)
        {
            Logger.LogWarning($"Failed to set sentry camera animations: {ex.Message}");
        }

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