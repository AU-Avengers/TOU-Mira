using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Roles;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class HaunterEvents
{
    [RegisterEvent]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        if (@event.Player.Data.Role is not HaunterRole haunter)
        {
            return;
        }

        haunter.CheckTaskRequirements();
    }
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        var allHaunters = CustomRoleUtils.GetActiveRolesOfType<HaunterRole>();
        if (!allHaunters.HasAny() || allHaunters.All(h => !h.CompletedAllTasks))
        {
            return;
        }

        foreach (var plr in PlayerControl.AllPlayerControls)
        {
            if (!HaunterRole.IsTargetOfHaunter(plr) || HaunterRole.HaunterVisibilityFlag(plr))
            {
                continue;
            }
            HaunterRole.AddRevealed(plr);
        }
    }
}