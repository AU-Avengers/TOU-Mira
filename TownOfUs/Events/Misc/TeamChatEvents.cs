using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Patches.Options;

namespace TownOfUs.Events.Misc;

// Never hurts to check... i think - Atony
public static class TeamChatEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (TeamChatPatches.TeamChatActive)
        {
            TeamChatPatches.ToggleTeamChat();
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (TeamChatPatches.TeamChatActive)
        {
            TeamChatPatches.ToggleTeamChat();
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (TeamChatPatches.TeamChatActive)
        {
            TeamChatPatches.ToggleTeamChat();
        }
    }
}