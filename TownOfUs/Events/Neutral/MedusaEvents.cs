using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Achievements;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Classic.Neutral.NeutralEvil;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class MedusaEvents
{
    [RegisterEvent(1)]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        CheckForMedusaGaze(@event, source, target, button is IKillButton);
    }

    [RegisterEvent(1)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        CheckForMedusaGaze(@event, source, target, true);
    }

    private static void CheckForMedusaGaze(MiraCancelableEvent miraEvent, PlayerControl source, PlayerControl target, bool isAttack)
    {
        if (MeetingHud.Instance || ExileController.Instance || isAttack || miraEvent is ExtendedMiraButtonClickEvent { IgnoreDefense: true })
        {
            return;
        }

        if (target.HasModifier<MedusaGazingModifier>() && source != target)
        {
            if (source.HasModifier<InvulnerabilityModifier>())
            {
                // stops pestilence from softlocking the game when attacking vet
                return;
            }
            miraEvent.Cancel();

            if (miraEvent is MiraButtonClickEvent { Button: JesterPokeButton })
            {
                TouAchievements.DontPokeTheBear.Unlock();
            }

            if (TutorialManager.InstanceExists || source.AmOwner)
            {
                target.RpcSpecialMurder(source, MeetingCheck.OutsideMeeting, createDeadBody: false, teleportMurderer: false, causeOfDeath: "Medusa");
            }
        }
    }
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (MedusaRole.AutoPlaceFakePlayers && source.IsRole<MedusaRole>() && !MeetingHud.Instance)
        // leave behind standing body
        // Message($"Leaving behind soulless player '{target.Data.PlayerName}'");
        {
            if (source.AmOwner)
            {
                TouAchievements.SnakeEyes.Increment(1,
                    TouAchievements.SnakeEyes.GetCurrentValue() == 14);
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.MedusaPetrifyNotif").Replace("<player>", $"{TownOfUsColors.Medusa.ToTextColor()}{target.Data.PlayerName}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Medusa);

                notif1.AdjustNotification();
            }
            _ = StonedPlayer.CreateStone(target);
        }
    }

    [RegisterEvent]
    public static void ReviveEventHandler(PlayerReviveEvent @event)
    {
        var player = @event.Player;

        var stonedPlayer = StonedPlayer.FakePlayers.FirstOrDefault(x => x.PlayerId == player.PlayerId && x.ProgressStage is not StoneStage.Permanent and not StoneStage.Petrified);
        if (stonedPlayer != null)
        {
            stonedPlayer.Destroy();
        }

        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x.PlayerId == player.PlayerId && x.body);
        if (fakePlayer != null)
        {
            fakePlayer.Destroy();
        }
    }

    // These are semi-frequent but not as costly as constantly updating the fake player names.
    [RegisterEvent]
    public static void RoleChangeEventHandler(SetRoleEvent _)
    {
        StonedPlayer.UpdateFakePlayerText(true);
        FakePlayer.UpdateFakePlayerText(true);
    }

    [RegisterEvent]
    public static void ChangeRoleEventHandler(ChangeRoleEvent _)
    {
        StonedPlayer.UpdateFakePlayerText(true);
        FakePlayer.UpdateFakePlayerText(true);
    }
}