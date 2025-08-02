using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class MarshalEvents
{
    private static byte _previousVote;

    private static SpriteRenderer CustomBloopAVoteIcon(NetworkedPlayerInfo voterPlayer, int index, Transform parent)
    {
        var instance = MeetingHud.Instance;
        SpriteRenderer spriteRenderer = GameObject.Instantiate(instance.PlayerVotePrefab, parent);
        spriteRenderer.gameObject.name = voterPlayer.PlayerId.ToString();
        spriteRenderer.transform.localScale = Vector3.zero;
        
        if (GameManager.Instance.LogicOptions.GetAnonymousVotes())
            PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
        else
            PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
        
        PlayerVoteArea component = parent.GetComponent<PlayerVoteArea>();
        if (component != null)
        {
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
        }
        instance.StartCoroutine(Effects.Bloop(index * 0.3f, spriteRenderer.transform));
        parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
        return spriteRenderer;
    }
    
    [RegisterEvent]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        if (!MarshalRole.TribunalHappening) return;
        @event.Cancel();
        
        if (_previousVote != null)
        {
            var previousState =
                MeetingHud.Instance.playerStates.FirstOrDefault(state => state.TargetPlayerId == _previousVote);

            if (previousState != null)
            {
                previousState.ThumbsDown.enabled = false;
                MarshalRole.RemoveBloopsOfId(@event.Player.PlayerId, previousState);
            }
        }
        
        var targetState = MeetingHud.Instance.playerStates.FirstOrDefault(state => state.TargetPlayerId == @event.TargetId);
        var voterVoteData = @event.Player.GetVoteData();
        voterVoteData.Votes.Clear();
        for (int i = 0; i < voterVoteData.VotesRemaining; i++)
        {
            CustomBloopAVoteIcon(@event.Player.Data, 0, targetState.transform);
            voterVoteData.VoteForPlayer(@event.TargetId);
        }
        
        MarshalRole.UpdateVoteNumbers();
        if (AmongUsClient.Instance.AmHost)
            MarshalRole.CheckForEjection();
        _previousVote = @event.TargetId;
    }

    [RegisterEvent]
    public static void MeetingSelectEvent(MeetingSelectEvent @event)
    {
        if (!MarshalRole.TribunalHappening) return;

        // - Players marked as ejected during a tribunal can't vote
        // - Other players can't vote them
        if (MarshalRole.EjectedPlayers.Contains(PlayerControl.LocalPlayer.Data) || MarshalRole.EjectedPlayers.Contains(@event.TargetPlayerInfo))
        {
            @event.AllowSelect = false;
        }
    }
    
    [RegisterEvent]
    public static void PopulateResultsEvent(PopulateResultsEvent @event)
    {
        if (!MarshalRole.TribunalHappening) return;
        
        @event.Cancel();
    }
    
    [RegisterEvent]
    public static void VotingCompleteEvent(VotingCompleteEvent @event)
    {
        if (!MarshalRole.TribunalHappening) return;
        if (MarshalRole.EjectedPlayers.Count >=
            OptionGroupSingleton<MarshalOptions>.Instance.MaxTribunalEjections) return;
        
        MeetingHud.Instance.playerStates.Do(x => x.UnsetVote());
        
        if (AmongUsClient.Instance.AmHost)
            MarshalRole.RpcEndTribunal(PlayerControl.LocalPlayer);
    }

    [RegisterEvent]
    public static void BeforeRoundStartEvent(BeforeRoundStartEvent @event)
    {
        if (!MarshalRole.TribunalHappening) return;

        if (MarshalRole.EjectedPlayers.Count > 0)
        {
            @event.Cancel();
            Coroutines.Start(MarshalRole.CoEjectionCutscene(MarshalRole.EjectedPlayers[0]));
            MarshalRole.EjectedPlayers.RemoveAt(0);
        }
        else
        {
            MarshalRole.TribunalHappening = false;
        }
    }
}