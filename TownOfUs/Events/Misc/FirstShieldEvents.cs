using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Misc;

public static class FirstShieldEvents
{
    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        CheckForFirstDeathShield(@event, @event.Target, @event.Source);
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;
        if (target == null || button is not IKillButton || !button.CanClick())
        {
            return;
        }

        CheckForFirstDeathShield(@event, target, source, button);
    }

    [RegisterEvent]
    public static void RemoveShieldEventHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro && PlayerControl.LocalPlayer.HasModifier<FirstDeadShield>())
        {
            PlayerControl.LocalPlayer.RpcRemoveModifier<FirstDeadShield>();
        }
    }

    private static void CheckForFirstDeathShield(MiraCancelableEvent @event, PlayerControl target, PlayerControl source,
        CustomActionButton<PlayerControl>? button = null)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!target.HasModifier<FirstDeadShield>() || source == target ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return;
        }

        @event.Cancel();

        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;

        button?.SetTimer(reset);

        // Reset impostor kill cooldown if they attack a shielded player
        if (!source.AmOwner || !source.IsImpostor())
        {
            return;
        }

        source.SetKillTimer(reset);
    }
}