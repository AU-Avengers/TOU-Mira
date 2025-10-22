using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral;
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

        CheckForGlitchHacked(@event, source);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        CheckForGlitchHacked(@event, source);
    }

    private static void CheckForGlitchHacked(MiraCancelableEvent miraEvent, PlayerControl source)
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

        if (source.AmOwner)
        {
            PlayerControl.LocalPlayer.GetModifier<GlitchHackedModifier>()!.ShowHacked();
        }
    }
}