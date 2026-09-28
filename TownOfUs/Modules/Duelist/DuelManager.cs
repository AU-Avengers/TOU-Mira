using System.Collections;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral.NeutralOutlier;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral.NeutralOutlier;
using UnityEngine;

namespace TownOfUs.Modules.Duelist;

public enum DuelOutcome : byte
{
    DuelistWon,
    OpponentWon,
    Tie,
}

public static class DuelManager
{
    public const float TieWindow = 0.10f;

    private static readonly Dictionary<byte, int> Wins = new();
    private static readonly Dictionary<byte, int> Losses = new();
    private static readonly HashSet<byte> ActiveDuelers = new();
    private static readonly HashSet<byte> DuelDeaths = new();
    private static readonly HashSet<byte> Resolved = new();
    private static readonly HashSet<byte> Struck = new();
    private static readonly HashSet<byte> Resolving = new();
    private static readonly Dictionary<byte, byte> Sanctioned = new();

    public static bool IsInDuel(byte playerId) => ActiveDuelers.Contains(playerId);

    public static void MarkInDuel(byte playerId)
    {
        ActiveDuelers.Add(playerId);
        Resolved.Remove(playerId);
        Struck.Remove(playerId);
        Resolving.Remove(playerId);
        Sanctioned.Remove(playerId);
    }

    public static void ClearActiveDuelers()
    {
        ActiveDuelers.Clear();
        Resolved.Clear();
        Struck.Clear();
        Resolving.Clear();
        Sanctioned.Clear();
    }

    public static bool IsResolved(byte playerId) => Resolved.Contains(playerId);
    public static void MarkResolved(byte a, byte b)
    {
        Resolved.Add(a);
        Resolved.Add(b);
    }

    public static bool HasStruck(byte playerId) => Struck.Contains(playerId);
    public static void MarkStruck(byte playerId) => Struck.Add(playerId);

    public static bool IsSanctionedKill(byte killer, byte victim) =>
        Sanctioned.TryGetValue(killer, out var v) && v == victim;

    public static void SanctionKill(byte killer, byte victim) => Sanctioned[killer] = victim;

    public static bool IsDuelUnresolved => ActiveDuelers.Count > 0 || Sanctioned.Count > 0;

    public static void HostBeginResolve(PlayerControl striker, PlayerControl opponent)
    {
        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }
        if (Resolving.Contains(striker.PlayerId) || Resolving.Contains(opponent.PlayerId))
        {
            return;
        }
        if (IsResolved(striker.PlayerId) || IsResolved(opponent.PlayerId))
        {
            return;
        }

        Resolving.Add(striker.PlayerId);
        Resolving.Add(opponent.PlayerId);
        Coroutines.Start(CoResolveStrike(striker, opponent));
    }

    private static IEnumerator CoResolveStrike(PlayerControl a, PlayerControl b)
    {
        var elapsed = 0f;
        while (elapsed < TieWindow && !(HasStruck(a.PlayerId) && HasStruck(b.PlayerId)))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Resolving.Remove(a.PlayerId);
        Resolving.Remove(b.PlayerId);

        if (a == null || b == null || !a.TryGetComponent<ModifierComponent>(out _) ||
            !b.TryGetComponent<ModifierComponent>(out _) ||
            !a.TryGetModifier<DuelModifier>(out var am) || !b.HasModifier<DuelModifier>())
        {
            yield break;
        }
        if (IsResolved(a.PlayerId) || IsResolved(b.PlayerId))
        {
            yield break;
        }

        var duelist = am.IsDuelist ? a : b;
        var opponent = am.IsDuelist ? b : a;

        var duelistStruck = HasStruck(duelist.PlayerId);
        var opponentStruck = HasStruck(opponent.PlayerId);

        DuelOutcome outcome;
        if (duelistStruck && opponentStruck)
        {
            outcome = DuelOutcome.Tie;
        }
        else if (duelistStruck)
        {
            outcome = DuelOutcome.DuelistWon;
        }
        else if (opponentStruck)
        {
            outcome = DuelOutcome.OpponentWon;
        }
        else
        {
            yield break;
        }

        DuelistRpc.RpcResolveDuel(duelist, opponent.PlayerId, (byte)outcome);
    }

    public static bool DiedInDuel(byte playerId) => DuelDeaths.Contains(playerId);
    public static void MarkDuelDeath(byte playerId) => DuelDeaths.Add(playerId);

    public static int GetWins(byte playerId) => Wins.TryGetValue(playerId, out var v) ? v : 0;
    public static int GetLosses(byte playerId) => Losses.TryGetValue(playerId, out var v) ? v : 0;
    public static void AddWin(byte playerId) => Wins[playerId] = GetWins(playerId) + 1;
    public static void AddLoss(byte playerId) => Losses[playerId] = GetLosses(playerId) + 1;

    public static void RefundLoss(byte playerId)
    {
        if (Losses.TryGetValue(playerId, out var v) && v > 0)
        {
            Losses[playerId] = v - 1;
        }
        DuelDeaths.Remove(playerId);
    }

    public static void ResetAll()
    {
        Wins.Clear();
        Losses.Clear();
        ActiveDuelers.Clear();
        DuelDeaths.Clear();
        Resolved.Clear();
        Struck.Clear();
        Resolving.Clear();
        Sanctioned.Clear();
    }

    public static bool TryGetDuelDestinations(PlayerControl duelist, PlayerControl target,
        out Vector2 duelistDest, out Vector2 targetDest)
    {
        duelistDest = duelist.GetTruePosition();
        targetDest = target.GetTruePosition();

        if (ShipStatus.Instance == null)
        {
            return false;
        }

        var spawnType = (DuelSpawnType)OptionGroupSingleton<DuelistOptions>.Instance.SpawnType.Value;

        if (spawnType == DuelSpawnType.Far &&
            DuelRoomPositions.TryGetFarRooms(out var farA, out var farB) &&
            TryGetRoomDest(farA, out var farDestA) && TryGetRoomDest(farB, out var farDestB))
        {
            duelistDest = farDestA;
            targetDest = farDestB;
            return true;
        }

        if (spawnType == DuelSpawnType.Random && TryGetRandomDestinations(out var randA, out var randB))
        {
            duelistDest = randA;
            targetDest = randB;
            return true;
        }

        var from = duelist.GetTruePosition();
        var currentRoom = GetRoomAt(from);

        var rooms = new List<PlainShipRoom>();
        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room == null || room.roomArea == null)
            {
                continue;
            }
            if (currentRoom != null && room.RoomId == currentRoom.RoomId)
            {
                continue;
            }
            rooms.Add(room);
        }

        if (rooms.Count < 2)
        {
            return false;
        }

        rooms.Sort((a, b) =>
            Vector2.Distance(from, a.roomArea.bounds.center)
                .CompareTo(Vector2.Distance(from, b.roomArea.bounds.center)));

        duelistDest = GetRoomDest(rooms[0]);
        targetDest = GetRoomDest(rooms[1]);
        return true;
    }

    private static bool TryGetRoomDest(SystemTypes room, out Vector2 dest)
    {
        dest = Vector2.zero;

        if (DuelRoomPositions.TryGet(room, out var pos))
        {
            dest = new Vector2(pos.x, pos.y + 0.3636f);
            return true;
        }

        foreach (var r in ShipStatus.Instance.FastRooms.Values)
        {
            if (r != null && r.roomArea != null && r.RoomId == room)
            {
                dest = r.roomArea.bounds.center;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetRandomDestinations(out Vector2 destA, out Vector2 destB)
    {
        destA = Vector2.zero;
        destB = Vector2.zero;

        var rooms = DuelRoomPositions.GetAllRooms();
        rooms.Shuffle();

        var found = new List<Vector2>();
        foreach (var room in rooms)
        {
            if (TryGetRoomDest(room, out var dest))
            {
                found.Add(dest);
            }
            if (found.Count == 2)
            {
                break;
            }
        }

        if (found.Count < 2)
        {
            return false;
        }

        destA = found[0];
        destB = found[1];
        return true;
    }

    private static PlainShipRoom? GetRoomAt(Vector2 pos)
    {
        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room != null && room.roomArea != null && room.roomArea.OverlapPoint(pos))
            {
                return room;
            }
        }
        return null;
    }

    private static Vector2 GetRoomDest(PlainShipRoom room)
    {
        return DuelRoomPositions.TryGet(room.RoomId, out var pos)
            ? new Vector2(pos.x, pos.y + 0.3636f)
            : room.roomArea.bounds.center;
    }

    public static void EndDuel(PlayerControl winner, PlayerControl loser, bool loserDied)
    {
        if (winner == null || loser == null)
        {
            return;
        }

        ShowResultNotifs(winner, loser);
        Coroutines.Start(CoEndDuel(winner, loser, loserDied));
    }

    public static void EndDuelTie(PlayerControl duelist, PlayerControl opponent)
    {
        if (duelist == null || opponent == null)
        {
            return;
        }

        ShowTieNotifs(duelist, opponent);
        Coroutines.Start(CoEndDuel(duelist, opponent, false));
    }

    private static IEnumerator CoEndDuel(PlayerControl winner, PlayerControl loser, bool loserDied)
    {
        yield return new WaitForSeconds(0.5f);

        if (winner == null || loser == null || AmongUsClient.Instance == null ||
            AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
        {
            yield break;
        }

        if (winner.TryGetComponent<ModifierComponent>(out _) && winner.TryGetModifier<DuelModifier>(out var winnerMod))
        {
            TeleportBack(winner, winnerMod.ReturnPos);
        }

        if (!loserDied && loser.TryGetComponent<ModifierComponent>(out _) &&
            loser.TryGetModifier<DuelModifier>(out var loserMod))
        {
            TeleportBack(loser, loserMod.ReturnPos);
        }

        ApplyReturnInvisibility(winner);
        ApplyReturnInvisibility(loser);

        RemoveDuel(winner);
        RemoveDuel(loser);

        ShowReturnNotif(winner, loser);
    }

    private static void ApplyReturnInvisibility(PlayerControl player)
    {
        if (player == null || player.HasDied())
        {
            return;
        }

        if (player.TryGetComponent<ModifierComponent>(out var comp) && !player.HasModifier<DuelReturnInvisModifier>())
        {
            comp.AddModifier(new DuelReturnInvisModifier());
        }
    }

    private static void ShowReturnNotif(PlayerControl winner, PlayerControl loser)
    {
        Coroutines.Start(CoReturnCountdown(winner, loser));
    }

    private static IEnumerator CoReturnCountdown(PlayerControl winner, PlayerControl loser)
    {
        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var icon = DivaniAssets.DuelistIcon.LoadAsset();

        for (var seconds = 5; seconds >= 1; seconds--)
        {
            var local = PlayerControl.LocalPlayer;
            if (local == null || local.HasDied() ||
                (local.PlayerId != winner.PlayerId && local.PlayerId != loser.PlayerId))
            {
                yield break;
            }

            var key = seconds == 1
                ? "TownOfUsMira.Role.Duelist.Notification.ReturningSingular"
                : "TownOfUsMira.Role.Duelist.Notification.ReturningPlural";

            var text = MiraLocaleManager
                .Get(key)
                .Replace("<seconds>", seconds.ToString());

            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>{text}</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: icon);

            yield return new WaitForSeconds(1f);
        }
    }

    public static void AbortDuel(PlayerControl player)
    {
        if (player == null || !player.TryGetComponent<ModifierComponent>(out _) ||
            !player.TryGetModifier<DuelModifier>(out var mod))
        {
            return;
        }

        var opponent = MiscUtils.PlayerById(mod.OpponentId);
        TeleportBack(player, mod.ReturnPos);
        RemoveDuel(player);
        if (opponent != null)
        {
            RemoveDuel(opponent);
        }
    }

    private static void ShowResultNotifs(PlayerControl winner, PlayerControl loser)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var icon = DivaniAssets.DuelistIcon.LoadAsset();

        if (local.PlayerId == winner.PlayerId)
        {
             var text =
                MiraLocaleManager.Get("TownOfUsMira.Role.Duelist.Notification.WonDuel");
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>{text}</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: icon);
            Coroutines.Start(MiscUtils.CoFlash(Color.green, alpha: 0.4f));
        }
        else if (local.PlayerId == loser.PlayerId)
        {
            var text =
                MiraLocaleManager.Get("TownOfUsMira.Role.Duelist.Notification.LostDuel");

            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>{text}</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: icon);
            Coroutines.Start(MiscUtils.CoFlash(Color.red, alpha: 0.4f));
        }
    }

    private static void ShowTieNotifs(PlayerControl duelist, PlayerControl opponent)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || (local.PlayerId != duelist.PlayerId && local.PlayerId != opponent.PlayerId))
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var icon = DivaniAssets.DuelistIcon.LoadAsset();

        var text =
            MiraLocaleManager.Get("TownOfUsMira.Role.Duelist.Notification.Draw");
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{hex}>{text}</color></b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: icon);
        Coroutines.Start(MiscUtils.CoFlash(Color.grey, alpha: 0.4f));
    }

    private static void TeleportBack(PlayerControl player, Vector2 pos)
    {
        if (player == null || player.HasDied())
        {
            return;
        }

        player.MyPhysics.ResetMoveState();
        player.transform.position = pos;
        if (player.AmOwner)
        {
            player.NetTransform.RpcSnapTo(pos);
            MiscUtils.SnapPlayerCamera(player);
        }
    }

    private static void RemoveDuel(PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        ActiveDuelers.Remove(player.PlayerId);
        Resolved.Remove(player.PlayerId);
        Struck.Remove(player.PlayerId);
        Resolving.Remove(player.PlayerId);
        Sanctioned.Remove(player.PlayerId);
        if (player.TryGetComponent<ModifierComponent>(out _) && player.TryGetModifier<DuelModifier>(out var mod))
        {
            player.RemoveModifier(mod);
        }
    }
}
