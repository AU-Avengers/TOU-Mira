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
using MiraAPI.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class HunterEvents
{
    public static int ActiveStalkTaskCount;
    public static PlayerControl HunterToAnnounceNext;
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            if (!HunterToAnnounceNext)
            {
                return;
            }

            if (HunterToAnnounceNext.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.Hunter.ShameNotificationSelf"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hunter.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.Hunter.ShameNotification").Replace("<player>", HunterToAnnounceNext.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hunter.LoadAsset());

                notif1.AdjustNotification();
            }

            HunterToAnnounceNext = null!;
            return; // Only run when game starts.
        }

        HunterToAnnounceNext = null!;
        ActiveStalkTaskCount = 0;

        var hunterStalk = CustomButtonSingleton<HunterStalkButton>.Instance;
        hunterStalk.ExtraUses = 0;
        hunterStalk.SetUses((int)OptionGroupSingleton<HunterOptions>.Instance.StalkUses);
        if (!hunterStalk.LimitedUses)
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(false);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(true);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }
    }

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        var opt = OptionGroupSingleton<HunterOptions>.Instance;
        var stalkButton = CustomButtonSingleton<HunterStalkButton>.Instance;
        if (@event.Player.AmOwner)
        {
            ++ActiveStalkTaskCount;
            if (@event.Player.Data.Role is not HunterRole)
            {
                return;
            }

            if (stalkButton.LimitedUses &&
                opt.StalkPerTasks != 0 && opt.StalkPerTasks <= ActiveStalkTaskCount)
            {
                ++stalkButton.UsesLeft;
                ++stalkButton.ExtraUses;
                stalkButton.SetUses(stalkButton.UsesLeft);
                ActiveStalkTaskCount = 0;
            }
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

        CheckForHunterStalked(source, button is CustomActionButton<PlayerControl>);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        CheckForHunterStalked(source, true);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        CheckForHunterStalked(source, true);

        if (source.Data.Role is not HunterRole role)
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

        if (OptionGroupSingleton<HunterOptions>.Instance.SameRoundKillPunishment
            && !MeetingHud.Instance && !ExileController.Instance
            && IsIncorrectKill(source, target)
            && role.CaughtRounds.TryGetValue(target.PlayerId, out var caughtRound)
            && caughtRound == HudManagerHelper.Instance.CurrentRound)
        {
            role.PendingShame = true;
        }
    }

    private static bool IsIncorrectKill(PlayerControl hunter, PlayerControl target)
    {
        if (hunter.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            return false;
        }

        if (target.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished)
        {
            return false;
        }

        if (hunter == target)
        {
            return false;
        }

        return target.IsCrewmate();
    }

    [RegisterEvent(400)]
    public static void ShameWrapUpEvent(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        foreach (var hunter in CustomRoleUtils.GetActiveRolesOfType<HunterRole>())
        {
            if (!hunter.PendingShame)
            {
                continue;
            }

            hunter.PendingShame = false;

            if (hunter.Player.HasDied() || exiled == hunter.Player)
            {
                continue;
            }

            HunterToAnnounceNext = hunter.Player;
            if (hunter.Player.TryGetModifier<CelebrityModifier>(out var celeb))
            {
                celeb.Announced = true;
            }
            GameHistory.UpdatePlayerDeathData(hunter.Player.PlayerId, MiraLocaleManager.Get("DiedToPunishment"), 0,
                HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetFalse,
                lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);

            hunter.Player.Exiled();
        }
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent _)
    {
        if (!OptionGroupSingleton<HunterOptions>.Instance.RetributionOnVote)
        {
            return;
        }
        var states = MeetingHudGetVotesPatch.States;
        var hunters = CustomRoleUtils.GetActiveRolesOfType<HunterRole>();
        if (!hunters.HasAny())
        {
            return;
        }
        foreach (var state in states)
        {
            if (state.SkippedVote || state.AmDead)
            {
                continue;
            }
            foreach (var hunter in hunters)
            {
                var voter = MiscUtils.PlayerById(state.VoterId);
                if (hunter.Player.PlayerId != state.VoterId && hunter.Player.PlayerId == state.VotedForId && voter != null)
                {
                    hunter.LastVoted = voter;
                }
            }
        }
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

    private static void CheckForHunterStalked(PlayerControl source, bool isInteraction)
    {
        if (MeetingHud.Instance || ExileController.Instance || !isInteraction && (StalkTriggered)OptionGroupSingleton<HunterOptions>.Instance.StalkTriggeredBy.Value is StalkTriggered.Interactions)
        {
            return;
        }

        if (!source.HasModifier<HunterStalkedModifier>())
        {
            return;
        }

        var mod = source.GetModifier<HunterStalkedModifier>();

        if (mod?.Hunter == null || !(TutorialManager.InstanceExists || source.AmOwner))
        {
            return;
        }

        HunterRole.RpcCatchPlayer(source, mod.Hunter, isInteraction);
    }
}