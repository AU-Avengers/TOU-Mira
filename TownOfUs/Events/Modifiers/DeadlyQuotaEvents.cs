using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Impostor;
using MiraAPI.Events.Mira;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using Reactor.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Options.Modifiers.Impostor;
using MiraAPI.Utilities;

namespace TownOfUs.Events.Modifiers;

public static class DeadlyQuotaEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        if (!source.AmOwner || !source.HasModifier<DeadlyQuotaModifier>() || MeetingHud.Instance)
            return;

        var deadlyQuota = source.GetModifier<DeadlyQuotaModifier>();

        if (deadlyQuota is null) return;

        deadlyQuota.KillCount++;

        if (deadlyQuota.KillQuota > deadlyQuota.KillCount)
        {
            var remaining = deadlyQuota.KillQuota - deadlyQuota.KillCount;

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>You need {remaining} more {(remaining == 1 ? "kill" : "kills")} to complete your quota!</b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouModifierIcons.DeadlyQuota.LoadAsset());


            notif1.Text.SetOutlineThickness(0.4f);
        }

        if (deadlyQuota.KillQuota == deadlyQuota.KillCount)
        {
            if (!OptionGroupSingleton<DeadlyQuotaOptions>.Instance.QuotaShield)
            {
                var notif2 = Helpers.CreateAndShowNotification(
                $"<b>You have completed your quota!</b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouModifierIcons.DeadlyQuota.LoadAsset());

                notif2.Text.SetOutlineThickness(0.4f);
            }
            else
            {
                var notif3 = Helpers.CreateAndShowNotification(
                $"<b>You have completed your quota! You have lost your temporarily shield.</b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouModifierIcons.DeadlyQuota.LoadAsset());

                notif3.Text.SetOutlineThickness(0.4f);

            }
        }

        if (deadlyQuota.KillCount >= deadlyQuota.KillQuota) deadlyQuota.KillCount = deadlyQuota.KillQuota;


    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
            return;

        CheckForTemporaryShield(@event, target);
    }

    [RegisterEvent]
    public static void MiraButtonCancelledEventHandler(MiraButtonCancelledEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button is not IKillButton)
            return;

        if (!CheckForTemporaryShield(null, target))
            return;

        ResetButtonTimer(source, button);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForTemporaryShield(@event, target))
        {
            ResetButtonTimer(source);

            if (target.HasModifier<DeadlyQuotaModifier>() && source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(new Color(0f, 0.5f, 0f, 1f)));
            }
        }
    }

    private static bool CheckForTemporaryShield(MiraCancelableEvent? @event, PlayerControl target)
    {
        var deadlyQuota = target.GetModifier<DeadlyQuotaModifier>();
        if (deadlyQuota is null)
            return false;

        if (!OptionGroupSingleton<DeadlyQuotaOptions>.Instance.QuotaShield)
            return false;

        if (MeetingHud.Instance || ExileController.Instance)
            return false;

        if (deadlyQuota.KillQuota <= deadlyQuota.KillCount)
            return false;

        @event?.Cancel();
        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;

        button?.SetTimer(reset);

        if (!source.AmOwner || !source.IsImpostor())
            return;

        source.SetKillTimer(reset);
    }

}