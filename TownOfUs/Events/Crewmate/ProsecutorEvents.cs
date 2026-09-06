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

namespace TownOfUs.Events.Crewmate;

public static class ProsecutorEvents
{
    public static PlayerControl ProsToAnnounceNext;
    public static bool IsBadJudgement(PlayerControl pros, PlayerControl victim)
    {
        if (pros.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            return false;
        }

        if (victim.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished)
        {
            return false;
        }

        return victim.IsCrewmate();
    }
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

        if (voteArea != MeetingHud.Instance.SkipVoteButton && prosecutor.WantsToPros is ProsecuteToggleMode.ToggledOn)
        {
            ProsecutorRole.RpcProsecute(PlayerControl.LocalPlayer, voteArea.PlayerId);
            @event.Cancel();
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

        var toVote = prosecutor.ProsecuteVictim;
        if ((BadProsecuteResult)OptionGroupSingleton<ProsecutorOptions>.Instance.WrongfulProsecutionResult.Value is
            BadProsecuteResult.EjectPros && IsBadJudgement(prosecutor.Player, GameData.Instance.GetPlayerById(prosecutor.ProsecuteVictim).Object))
        {
            toVote = prosecutor.Player.PlayerId;
        }
        for (var i = 0; i < 5; i++)
        {
            prosdata.VoteForPlayer(toVote);
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
                GameHistory.UpdatePlayerDeathData(player.PlayerId, MiraLocaleManager.Get("DiedToProsecutor"), 0,
                    HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetFalse,
                    MiraLocaleManager.Get("DiedByStringBasic").Replace("<player>", pros.Player.Data.PlayerName),
                    lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);

                if (IsBadJudgement(pros.Player, player) && player != pros.Player)
                {
                    ProsToAnnounceNext = pros.Player;
                    if ((BadProsecuteResult)OptionGroupSingleton<ProsecutorOptions>.Instance.WrongfulProsecutionResult.Value is BadProsecuteResult.EjectProsAndEjectTarget)
                    {
                        if (pros.Player.TryGetModifier<CelebrityModifier>(out var celeb))
                        {
                            celeb.Announced = true;
                        }
                        GameHistory.UpdatePlayerDeathData(pros.Player.PlayerId, MiraLocaleManager.Get("DiedToPunishment"), 0,
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
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro || !ProsToAnnounceNext)
        {
            ProsToAnnounceNext = null!;
            return;
        }

        if ((BadProsecuteResult)OptionGroupSingleton<ProsecutorOptions>.Instance.WrongfulProsecutionResult.Value is
            BadProsecuteResult.EjectProsAndEjectTarget)
        {
            if (ProsToAnnounceNext.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.Prosecutor.ShameNotificationSelf"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Prosecutor.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.Prosecutor.ShameNotification").Replace("<player>", ProsToAnnounceNext.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Prosecutor.LoadAsset());

                notif1.AdjustNotification();
            }
        }
        else if (ProsToAnnounceNext.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("TownOfUsMira.Role.Prosecutor.ShameNotificationSelfPrivate"),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Prosecutor.LoadAsset());

            notif1.AdjustNotification();
        }
        ProsToAnnounceNext = null!;
    }
}