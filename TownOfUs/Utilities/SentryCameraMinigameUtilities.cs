using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Utilities;

public static class SentryCameraMinigameUtilities
{
    public static void AddSentryCameras(Minigame minigame)
    {
        var ship = ShipStatus.Instance;
        if (ship == null || ship.AllCameras == null)
        {
            return;
        }

        if (minigame is SurveillanceMinigame skeld)
        {
            AddSentryCamerasToSkeld(skeld, ship);
            return;
        }

        var (cameraPrefab, texturesObj, texturesSetter) = SentryCameraReflectionUtilities.TryGetMinigameCameraData(minigame);
        if (cameraPrefab == null || texturesObj == null)
        {
            return;
        }

        if (ship.AllCameras.Length <= 4)
        {
            return;
        }

        var textureLength = SentryCameraReflectionUtilities.GetTexturesLength(texturesObj);
        if (textureLength < ship.AllCameras.Length)
        {
            texturesObj = SentryCameraReflectionUtilities.ResizeTextures(texturesObj, ship.AllCameras.Length);
            texturesSetter?.Invoke(minigame, texturesObj);
        }

        for (int i = 4; i < ship.AllCameras.Length; i++)
        {
            var survCamera = ship.AllCameras[i];
            if (survCamera == null)
            {
                continue;
            }

            if (SentryCameraReflectionUtilities.GetTextureAt(texturesObj, i) != null)
            {
                continue;
            }

            var existingCamera = minigame.transform.GetComponentsInChildren<Camera>()
                .FirstOrDefault(c => Mathf.Approximately(c.transform.position.x, survCamera.transform.position.x) &&
                                     Mathf.Approximately(c.transform.position.y, survCamera.transform.position.y) &&
                                     c.targetTexture != null);
            if (existingCamera != null)
            {
                continue;
            }

            var renderCam = Object.Instantiate(cameraPrefab);
            renderCam.transform.SetParent(minigame.transform, false);
            renderCam.transform.position = new Vector3(survCamera.transform.position.x, survCamera.transform.position.y, 8f);
            renderCam.orthographicSize = 2.35f;

            var temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
            SentryCameraReflectionUtilities.SetTextureAt(texturesObj, i, temporary);
            renderCam.targetTexture = temporary;
        }
    }

    public static void AddSentryCamerasToSkeld(SurveillanceMinigame minigame, ShipStatus ship)
    {
        if (ship.AllCameras.Length <= 4 || minigame.FilteredRooms.Length == 0)
        {
            return;
        }

        if (minigame.textures == null)
        {
            return;
        }

        if (minigame.textures.Length < ship.AllCameras.Length)
        {
            minigame.textures = minigame.textures.ToList().Concat(new RenderTexture[ship.AllCameras.Length - 4]).ToArray();
        }

        for (int i = 4; i < ship.AllCameras.Length; i++)
        {
            var survCamera = ship.AllCameras[i];
            if (survCamera == null)
            {
                continue;
            }

            if (minigame.textures[i] != null)
            {
                continue;
            }

            var existingCamera = minigame.transform.GetComponentsInChildren<Camera>()
                .FirstOrDefault(c => Mathf.Approximately(c.transform.position.x, survCamera.transform.position.x) &&
                                     Mathf.Approximately(c.transform.position.y, survCamera.transform.position.y) &&
                                     c.targetTexture != null);
            if (existingCamera != null)
            {
                continue;
            }

            var cameraPrefab = minigame.CameraPrefab;
            if (cameraPrefab == null)
            {
                cameraPrefab = minigame.GetComponentsInChildren<Camera>()
                    .FirstOrDefault(c => c != null && c.targetTexture != null);
                if (cameraPrefab == null)
                {
                    continue;
                }
            }

            Camera camera = Object.Instantiate(cameraPrefab);
            camera.transform.SetParent(minigame.transform);
            camera.transform.position = new Vector3(survCamera.transform.position.x, survCamera.transform.position.y, 8f);
            camera.orthographicSize = 2.35f;

            RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
            minigame.textures[i] = temporary;
            camera.targetTexture = temporary;
        }
    }

    public static bool IsPendingCamera(SurvCamera cam)
    {
        return SentryCameraUtilities.IsPendingCamera(cam);
    }

    public static void SwapAllCamerasForNonSentry(Minigame minigame)
    {
        if (minigame == null) return;
        if (IsLocalSentry()) return;
        if (ShipStatus.Instance == null) return;
        if (ShipStatus.Instance.AllCameras == null) return;

        var id = minigame.GetInstanceID();
        if (OriginalAllCamerasByMinigameId.ContainsKey(id)) return;

        var original = ShipStatus.Instance.AllCameras;
        var filtered = original.Where(c => c != null && !IsPendingCamera(c)).ToArray();
        OriginalAllCamerasByMinigameId[id] = original;
        ShipStatus.Instance.AllCameras = filtered;
    }

    public static void RestoreAllCameras(Minigame minigame)
    {
        if (minigame == null) return;
        if (ShipStatus.Instance == null) return;

        var id = minigame.GetInstanceID();
        if (!OriginalAllCamerasByMinigameId.TryGetValue(id, out var original)) return;

        try
        {
            ShipStatus.Instance.AllCameras = original;
        }
        catch
        {
            // ignored
        }
        finally
        {
            OriginalAllCamerasByMinigameId.Remove(id);
        }
    }

    private static bool IsLocalSentry()
    {
        try
        {
            return PlayerControl.LocalPlayer?.Data?.Role is SentryRole;
        }
        catch
        {
            return false;
        }
    }

    private static readonly System.Collections.Generic.Dictionary<int, SurvCamera[]?> OriginalAllCamerasByMinigameId = new();
}

