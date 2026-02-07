using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Neutral;

public static class GlitchEvents
{
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button;

        if (button == null || !button.CanClick())
        {
            return;
        }

        CheckForGlitchHacked(@event, source, true);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        CheckForGlitchHacked(@event, source);
    }

    private static void CheckForGlitchHacked(MiraCancelableEvent miraEvent, PlayerControl source, bool isLocal = false)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!source.HasModifier<GlitchHackedModifier>())
        {
            return;
        }

        miraEvent.Cancel();
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, $"{source.Data.PlayerName} was hacked, cancelling their interaction!");

        if (isLocal)
        {
            GlitchRole.RpcTriggerGlitchHack(source, false);
        }
        else
        {
            source.GetModifier<GlitchHackedModifier>()!.ShowHacked();
        }
    }
}