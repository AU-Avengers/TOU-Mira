using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class MarshalEvents
{
    private static byte _previousVote;

    private static List<CustomVote> GetVotesRecievied(this PlayerControl player)
    {
        List<CustomVote> votes = new();
        foreach (PlayerControl plr in PlayerControl.AllPlayerControls)
        {
            var voteData = plr.GetVoteData();
            var validVotes = voteData.Votes.Where(x => x.Suspect == player.PlayerId);
            votes.AddRange(validVotes);
        }

        return votes;
    }

    private static SpriteRenderer CustomBloopAVoteIcon(NetworkedPlayerInfo voterPlayer, int index, Transform parent)
    {
        var instance = MeetingHud.Instance;
        SpriteRenderer spriteRenderer = GameObject.Instantiate<SpriteRenderer>(instance.PlayerVotePrefab);
        spriteRenderer.gameObject.name = voterPlayer.PlayerId.ToString();
        if (GameManager.Instance.LogicOptions.GetAnonymousVotes())
        {
            PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
        }
        else
        {
            PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
        }
        spriteRenderer.transform.SetParent(parent);
        spriteRenderer.transform.localScale = Vector3.zero;
        PlayerVoteArea component = parent.GetComponent<PlayerVoteArea>();
        if (component != null)
        {
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
        }
        instance.StartCoroutine(Effects.Bloop((float)index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
        parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
        return spriteRenderer;
    }

    private static void UpdateVoteNumbers()
    {
        
    }
    
    [RegisterEvent]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        if (!MarshalRole.TribunalHappening) return;
        
        if (_previousVote != null)
        {
            var previousState =
                MeetingHud.Instance.playerStates.FirstOrDefault(state => state.TargetPlayerId == _previousVote);

            if (previousState != null)
            {
                previousState.ThumbsDown.enabled = false;
                var renderers = previousState.transform
                    .GetComponentsInChildren<SpriteRenderer>()
                    .Select(r => r.gameObject);
                foreach (var obj in renderers)
                {
                    if (obj.name == @event.Player.PlayerId.ToString())
                    {
                        previousState.GetComponent<VoteSpreader>().Votes.Remove(obj.GetComponent<SpriteRenderer>());
                        obj.DestroyImmediate();
                    }
                        
                }
            }
        }
        
        
        var targetState = MeetingHud.Instance.playerStates.FirstOrDefault(state => state.TargetPlayerId == @event.TargetId);
        
        var voterVoteData = @event.Player.GetVoteData();
        var target = MiscUtils.PlayerById(@event.TargetId);
        voterVoteData.Votes.Clear();
        for (int i = 0; i < voterVoteData.VotesRemaining; i++)
        {
            CustomBloopAVoteIcon(@event.Player.Data, 0, targetState.transform);
            voterVoteData.VoteForPlayer(@event.TargetId);
        }
        
        _previousVote = @event.TargetId;
        @event.Cancel();
    }
    
    [RegisterEvent]
    public static void PopulateResultsEvent(PopulateResultsEvent @event)
    {
        if (!MarshalRole.TribunalHappening) return;
        
        @event.Cancel();
    }
    
    [RegisterEvent]
    public static void EndMeetingEvent(EndMeetingEvent @event)
    {
        MarshalRole.TribunalHappening = false;
    }
}