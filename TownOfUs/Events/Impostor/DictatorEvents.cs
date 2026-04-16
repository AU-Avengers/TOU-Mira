using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Voting;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Impostor;

public static class DictatorEvents
{
    [RegisterEvent(5)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        foreach (var dictator in CustomRoleUtils.GetActiveRolesOfType<DictatorRole>())
        {
            CoerceVotes(@event, dictator);
        }
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent @event)
    {
        foreach (var dictator in CustomRoleUtils.GetActiveRolesOfType<DictatorRole>())
        {
            if (!dictator.CoerceActive)
            {
                continue;
            }

            ClearInfluence(dictator.Player.PlayerId);
            dictator.CoerceActive = false;
            dictator.CoerceTargetId = byte.MaxValue;
        }
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player.Data.Role is not DictatorRole)
        {
            return;
        }

        ClearInfluence(@event.Player.PlayerId);
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null || exiled.Data.Role is not DictatorRole)
        {
            return;
        }

        ClearInfluence(exiled.PlayerId);
    }

    private static void CoerceVotes(ProcessVotesEvent @event, DictatorRole dictator)
    {
        if (!dictator || dictator.Player.HasDied() || !dictator.CoerceActive)
        {
            return;
        }

        var dictatorVote = @event.Votes.FirstOrDefault(x => x.Voter == dictator.Player.PlayerId);
        if (dictatorVote == null || dictatorVote.Suspect == byte.MaxValue)
        {
            return;
        }

        var target = MiscUtils.PlayerById(dictatorVote.Suspect);
        if (target == null || target.HasDied() || target.Data.Disconnected)
        {
            return;
        }

        dictator.CoerceTargetId = target.PlayerId;

        var votes = @event.Votes.ToList();
        var didChange = false;
        var stealMayorVotes = OptionGroupSingleton<DictatorOptions>.Instance.CanStealMayorVotes;

        foreach (var influenced in ModifierUtils.GetActiveModifiers<DictatorInfluencedModifier>(x => x.DictatorId == dictator.Player.PlayerId).ToList())
        {
            var voter = influenced.Player;
            if (!dictator.IsValidInfluenceTarget(voter))
            {
                continue;
            }

            if (voter.Data.Role is MayorRole && !stealMayorVotes)
            {
                continue;
            }

            var voteCount = votes.Count(x => x.Voter == voter.PlayerId);
            if (voteCount == 0)
            {
                votes.Add(new CustomVote(voter.PlayerId, target.PlayerId));
                didChange = true;
                continue;
            }

            votes.RemoveAll(x => x.Voter == voter.PlayerId);
            for (var i = 0; i < voteCount; i++)
            {
                votes.Add(new CustomVote(voter.PlayerId, target.PlayerId));
            }

            didChange = true;
        }

        if (!didChange)
        {
            return;
        }

        @event.Votes.Clear();
        votes.ForEach(vote => @event.Votes.Add(vote));
        @event.ExiledPlayer = VotingUtils.GetExiled(@event.Votes, out _);
    }

    private static void ClearInfluence(byte dictatorId)
    {
        foreach (var modifier in ModifierUtils.GetActiveModifiers<DictatorInfluencedModifier>(x => x.DictatorId == dictatorId)
                     .ToList())
        {
            modifier.ModifierComponent?.RemoveModifier(modifier);
        }
    }
}
