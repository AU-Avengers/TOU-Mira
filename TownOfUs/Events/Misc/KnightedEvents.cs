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

    [RegisterEvent]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        ExtraKnightVotes.Clear();

        var votes = @event.Votes.ToList();
        var baseExtraVotes = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;
        var allowSelfVotes = OptionGroupSingleton<MonarchOptions>.Instance.AllowSelfVotes;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var knightModifiers = player.GetModifiers<KnightedModifier>()?.ToList();
            if (knightModifiers == null || knightModifiers.Count == 0)
                continue;

            var vote = votes.FirstOrDefault(v => v.Voter == player.PlayerId);
            if (vote == default)
                continue;

            if (vote.Voter == vote.Suspect && !allowSelfVotes)
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
}
