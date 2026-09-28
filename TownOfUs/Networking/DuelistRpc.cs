using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Translation;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Duelist;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral.NeutralOutlier;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Networking;

public static class DuelistRpc
{
    [MethodRpc((uint)TownOfUsRpc.DuelistStartDuel, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStartDuel(PlayerControl duelist, byte targetId,
        Vector2 duelistDest, Vector2 targetDest, Vector2 duelistReturn, Vector2 targetReturn)
    {
        if (duelist == null || duelist.Data == null || duelist.Data.Role is not DuelistRole)
        {
            return;
        }

        var target = MiscUtils.PlayerById(targetId);
        if (target == null || target.HasDied())
        {
            return;
        }

        DuelManager.MarkInDuel(duelist.PlayerId);
        DuelManager.MarkInDuel(target.PlayerId);

        if (duelist.TryGetComponent<ModifierComponent>(out var duelistComp))
        {
            duelistComp.AddModifier(new DuelModifier(targetId, true, duelistReturn));
            Teleport(duelist, duelistDest);
        }
        if (target.TryGetComponent<ModifierComponent>(out var targetComp))
        {
            targetComp.AddModifier(new DuelModifier(duelist.PlayerId, false, targetReturn));
            Teleport(target, targetDest);
        }

        ShowStartNotifs(duelist, target);
    }

    [MethodRpc((uint)TownOfUsRpc.DuelistStrike, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcDuelStrike(PlayerControl striker, byte opponentId)
    {
        if (striker == null || striker.HasDied() ||
            !striker.TryGetModifier<DuelModifier>(out var mod) || mod.OpponentId != opponentId)
        {
            return;
        }

        var opponent = MiscUtils.PlayerById(opponentId);
        if (opponent == null || opponent.HasDied() || !opponent.HasModifier<DuelModifier>())
        {
            return;
        }

        if (DuelManager.IsResolved(striker.PlayerId) || DuelManager.HasStruck(striker.PlayerId))
        {
            return;
        }

        DuelManager.MarkStruck(striker.PlayerId);
        DuelManager.HostBeginResolve(striker, opponent);
    }

    [MethodRpc((uint)TownOfUsRpc.DuelistResolveDuel, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcResolveDuel(PlayerControl duelist, byte opponentId, byte outcome)
    {
        if (duelist == null || duelist.Data == null || duelist.Data.Role is not DuelistRole)
        {
            return;
        }

        var opponent = MiscUtils.PlayerById(opponentId);
        if (opponent == null)
        {
            return;
        }

        if (DuelManager.IsResolved(duelist.PlayerId) || DuelManager.IsResolved(opponent.PlayerId))
        {
            return;
        }

        DuelManager.MarkResolved(duelist.PlayerId, opponent.PlayerId);

        var amHost = PlayerControl.LocalPlayer.IsHost();

        switch ((DuelOutcome)outcome)
        {
            case DuelOutcome.DuelistWon:
                DuelManager.AddWin(duelist.PlayerId);
                NotifyVictoryPending(duelist);
                DuelManager.SanctionKill(duelist.PlayerId, opponent.PlayerId);
                if (amHost)
                {
                    Coroutines.Start(CoSanctionedMurder(duelist, opponent));
                }
                break;

            case DuelOutcome.OpponentWon:
                DuelManager.AddLoss(duelist.PlayerId);
                var lossesToDie = (int)OptionGroupSingleton<DuelistOptions>.Instance.DuelsLostToDie.Value;
                if (DuelManager.GetLosses(duelist.PlayerId) >= lossesToDie)
                {
                    DuelManager.SanctionKill(opponent.PlayerId, duelist.PlayerId);
                    if (amHost)
                    {
                        Coroutines.Start(CoSanctionedMurder(opponent, duelist));
                    }
                }
                else
                {
                    DuelManager.EndDuel(opponent, duelist, false);
                }
                break;

            case DuelOutcome.Tie:
                DuelManager.EndDuelTie(duelist, opponent);
                break;
        }
    }

    private static void NotifyVictoryPending(PlayerControl duelist)
    {
        if (!duelist.AmOwner || duelist.Data?.Role is not DuelistRole { VictoryPending: true })
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var text = MiraLocaleManager.Get("TownOfUsMira.Role.Duelist.Notification.VictoryPending");
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{hex}>{text}</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.DuelistIcon.LoadAsset());
    }

    private static IEnumerator CoSanctionedMurder(PlayerControl killer, PlayerControl victim)
    {
        yield return null;
        yield return null;

        if (killer == null || victim == null || killer.HasDied() || victim.HasDied())
        {
            yield break;
        }

        killer.RpcCustomMurder(victim, MeetingCheck.OutsideMeeting);

        if (!victim.HasDied())
        {
            DuelManager.EndDuel(killer, victim, false);
        }
    }

    private static void ShowStartNotifs(PlayerControl duelist, PlayerControl target)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var icon = DivaniAssets.DuelistIcon.LoadAsset();
        var pos = new Vector3(0f, 1f, -20f);

        if (local.PlayerId == target.PlayerId)
        {
             var challengedText =
            MiraLocaleManager.Get("TownOfUsMira.Role.Duelist.Notification.Challenged");
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>{challengedText}</color></b>",
                Color.white, pos, spr: icon);
        }
        else if (local.PlayerId == duelist.PlayerId)
        {
            var duelStartedText = MiraLocaleManager
                .Get("TownOfUsMira.Role.Duelist.Notification.DuelStarted")
                .Replace("<player>", target.Data.PlayerName);
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>{duelStartedText}</color></b>",
                Color.white, pos, spr: icon);
        }
    }

    private static void Teleport(PlayerControl player, Vector2 dest)
    {

        if (player.HasModifier<ImmovableModifier>())
        {
            return;
        }

        if (player.inVent)
        {
            player.MyPhysics.ExitAllVents();
        }

        player.MyPhysics.ResetMoveState();
        player.transform.position = dest;

        if (player.AmOwner)
        {
            player.NetTransform.RpcSnapTo(dest);
            MiscUtils.SnapPlayerCamera(player);
        }
    }
}
