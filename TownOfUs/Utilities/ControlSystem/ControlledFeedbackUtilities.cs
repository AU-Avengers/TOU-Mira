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

        var controlledText = Modules.Localization.TouLocale.GetParsed(
            "TouControlControlledNotif",
            "You are being controlled by a <controller>!",
            new Dictionary<string, string> { { "<controller>", controllerName } });

        var colored = controllerColor.ToTextColor();
        return Helpers.CreateAndShowNotification(
            $"<b>{colored}{controlledText}</color></b>",
            Color.white,
            new Vector3(0f, 2f, -20f),
            spr: icon);
    }

    public static void ClearNotification(ref LobbyNotificationMessage? notification)
    {
        if (notification != null && notification.gameObject != null)
        {
            UnityEngine.Object.Destroy(notification);
            notification = null;
        }
    }
}


