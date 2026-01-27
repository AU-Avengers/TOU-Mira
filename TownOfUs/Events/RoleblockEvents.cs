using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Other;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events;

public static class RoleblockEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        foreach (var hangover in ModifierUtils.GetActiveModifiers<HangoverModifier>())
        {
            if (!hangover.TimerActive)
            {
                hangover.StartTimer();
                if (hangover.Player.AmOwner)
                {
                    var notif = Helpers.CreateAndShowNotification(
                        $"<b>You are now hungover!</color></b>", Color.white,
                        spr: TouRoleIcons.Barkeeper.LoadAsset());

                    notif.Text.SetOutlineThickness(0.35f);
                    notif.transform.localPosition = new Vector3(0f, 1f, -20f);
                }
            }
        }
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button;

        if (button == null || !button.CanClick())
        {
            return;
        }

        CheckForRoleblock(@event, source);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        CheckForRoleblock(@event, source);
    }

    private static void CheckForRoleblock(MiraCancelableEvent miraEvent, PlayerControl source)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!source.HasModifier<RoleblockedModifier>())
        {
            return;
        }

        miraEvent.Cancel();
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, $"{source.Data.PlayerName} was roleblocked, cancelling their interaction!");
    }
}