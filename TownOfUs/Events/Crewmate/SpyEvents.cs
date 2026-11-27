using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

[RegisterEventContainer]
public static class SpyEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.HasModifier<SpyModifier>() && @event.Player.AmOwner)
        {
            SpyModifier.OnTaskComplete();
        }
        else if (@event.Player.Data.Role is SpyRole && @event.Player.AmOwner)
        {
            SpyRole.OnTaskComplete();
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return; // Never run when round starts.
        }

        if (PlayerControl.LocalPlayer.HasModifier<SpyModifier>())
        {
            SpyModifier.OnRoundStart();
        }
        else if (PlayerControl.LocalPlayer.Data.Role is SpyRole)
        {
            SpyRole.OnRoundStart();
        }
    }
}