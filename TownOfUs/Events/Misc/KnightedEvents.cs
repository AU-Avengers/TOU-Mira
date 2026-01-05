using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Voting;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Events.Misc;

public static class KnightedEvents
{
    public static List<CustomVote> ExtraKnightVotes { get; } = [];
    public static bool ShowVotes => OptionGroupSingleton<MonarchOptions>.Instance.ShowKnightedVotes;
    public static int TotalVotes => (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight + 1;

    [RegisterEvent]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        ExtraKnightVotes.Clear();
        if (ShowVotes)
        {
            return;
        }

        var votes = @event.Votes.ToList();
        var baseExtraVotes = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var knightModifiers = player.GetModifiers<KnightedModifier>()?.ToList();
            if (knightModifiers == null || knightModifiers.Count == 0)
                continue;

            var vote = votes.FirstOrDefault(v => v.Voter == player.PlayerId);
            if (vote == default)
                continue;

            var totalBonusVotes = knightModifiers.Count * baseExtraVotes;

            for (var i = 0; i < totalBonusVotes; i++)
            {
                var extraVote = new CustomVote(vote.Voter, vote.Suspect);
                votes.Add(extraVote);
                ExtraKnightVotes.Add(extraVote);
            }
        }

        @event.ExiledPlayer = VotingUtils.GetExiled(votes, out _);
    }

    [RegisterEvent]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        if (!ShowVotes || !@event.VoteData.Owner.HasModifier<KnightedModifier>())
        {
            return;
        }

        @event.VoteData.SetRemainingVotes(0);

        for (var i = 0; i < TotalVotes; i++)
        {
            @event.VoteData.VoteForPlayer(@event.TargetId);
        }

        @event.Cancel();
    }

}
