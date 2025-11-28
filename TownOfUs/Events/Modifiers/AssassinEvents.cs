using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;

namespace TownOfUs.Events.Modifiers;

public static class AssassinEvents
{
    [RegisterEvent]
    public static void VotingCompleteHandler(VotingCompleteEvent @event)
    {
        ModifierUtils.GetActiveModifiers<AssassinModifier>().Do(x => x.OnVotingComplete());
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!MeetingHud.Instance)
        {
            return;
        }

        var source = @event.Source;

        if (!source.HasModifier<AssassinModifier>())
        {
            return;
        }

        var target = @event.Target;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (source != target)
            {
                stats.CorrectAssassinKills++;
            }
            else
            {
                stats.IncorrectAssassinKills++;
            }
        }
    }
}