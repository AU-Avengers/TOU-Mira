using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Events;

public static class RoleblockEvents
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

        if (!source.HasModifier<BarkeeperRoleblockedModifier>() && !source.HasModifier<BootleggerRoleblockedModifier>())
        {
            return;
        }

        miraEvent.Cancel();
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, $"{source.Data.PlayerName} was roleblocked, cancelling their interaction!");
    }
}