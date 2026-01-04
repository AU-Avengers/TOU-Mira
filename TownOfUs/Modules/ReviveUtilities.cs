using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules.Anims;
using TownOfUs.Modules.TimeLord;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules;

/// <summary>
/// Shared revive implementation (used by roles like Altruist and Time Lord).
/// This is intentionally modeled after the Altruist revive flow to avoid
/// ghost-role / crewmate-ghost desync issues.
/// </summary>
public static class ReviveUtilities
{
    private static System.Collections.IEnumerator CoEnsureNoLingeringBody(byte revivedId, float timeoutSeconds)
    {
        const float step = 0.05f;
        var waited = 0f;

        while (waited < timeoutSeconds)
        {
            var body = TimeLordBodyManager.FindDeadBodyIncludingInactive(revivedId) ??
                       UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == revivedId);
            if (body != null && body.gameObject != null)
            {
                try { UnityEngine.Object.Destroy(body.gameObject); } catch { /* ignored */ }
            }

            waited += step;
            yield return new WaitForSeconds(step);
        }
    }

    public static void RevivePlayer(
        PlayerControl reviver,
        PlayerControl revived,
        Vector2 position,
        RoleBehaviour roleWhenAlive,
        Color flashColor,
        string? revivedOwnerNotificationText,
        string? reviverOwnerNotificationText,
        Sprite? notificationIcon = null)
    {
        if (!revived || revived.Data == null)
        {
            return;
        }

        var inMeetingOrExile = MeetingHud.Instance || ExileController.Instance;

        GameHistory.ClearMurder(revived);

        revived.Revive();

        if (!inMeetingOrExile)
        {
            revived.transform.position = position;
            if (revived.AmOwner)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
            }

            if (revived.MyPhysics?.body != null)
            {
                revived.MyPhysics.body.position = position;
                Physics2D.SyncTransforms();
            }
        }

        if (ModCompatibility.IsSubmerged() && PlayerControl.LocalPlayer != null &&
            PlayerControl.LocalPlayer.PlayerId == revived.PlayerId)
        {
            ModCompatibility.ChangeFloor(revived.transform.position.y > -7);
        }

        if (!inMeetingOrExile && revived.AmOwner && !revived.HasModifier<LoverModifier>())
        {
            try
            {
                HudManager.Instance.Chat.gameObject.SetActive(false);
            }
            catch
            {
                // ignored
            }
        }

        revived.ChangeRole((ushort)roleWhenAlive.Role, recordRole: false);

        if (revived.Data.Role is IAnimated animatedRole)
        {
            animatedRole.IsVisible = true;
            animatedRole.SetVisible();
        }

        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(revived.Data.Role)).OfType<IAnimated>())
        {
            button.IsVisible = true;
            button.SetVisible();
        }

        foreach (var modifier in revived.GetModifiers<GameModifier>().Where(x => x is IAnimated))
        {
            if (modifier is IAnimated animatedMod)
            {
                animatedMod.IsVisible = true;
                animatedMod.SetVisible();
            }
        }

        revived.RemainingEmergencies = 0;
        if (reviver != null)
        {
            reviver.RemainingEmergencies = 0;
        }

        if (!inMeetingOrExile && revived.AmOwner && !string.IsNullOrWhiteSpace(revivedOwnerNotificationText))
        {
            try
            {
                TouAudio.PlaySound(TouAudio.AltruistReviveSound);
                Coroutines.Start(MiscUtils.CoFlash(flashColor));
                var notif = Helpers.CreateAndShowNotification(
                    $"<b>{flashColor.ToTextColor()}{revivedOwnerNotificationText}</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: notificationIcon);
                notif.AdjustNotification();
            }
            catch
            {
                // ignored
            }
        }

        if (!inMeetingOrExile && reviver != null && reviver.AmOwner && reviver != revived && !string.IsNullOrWhiteSpace(reviverOwnerNotificationText))
        {
            try
            {
                TouAudio.PlaySound(TouAudio.AltruistReviveSound);
                Coroutines.Start(MiscUtils.CoFlash(flashColor));
                var notif = Helpers.CreateAndShowNotification(
                    $"<b>{flashColor.ToTextColor()}{reviverOwnerNotificationText}</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: notificationIcon);
                notif.AdjustNotification();
            }
            catch
            {
                // ignored
            }
        }

        try
        {
            var body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                .FirstOrDefault(b => b.ParentId == revived.PlayerId);
            if (body != null)
            {
                UnityEngine.Object.Destroy(body.gameObject);
            }
        }
        catch
        {
            // ignored
        }

        Coroutines.Start(CoEnsureNoLingeringBody(revived.PlayerId, 1.0f));
    }
}