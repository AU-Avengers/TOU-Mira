using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Events.Misc;

public static class KnightedEvents
{
    public static List<CustomVote> ExtraKnightVotes { get; } = [];
    public static bool ShowVotes => OptionGroupSingleton<MonarchOptions>.Instance.ShowKnightedVotes;
    public static int TotalVotes => (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight + 1;

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent _)
    {
        if (!OptionGroupSingleton<MonarchOptions>.Instance.RevealAtMeeting || !HudManager.InstanceExists)
        {
            return;
        }

        var knights = PlayerControl.LocalPlayer.GetModifiers<KnightedModifier>().Where(x => !x.Announced).ToList();
        if (knights.Count == 0)
        {
            return;
        }

        var title = $"<color=#{TownOfUsColors.Monarch.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleMonarchMessageTitle")}</color>";
        var role = $"{TownOfUsColors.Monarch.ToTextColor()}{TouLocale.Get("TouRoleMonarch", "Monarch")}</color>";
        var votes = ((int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight).ToString(TownOfUsPlugin.Culture);

        foreach (var knight in knights)
        {
            knight.Announced = true;

            var message = TouLocale.GetParsed("TouRoleMonarchKnightedFeedback").Replace("<role>", role).Replace("<votes>", votes);
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, message, false, true);
        }

        ModifierDisplayComponent.Instance?.RefreshModifiers();
    }

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
