using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class OracleEvents
{
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        CustomRoleUtils.GetActiveRolesOfType<OracleRole>().Do(x => x.ReportOnConfession());
    }

    [RegisterEvent(30)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        if (@event.ExiledPlayer == null)
        {
            return;
        }

        if (@event.ExiledPlayer?.Object.HasModifier<OracleBlessedModifier>() == false)
        {
            return;
        }

        OracleRole.RpcOracleBless(@event.ExiledPlayer!.Object);
        @event.ExiledPlayer = null;
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        ModifierUtils.GetPlayersWithModifier<OracleConfessModifier>()
            .Do(x => x.RpcRemoveModifier<OracleConfessModifier>());
        ModifierUtils.GetPlayersWithModifier<OracleBlessedModifier>()
            .Do(x => x.RpcRemoveModifier<OracleBlessedModifier>());
    }
}