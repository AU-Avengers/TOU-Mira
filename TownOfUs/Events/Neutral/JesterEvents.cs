using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class JesterEvents
{
    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason != DeathReason.Exile)
        {
            return;
        }

        if (@event.Player.GetRoleWhenAlive() is JesterRole jester && jester.AboutToWin)
        {
            jester.Voted = true;

            if (OptionGroupSingleton<JesterOptions>.Instance.JestWin is JestWinOptions.EndsGame)
            {
                return;
            }

            jester.SentWinMsg = true;
            var jestRoleName = TouLocale.Get("TouRoleJester");
            if (jester.Player.AmOwner)
            {
                var text = TouLocale.GetParsed("TouNotifJesterWinOwner");
                if (text.Contains(jestRoleName))
                {
                    text = text.Replace(jestRoleName, $"{TownOfUsColors.Jester.ToTextColor()}{jestRoleName}</color>");
                }

                var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                    spr: TouRoleIcons.Jester.LoadAsset());

                notif1.AdjustNotification();
                if (OptionGroupSingleton<JesterOptions>.Instance.JestWin is JestWinOptions.Haunts)
                {
                    CustomButtonSingleton<JesterHauntButton>.Instance.SetActive(true, jester);
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "null", HudManagerHelper.Instance.CurrentRound,
                        DeathHandlerOverride.SetTrue, lockInfo: DeathHandlerOverride.SetTrue);
                    var notif2 = Helpers.CreateAndShowNotification(TouLocale.GetParsed("TouNotifJesterHauntOwner"),
                        Color.white, new Vector3(0f, 0.85f, -20f));
                    notif2.AdjustNotification();
                }
                else
                {
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "null", HudManagerHelper.Instance.CurrentRound,
                        DeathHandlerOverride.SetFalse, lockInfo: DeathHandlerOverride.SetTrue);
                }
            }
            else if (OptionGroupSingleton<JesterOptions>.Instance.JestAnnounceWin)
            {
                var text = TouLocale.GetParsed("TouNotifJesterWinGlobal");
                if (text.Contains(jestRoleName))
                {
                    text = text.Replace(jestRoleName, $"{TownOfUsColors.Jester.ToTextColor()}{jestRoleName}</color>");
                }

                if (text.Contains("<player>"))
                {
                    text = text.Replace("<player>", jester.Player.Data.PlayerName);
                }

                var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                    spr: TouRoleIcons.Jester.LoadAsset());
                notif1.AdjustNotification();
            }
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent _)
    {
        foreach (var jester in CustomRoleUtils.GetActiveRolesOfType<JesterRole>())
        {
            if (!jester.AboutToWin)
            {
                jester.Voters.Clear();
            }
        }
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent _)
    {
        var states = MeetingHudGetVotesPatch.States;
        var jests = CustomRoleUtils.GetActiveRolesOfType<JesterRole>();
        if (!jests.HasAny())
        {
            return;
        }
        foreach (var state in states)
        {
            if (state.SkippedVote || state.AmDead)
            {
                continue;
            }
            foreach (var jest in jests)
            {
                if (jest.Player.PlayerId == state.VotedForId)
                {
                    jest.Voters.Add(state.VoterId);
                }
            }
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        if (exiled == null || exiled.Data.Role is not JesterRole jest)
        {
            return;
        }

        jest.SentWinMsg = false;
        jest.AboutToWin = true;
        if (!PlayerControl.LocalPlayer.IsHost())
        {
            jest.Voted = true;
        }

        if (jest.Player.AmOwner && OptionGroupSingleton<JesterOptions>.Instance.JestWin is JestWinOptions.Haunts)
        {
            var allVoters = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => jest.Voters.Contains(x.PlayerId) && !x.AmOwner);
            if (!allVoters.HasAny())
            {
                return;
            }

            foreach (var player in allVoters)
            {
                player.AddModifier<MisfortuneTargetModifier>();
            }

            CustomButtonSingleton<JesterHauntButton>.Instance.Show = true;
        }
    }

    [RegisterEvent]
    public static void PlumberFlushJesterHandler(TouAbilityEvent @event)
    {
        if (@event.AbilityType is not AbilityType.PlumberFlush)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is not JesterRole ||
            !PlayerControl.LocalPlayer.inVent)
        {
            return;
        }

        var ventButton = CustomButtonSingleton<JesterVentButton>.Instance;
        if (ventButton.EffectActive || !ventButton.HasEffect)
        {
            ventButton.EffectActive = false;
            ventButton.Timer = ventButton.Cooldown;
        }
    }
}