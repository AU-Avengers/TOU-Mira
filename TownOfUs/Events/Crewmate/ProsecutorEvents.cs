using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class ProsecutorEvents
{
    [RegisterEvent(1000)]
    public static void BeforeLocalVoteEvent(BeforeVoteEvent @event)
    {
        // Players who are dead can no longer vote, and dead player can't be voted either
        var voteArea = @event.VoteArea;
        var votedPlayer = voteArea.GetPlayer();
        if (PlayerControl.LocalPlayer.HasDied() || (votedPlayer != null && votedPlayer.HasDied()))
        {
            @event.Cancel();
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is not ProsecutorRole prosecutor)
        {
            return;
        }

        if (voteArea.Parent.state is MeetingHud.VoteStates.Proceeding or MeetingHud.VoteStates.Results)
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
            ProsecutorRole.RpcProsecute(PlayerControl.LocalPlayer, voteArea.TargetPlayerId);
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
                DeathHandlerModifier.UpdateDeathHandler(player, TouLocale.Get("DiedToProsecutor"),
                    DeathEventHandlers.CurrentRound,
                    DeathHandlerOverride.SetFalse,
                    TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", pros.Player.Data.PlayerName),
                    lockInfo: DeathHandlerOverride.SetTrue);

                if (pros.Player.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
                {
                    return;
                }

                if (player.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished)
                {
                    return;
                }

                if (player.IsCrewmate())
                {
                    if (OptionGroupSingleton<ProsecutorOptions>.Instance.ExileOnCrewmate)
                    {
                        if (pros.Player.TryGetModifier<CelebrityModifier>(out var celeb))
                        {
                            celeb.Announced = true;
                        }

                        DeathHandlerModifier.UpdateDeathHandler(pros.Player, TouLocale.Get("DiedToPunishment"),
                            DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
                            lockInfo: DeathHandlerOverride.SetTrue);

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