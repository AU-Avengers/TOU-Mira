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
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class JesterEvents
{
    [RegisterEvent(0)]
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

            if (jester.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You have successfully won as the {TownOfUsColors.Jester.ToTextColor()}{TouLocale.Get(TouNames.Jester, "Jester")}</color>, by getting voted out!</b>",
                    Color.white, spr: TouRoleIcons.Jester.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
                if (OptionGroupSingleton<JesterOptions>.Instance.JestWin is JestWinOptions.Haunts)
                {
                    PlayerControl.LocalPlayer.RpcAddModifier<IndirectAttackerModifier>(true);
                    CustomButtonSingleton<JesterHauntButton>.Instance.SetActive(true, jester);
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "null", -1, DeathHandlerOverride.SetTrue, lockInfo: DeathHandlerOverride.SetTrue);
                    var notif2 = Helpers.CreateAndShowNotification(
                        $"<b>You have one round to haunt a player of your choice to death, choose wisely.</b>",
                        Color.white);

                    notif2.Text.SetOutlineThickness(0.35f);
                    notif2.transform.localPosition = new Vector3(0f, 0.85f, -20f);
                }
                else
                {
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "null", -1, DeathHandlerOverride.SetFalse, lockInfo: DeathHandlerOverride.SetTrue);
                }
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Jester.ToTextColor()}{TouLocale.Get(TouNames.Jester, "Jester")}</color>, {jester.Player.Data.PlayerName}, has successfully won, as they were voted out!</b>",
                    Color.white, spr: TouRoleIcons.Jester.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        foreach (var jester in CustomRoleUtils.GetActiveRolesOfType<JesterRole>())
        {
            if (!jester.AboutToWin) jester.Voters.Clear();
        }
    }
    
    [RegisterEvent]
    public static void HandleVoteEventHandler(HandleVoteEvent @event)
    {
        var votingPlayer = @event.Player;
        var suspectPlayer = @event.TargetPlayerInfo;

        if (suspectPlayer?.Role is not JesterRole jester)
        {
            return;
        }

        jester.Voters.Add(votingPlayer.PlayerId);
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
            if (!allVoters.Any())
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
}