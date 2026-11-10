using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Modifiers;
using MiraAPI.Voting;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Universal;

namespace TownOfUs.Events.Modifiers;

public static class TiebreakerEvents
{
    public static CustomVote? TiebreakingVote { get; set; }

    // lower number higher priority
    [RegisterEvent]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        // Error($"TiebreakerEvents.ProcessVotesEventHandler");

        TiebreakingVote = null;
        if (@event.ExiledPlayer != null)
        {
            return;
        }

        VotingUtils.GetExiled(@event.Votes, out var isTie);
        if (!isTie)
        {
            return;
        }

        var tieBreakers = ModifierUtils.GetPlayersWithModifier<TiebreakerModifier>();
        if (!tieBreakers.Any())
        {
            return; // Skip everything if not a single person is tiebreaker
        }

        var votes = @event.Votes.ToList();
        var player = tieBreakers.Random();
        var vote = votes.FirstOrDefault(x => x.Voter == player!.PlayerId);

        if (vote == default)
        {
            return;
        }

        var extraVote = new CustomVote(vote.Voter, vote.Suspect);

        votes.Add(extraVote);

        // Message($"ProcessVotesEventHandler - exiled: {exiled?.PlayerName}");
        @event.ExiledPlayer = VotingUtils.GetExiled(votes, out _);

        TiebreakingVote = extraVote;
    }
}