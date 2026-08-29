using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = System.Object;

namespace TownOfUs.Events.Crewmate;

public static class ProsecutorEvents
{
    [RegisterEvent(1000)]
    public static void BeforeLocalVoteEvent(BeforeVoteEvent @event)
    {
        // Players who are dead can no longer vote, and dead player can't be voted either. Blackmailed players can't vote as well.
        var voteArea = @event.VoteArea;
        var votedPlayer = voteArea.GetPlayer();
        if (PlayerControl.LocalPlayer.HasDied() || (votedPlayer != null && votedPlayer.HasDied()) ||
            PlayerControl.LocalPlayer.TryGetModifier<BlackmailedModifier>(out var bm) && bm.IsVoteReady && !bm.AboutToVote)
        {
            @event.Cancel();
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is not ProsecutorRole prosecutor)
        {
            return;
        }

        if (voteArea.Parent.state is MeetingHud.MeetingStates.Proceeding or MeetingHud.MeetingStates.Results)
        {
            @event.Cancel();
            return;
        }

        if (voteArea == prosecutor.ProsecuteButton && !prosecutor.SelectingProsecuteVictim)
        {
            prosecutor.SelectingProsecuteVictim = true;
            @event.Cancel();
            return;
        }

        if (voteArea != prosecutor.ProsecuteButton && voteArea != MeetingHud.Instance.SkipVoteButton &&
            prosecutor.SelectingProsecuteVictim)
        {
            ProsecutorRole.RpcProsecute(PlayerControl.LocalPlayer, voteArea.PlayerId);
        }

        if (voteArea == MeetingHud.Instance.SkipVoteButton && prosecutor.SelectingProsecuteVictim)
        {
            prosecutor.SelectingProsecuteVictim = false;
            prosecutor.ProsecuteVictim = byte.MaxValue;
        }
    }

    [RegisterEvent]
    public static void VoteEvent(CheckForEndVotingEvent @event)
    {
        if (!@event.IsVotingComplete)
        {
            return;
        }

        var prosecutor = CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>()
            .FirstOrDefault(x => !x.Player.HasDied() && x.HasProsecuted && x.ProsecuteVictim != byte.MaxValue);

        if (prosecutor == null)
        {
            return;
        }

        if (prosecutor.ProsecutionsCompleted >=
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions)
        {
            return;
        }

        if (!ProsecutorRole.HasProsecutedBefore)
        {
            ProsecutorRole.RpcShowProsAnimation(PlayerControl.LocalPlayer);
        }

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            plr.GetVoteData().Votes.Clear();
            plr.GetVoteData().VotesRemaining = 0;
        }

        var prosdata = prosecutor.Player.GetVoteData();

        for (var i = 0; i < 5; i++)
        {
            prosdata.VoteForPlayer(prosecutor.ProsecuteVictim);
        }
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent _)
    {
        var prosecutor = CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>()
            .FirstOrDefault(x => !x.Player.HasDied() && x.HasProsecuted && x.ProsecuteVictim != byte.MaxValue);

        if (prosecutor == null)
        {
            return;
        }

        MeetingHud.Instance.wasOverruled = true;
        if (ProsecutorRole.HasProsecutedBefore)
        {
            var gameObject = UnityEngine.Object.Instantiate(MeetingHud.Instance.judgeGavelPrefab, MeetingHud.Instance.transform);
            JudgeGavel component = gameObject.GetComponent<JudgeGavel>();
            var tmp = component.text.transform.GetComponent<TextMeshPro>();
            component.text.Destroy();
            component.text = null;
            tmp.text = "The Prosecutor has spoken.";
        }
    }

    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        var target = @event.Target;
        foreach (var pros in CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>())
        {
            // if someone dies after the Prosecutor selected them, it will not be a valid prosecute
            if (pros.ProsecuteVictim == target.PlayerId)
            {
                pros.SelectingProsecuteVictim = false;
                pros.ProsecuteVictim = byte.MaxValue;
            }
        }
    }

    [RegisterEvent(400)]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var player = @event.ExileController.initData.networkedPlayer?.Object;

        foreach (var pros in CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>())
        {
            var hasProsecuted = pros.HasProsecuted;
            pros.Cleanup();

            if (player == null)
            {
                continue;
            }

            if (hasProsecuted)
            {
                ProsecutorRole.HasProsecutedBefore = true;
                GameHistory.UpdatePlayerDeathData(player.PlayerId, TouLocale.Get("DiedToProsecutor"), 0,
                    HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetFalse,
                    TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", pros.Player.Data.PlayerName),
                    lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);

                if (pros.Player.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
                {
                    return;
                }

                if (player.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished)
                {
                    return;
                }

                if (player.IsCrewmate() && player != pros.Player)
                {
                    if (OptionGroupSingleton<ProsecutorOptions>.Instance.ExileOnCrewmate)
                    {
                        if (pros.Player.TryGetModifier<CelebrityModifier>(out var celeb))
                        {
                            celeb.Announced = true;
                        }
                        GameHistory.UpdatePlayerDeathData(pros.Player.PlayerId, TouLocale.Get("DiedToPunishment"), 0,
                            HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetFalse,
                            lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);

                        pros.Player.Exiled();
                    }
                    else
                    {
                        pros.ProsecutionsCompleted =
                            (int)OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
                    }
                }
            }
        }
    }
}