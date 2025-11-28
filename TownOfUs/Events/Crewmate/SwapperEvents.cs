using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities;
using TownOfUs.Events.Modifiers;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class SwapperEvents
{
    // lower number higher priority
    [RegisterEvent(10)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        // Error($"SwapperEvents.ProcessVotesEventHandler");
        CustomRoleUtils.GetActiveRolesOfType<SwapperRole>().Do(x => SwapVotes(@event, x));
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent @event)
    {
        if (!CustomRoleUtils.GetActiveRolesOfType<SwapperRole>().Any())
        {
            return;
        }

        Coroutines.Start(PerformSwaps());
    }

    private static void SwapVotes(ProcessVotesEvent @event, SwapperRole swapper)
    {
        if (!swapper || swapper.Player.HasDied() || !swapper.Swap1 || !swapper.Swap2)
        {
            return;
        }

        var swap1 = swapper.Swap1!.TargetPlayerId;
        var swap2 = swapper.Swap2!.TargetPlayerId;

        var originalVoteList = @event.Votes.ToList();

        if (TiebreakerEvents.TiebreakingVote.HasValue)
        {
            originalVoteList.Add(TiebreakerEvents.TiebreakingVote.Value);
        }

        var voteList = new List<CustomVote>();

        foreach (var vote in originalVoteList)
        {
            if (vote.Suspect == swap1)
            {
                voteList.Add(new CustomVote(vote.Voter, swap2));
            }
            else if (vote.Suspect == swap2)
            {
                voteList.Add(new CustomVote(vote.Voter, swap1));
            }
            else
            {
                voteList.Add(vote);
            }
        }

        if (@event.ExiledPlayer != null)
        {
            @event.ExiledPlayer = VotingUtils.GetExiled(voteList, out _);
        }
    }

    private static IEnumerator PerformSwaps()
    {
        var swapperRoles = CustomRoleUtils.GetActiveRolesOfType<SwapperRole>().ToList();

        var duration = 4f / (swapperRoles.Count + 1);

        foreach (var role in swapperRoles)
        {
            if (role == null || role.Player.HasDied() || role.Swap1 == null || role.Swap2 == null)
            {
                yield break;
            }

            var swapPlayer1 = role.Swap1.GetPlayer();
            var swapPlayer2 = role.Swap2.GetPlayer();

            if (swapPlayer1!.HasDied() || swapPlayer2!.HasDied())
            {
                yield break;
            }

            var elements1 = GetUIElements(role.Swap1);
            var elements2 = GetUIElements(role.Swap2);

            var votes1 = GetVoteTransforms(role.Swap1);
            var votes2 = GetVoteTransforms(role.Swap2);

            votes2.ForEach(vote =>
                vote.GetComponent<SpriteRenderer>().material.SetInt(PlayerMaterial.MaskLayer, role.Swap1.MaskLayer));
            votes1.ForEach(vote =>
                vote.GetComponent<SpriteRenderer>().material.SetInt(PlayerMaterial.MaskLayer, role.Swap2.MaskLayer));

            for (var i = 0; i < elements1.Length; i++)
            {
                Coroutines.Start(Slide2D(elements1[i], elements1[i].position, elements2[i].position, duration));
                Coroutines.Start(Slide2D(elements2[i], elements2[i].position, elements1[i].position, duration));
            }

            yield return new WaitForSeconds(duration);
        }
    }

    private static Transform[] GetUIElements(PlayerVoteArea voteArea)
    {
        return
        [
            voteArea.PlayerIcon.transform,
            voteArea.NameText.transform,
            voteArea.Background.transform,
            voteArea.MaskArea.transform,
            voteArea.PlayerButton.transform,
            voteArea.LevelNumberText.transform,
            voteArea.ColorBlindName.transform,
            voteArea.Overlay.transform,
            voteArea.Megaphone.transform
        ];
    }

    private static List<Transform> GetVoteTransforms(PlayerVoteArea voteArea)
    {
        var votes = new List<Transform>();
        for (var i = 0; i < voteArea.transform.childCount; i++)
        {
            var child = voteArea.transform.GetChild(i);
            if (child.name == "playerVote(Clone)")
            {
                votes.Add(child);
            }
        }

        return votes;
    }

    private static IEnumerator Slide2D(Transform target, Vector3 source, Vector3 dest, float duration)
    {
        yield return MiscUtils.PerformTimedAction(duration, p => target.position = Vector3.Lerp(source, dest, p));
    }
}