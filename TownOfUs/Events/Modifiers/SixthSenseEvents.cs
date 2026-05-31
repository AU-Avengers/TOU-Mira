using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Universal;

namespace TownOfUs.Events.Modifiers;

public static class SixthSenseEvents
{
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        // Warning("SixthSense click event!");
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick() || !target.HasModifier<SixthSenseModifier>())
        {
            return;
        }

        RpcTriggerSixthSense(PlayerControl.LocalPlayer, target);
    }

    [RegisterEvent]
    public static void KillButtonClickEventHandler(BeforeMurderEvent @event)
    {
        var target = @event.Target;
        if (target.AmOwner && target.HasModifier<SixthSenseModifier>())
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.SixthSense));
        }
    }

    [MethodRpc((uint)TownOfUsRpc.TriggerSixthSense, LocalHandling = RpcLocalHandling.None)]
    private static void RpcTriggerSixthSense(PlayerControl source, PlayerControl target)
    {
        if (target.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.SixthSense));
        }
    }
}