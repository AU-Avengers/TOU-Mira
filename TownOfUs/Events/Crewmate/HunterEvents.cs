using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class HunterEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is HunterRole &&
            OptionGroupSingleton<HunterOptions>.Instance.TaskUses)
        {
            var button = CustomButtonSingleton<HunterStalkButton>.Instance;
            ++button.UsesLeft;
            ++button.ExtraUses;
            button.SetUses(button.UsesLeft);
        }
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button;
        var source = PlayerControl.LocalPlayer;

        if (button == null || !button.CanClick())
        {
            return;
        }

        CheckForHunterStalked(source);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        CheckForHunterStalked(source);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        CheckForHunterStalked(source);

        if (source.Data.Role is not HunterRole)
        {
            return;
        }

        if (source.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            return;
        }

        var target = @event.Target;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (!target.IsCrewmate() ||
                (target.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished))
            {
                stats.CorrectKills += 1;
            }
            else if (source != target)
            {
                stats.IncorrectKills += 1;
            }
        }
    }

    [RegisterEvent]
    public static void HandleVoteEventHandler(HandleVoteEvent @event)
    {
        if (!OptionGroupSingleton<HunterOptions>.Instance.RetributionOnVote)
        {
            return;
        }

        var votingPlayer = @event.Player;
        var suspectPlayer = @event.TargetPlayerInfo;

        if (suspectPlayer?.Role is not HunterRole hunter)
        {
            return;
        }

        if (votingPlayer.Data.Role is HunterRole)
        {
            return;
        }

        hunter.LastVoted = votingPlayer;
    }


    [RegisterEvent(300)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (!OptionGroupSingleton<HunterOptions>.Instance.RetributionOnVote)
        {
            return;
        }

        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        if (exiled == null || exiled.Data.Role is not HunterRole hunter)
        {
            return;
        }

        var target = hunter.LastVoted!;
        var pros = CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>().FirstOrDefault();
        if (pros != null && pros.HasProsecuted)
        {
            target = pros.Player;
        }

        HunterRole.Retribution(hunter.Player, target);
    }

    private static void CheckForHunterStalked(PlayerControl source)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!source.HasModifier<HunterStalkedModifier>())
        {
            return;
        }

        var mod = source.GetModifier<HunterStalkedModifier>();

        if (mod?.Hunter == null || !source.AmOwner)
        {
            return;
        }

        HunterRole.RpcCatchPlayer(mod.Hunter, source);
    }
}