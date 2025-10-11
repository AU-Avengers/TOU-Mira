using HarmonyLib;
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
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class ExecutionerEvents
{
    [RegisterEvent(0)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason is DeathReason.Exile)
        {
            var victim = @event.Player;
            if (!victim.TryGetModifier<ExecutionerTargetModifier>(out var exeMod))
            {
                return;
            }

            var exe = GameData.Instance.GetPlayerById(exeMod.OwnerId).Object;
            if (exe != null && !exe.HasDied() && exe.Data.Role is ExecutionerRole exeRole && exeRole.AboutToWin)
            {
                if (victim.IsCrewmate())
                {
                    exeRole.TargetVoted = true;
                }
                else
                {
                    exeRole.TargetVotedAsEvil = true;
                }
            }
        }
        else
        {
            CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>().Do(x => x.CheckTargetDeath(@event.Player));
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        foreach (var executioner in CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>())
        {
            if (!executioner.AboutToWin)
            {
                executioner.Voters.Clear();
            }
        }

        if (@event.TriggeredByIntro)
        {
            return;
        }

        var winOption = OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin;
        
        var exe = CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>()
            .FirstOrDefault(x => x.AboutToWin && !x.Player.HasDied());

        if (winOption is ExeWinOptions.EndsGame)
        {
            if (exe != null && exe.Target != null && !exe.Target.IsCrewmate())
            {
                winOption = ExeWinOptions.Torments;
            }
            else
            {
                return;
            }
        }

        if (exe != null)
        {
            var victim = exe.Target!;
            if (victim.IsCrewmate())
            {
                exe.TargetVoted = true;
            }
            else
            {
                exe.TargetVotedAsEvil = true;
            }
            if (exe.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You have successfully won as the {TownOfUsColors.Executioner.ToTextColor()}Executioner</color>, as your target was exiled!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Executioner.LoadAsset());

                notif1.AdjustNotification();

                PlayerControl.LocalPlayer.RpcPlayerExile();

                if (winOption is ExeWinOptions.Torments)
                {
                    CustomButtonSingleton<ExeTormentButton>.Instance.SetActive(true, exe);
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer,
                        TouLocale.Get("DiedToWinning"), DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetTrue,
                        lockInfo: DeathHandlerOverride.SetTrue);
                    var notif2 = Helpers.CreateAndShowNotification(
                        $"<b>You have one round to torment a player of your choice to death, choose wisely.</b>",
                        Color.white, new Vector3(0f, 0.85f, -20f));
                    notif2.AdjustNotification();
                }
                else
                {
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer,
                        TouLocale.Get("DiedToWinning"), DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
                        lockInfo: DeathHandlerOverride.SetTrue);
                }
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Executioner.ToTextColor()}Executioner</color>, {exe.Player.Data.PlayerName}, has successfully won, as their target was exiled!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Executioner.LoadAsset());

                notif1.AdjustNotification();
            }
        }
    }

    [RegisterEvent]
    public static void HandleVoteEventHandler(HandleVoteEvent @event)
    {
        var votingPlayer = @event.Player;
        var suspectPlayer = @event.TargetPlayerInfo;

        if (suspectPlayer == null || !suspectPlayer._object.TryGetModifier<ExecutionerTargetModifier>(out var exeMod))
        {
            return;
        }

        var exe = GameData.Instance.GetPlayerById(exeMod.OwnerId).Object;
        if (exe != null && !exe.HasDied() && exe.Data.Role is ExecutionerRole exeRole)
        {
            exeRole.Voters.Add(votingPlayer.PlayerId);
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        if (exiled == null || !exiled.TryGetModifier<ExecutionerTargetModifier>(out var exeMod))
        {
            return;
        }

        var exe = GameData.Instance.GetPlayerById(exeMod.OwnerId).Object;
        if (exe != null && !exe.HasDied() && exe.Data.Role is ExecutionerRole exeRole)
        {
            exeRole.AboutToWin = true;
            if (!PlayerControl.LocalPlayer.IsHost())
            {
                if (exiled.IsCrewmate())
                {
                    exeRole.TargetVoted = true;
                }
                else
                {
                    exeRole.TargetVotedAsEvil = true;
                }
            }
            var winOption = OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin;

            if (!exiled.IsCrewmate() && winOption is ExeWinOptions.EndsGame)
            {
                winOption = ExeWinOptions.Torments;
            }

            if (exe.AmOwner && winOption is ExeWinOptions.Torments)
            {
                var allVoters = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => exeRole.Voters.Contains(x.PlayerId) && !x.AmOwner);

                if (!allVoters.Any())
                {
                    return;
                }

                foreach (var player in allVoters)
                {
                    player.AddModifier<MisfortuneTargetModifier>();
                }

                CustomButtonSingleton<ExeTormentButton>.Instance.Show = true;
            }
        }
    }
}