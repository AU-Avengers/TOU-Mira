using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Patches.Options;

namespace TownOfUs.Events.Misc;

// Never hurts to check... i think - Atony
public static class TeamChatEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent _)
    {
        if (TeamChatPatches.TeamChatActive)
        {
            TeamChatPatches.ToggleTeamChat();
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent _)
    {
        if (TeamChatPatches.TeamChatActive)
        {
            TeamChatPatches.ToggleTeamChat();
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent _)
    {
        if (TeamChatPatches.TeamChatActive)
        {
            TeamChatPatches.ToggleTeamChat();
        }
    }
}