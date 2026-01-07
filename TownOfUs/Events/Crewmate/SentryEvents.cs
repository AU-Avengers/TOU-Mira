using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Events.Crewmate;

public static class SentryEvents
{
    private static int ActiveCameraTaskCount;
    private static uint LastCameraUseTaskId = uint.MaxValue;

    private static bool ShouldUnlockPortableCameras(SentryRole role)
    {
        var options = OptionGroupSingleton<SentryOptions>.Instance;
        var mapId = MiscUtils.GetCurrentMap;
        var mapWithoutCameras = SentryCameraUtilities.IsMapWithoutCameras(mapId);
        return options.PortableCamerasMode switch
        {
            SentryPortableCamerasMode.Immediately => true,
            SentryPortableCamerasMode.AfterTasks => role.CompletedAllTasks,
            SentryPortableCamerasMode.OnMapsWithoutCameras => mapWithoutCameras || role.CompletedAllTasks,
            _ => role.CompletedAllTasks
        };
    }

    private static IEnumerator FlashActionButton(ActionButton? button, float durationSeconds = 1.5f)
    {
        if (button == null)
        {
            yield break;
        }

        var sr = button.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            yield break;
        }

        var original = sr.color;
        var t = 0f;
        while (t < durationSeconds)
        {
            var pulse = Mathf.PingPong(t * 6f, 1f);
            sr.color = Color.Lerp(original, new Color(1f, 0.9f, 0.2f, 1f), pulse);
            t += Time.deltaTime;
            yield return null;
        }

        sr.color = original;
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            SentryRole.ClearAll();
            SentryRole.ClearPortableCamsUsers();
            ActiveCameraTaskCount = 0;
            LastCameraUseTaskId = uint.MaxValue;
            SentryPortableCameraButtonBase.ResetBatteryState();
            SentryCameraUtilities.ResetSubmergedConsoleSwap();
        }

        if (PlayerControl.LocalPlayer?.Data?.Role is not SentryRole sentryRole)
        {
            return;
        }

        var options = OptionGroupSingleton<SentryOptions>.Instance;
        var btn = CustomButtonSingleton<SentryPlaceCameraButton>.Instance;
        if (@event.TriggeredByIntro)
        {
            btn.SetUses((int)options.InitialCameras.Value);
        }
        if (!btn.LimitedUses)
        {
            btn.Button?.usesRemainingText.gameObject.SetActive(false);
            btn.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            btn.Button?.usesRemainingText.gameObject.SetActive(true);
            btn.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }

        if (ShouldUnlockPortableCameras(sentryRole) && !sentryRole.PortableCamsUnlockedNotified)
        {
            sentryRole.PortableCamsUnlockedNotified = true;

            SentryPortableCameraButtonBase.ResetBatteryToMax();

            CustomButtonSingleton<SentryPortableCameraButton>.Instance.SetActive(true, sentryRole);
            CustomButtonSingleton<SentryPortableCameraSecondaryButton>.Instance.SetActive(true, sentryRole);

            var notifText = TouLocale.GetParsed("TouRoleSentryPortableCameraUnlocked", "Portable Cameras Unlocked!");
            var notif = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Sentry.ToTextColor()}{notifText}</color></b>",
                Color.white,
                new Vector3(0f, 1f, -150f),
                spr: TouRoleIcons.Sentry.LoadAsset());
            notif.AdjustNotification();

            Coroutines.Start(FlashActionButton(CustomButtonSingleton<SentryPortableCameraButton>.Instance.Button));
        }
    }

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is SentryRole sentryRole)
        {
            var options = OptionGroupSingleton<SentryOptions>.Instance;

            if (@event.Task != null && @event.Task.Id != LastCameraUseTaskId)
            {
                ++ActiveCameraTaskCount;
                LastCameraUseTaskId = @event.Task.Id;
            }

            var button = CustomButtonSingleton<SentryPlaceCameraButton>.Instance;
            if (button.LimitedUses && options.TasksPerCamera.Value != 0 && options.TasksPerCamera.Value <= ActiveCameraTaskCount)
            {
                ++button.UsesLeft;
                button.SetUses(button.UsesLeft);
                ActiveCameraTaskCount = 0;
            }

            if (sentryRole.CompletedAllTasks && !sentryRole.PortableCamsUnlockedNotified)
            {
                if (!ShouldUnlockPortableCameras(sentryRole))
                {
                    return;
                }

                sentryRole.PortableCamsUnlockedNotified = true;

                SentryPortableCameraButtonBase.ResetBatteryToMax();

                CustomButtonSingleton<SentryPortableCameraButton>.Instance.SetActive(true, sentryRole);
                CustomButtonSingleton<SentryPortableCameraSecondaryButton>.Instance.SetActive(true, sentryRole);

                var notifText = TouLocale.GetParsed("TouRoleSentryPortableCameraUnlocked", "Portable Cameras Unlocked!");
                var notif = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Sentry.ToTextColor()}{notifText}</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -150f),
                    spr: TouRoleIcons.Sentry.LoadAsset());
                notif.AdjustNotification();

                Coroutines.Start(FlashActionButton(CustomButtonSingleton<SentryPortableCameraButton>.Instance.Button));
            }
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (OptionGroupSingleton<SentryOptions>.Instance.DeployedCamerasVisibility is SentryDeployedCamerasVisibility.AfterMeeting)
        {
            foreach (var cameraPair in SentryRole.Cameras)
            {
                if (cameraPair.Key == null)
                {
                    continue;
                }

                cameraPair.Key.gameObject.SetActive(true);
                var spriteRenderer = cameraPair.Key.gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }

        if ((int)OptionGroupSingleton<SentryOptions>.Instance.CameraRoundsLast > 0)
        {
            var cameraList = new List<KeyValuePair<SurvCamera, int>>();
            if (SentryRole.Cameras.Count > 0)
            {
                foreach (var cameraPair in SentryRole.Cameras.ToList())
                {
                    if (cameraPair.Key == null || cameraPair.Key.gameObject == null)
                    {
                        continue;
                    }

                    if (cameraPair.Value <= 1)
                    {
                        if (ShipStatus.Instance != null && ShipStatus.Instance.AllCameras != null)
                        {
                            var allCameras = ShipStatus.Instance.AllCameras.ToList();
                            if (allCameras.Contains(cameraPair.Key))
                            {
                                allCameras.Remove(cameraPair.Key);
                                ShipStatus.Instance.AllCameras = allCameras.ToArray();
                            }
                        }
                        Object.Destroy(cameraPair.Key.gameObject);
                        continue;
                    }

                    cameraList.Add(new(cameraPair.Key, cameraPair.Value - 1));
                }
            }

            SentryRole.Cameras = cameraList;
        }
    }
}