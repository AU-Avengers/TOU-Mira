using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Networking;

public static class MiscTouRpcs
{
    /// <summary>
    /// Networked Revive method.
    /// </summary>
    /// <param name="player">The player to revive.</param>
    [MethodRpc((uint)TownOfUsRpc.BasicRevive, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBasicRevive(
        this PlayerControl player)
    {
        if (player.Data.IsDead)
        {
            GameHistory.ClearMurder(player);
            player.Revive();
        }
    }
    /// <summary>
    /// Networked Revive method.
    /// </summary>
    /// <param name="player">The player to revive.</param>
    /// <param name="isDead">Whether the player is meant to be dead or not.</param>
    /// <param name="pos">The player's set position.</param>
    /// <param name="newRoleType">The player's new role value.</param>
    /// <param name="recordRole">Whether to record the player's role change.</param>
    [MethodRpc((uint)TownOfUsRpc.FullRevive, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFullRevive(
        this PlayerControl player,
        bool isDead,
        Vector2 pos,
        ushort newRoleType,
        bool recordRole = true)
    {
        if (player.Data.IsDead && !isDead)
        {
            GameHistory.ClearMurder(player);
            player.Revive();
        }
        else if (!player.Data.IsDead && isDead)
        {
            player.CustomMurder(player, MurderResultFlags.Succeeded, false, false, false, false, false);
        }
        Utilities.Extensions.ChangeRole(player, newRoleType, recordRole);
        player.transform.position = pos;
        player.NetTransform.SnapTo(pos);
    }
}
