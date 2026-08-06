using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Utilities.ControlSystem;

public static class ControlledFeedbackUtilities
{
    /// <summary>
    /// Shows a "You are being controlled by X" notification for the local player (victim).
    /// </summary>
    public static LobbyNotificationMessage? ShowControlledByNotification(string controllerName, Color controllerColor, Sprite? icon)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || !local.AmOwner)
        {
            return null;
        }

        string[] possibles =
        [
            "TouRolePuppeteerControlNotifBasic", "TouRolePuppeteerControlNotif1", "TouRolePuppeteerControlNotif2",
            "TouRolePuppeteerControlNotif3"
        ];
        var controlledText = TouLocale.GetParsed(
            possibles.RandomSnapshot()).Replace("<role>", controllerName);

        var colored = controllerColor.ToTextColor();
        var notif = Helpers.CreateAndShowNotification(
            $"<b>{colored}{controlledText}</color></b>",
            Color.white,
            new Vector3(0f, 2f, -20f),
            spr: icon);
        notif.AdjustNotification();
        return notif;
    }

    public static void ClearNotification(ref LobbyNotificationMessage? notification)
    {
        if (notification == null)
        {
            return;
        }

        try
        {
            if (notification.gameObject != null)
            {
                UnityEngine.Object.Destroy(notification.gameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(notification);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            notification = null;
        }
    }
}