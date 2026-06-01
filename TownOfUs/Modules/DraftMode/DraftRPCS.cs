using Reactor.Networking.Attributes;
using UnityEngine;
using MiraAPI.Utilities;

namespace TownOfUs.Modules.DraftMode;

public static class DraftRpcs
{
    [MethodRpc((uint)TownOfUsRpc.DraftSubmitPick)]
    public static void RpcSubmitPick(PlayerControl sender, int index)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sender == null) return;
        DraftManager.SubmitPick(sender.PlayerId, (byte)index);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftStart)]
    public static void RpcStartDraft(PlayerControl sender, int totalSlots, byte[] playerIds, int[] slotNumbers)
    {
        if (AmongUsClient.Instance.AmHost) return;
        if (playerIds == null || slotNumbers == null) return;
        DraftManager.SetDraftStateFromHost(totalSlots, playerIds.ToList(), slotNumbers.ToList());
    }

    [MethodRpc((uint)TownOfUsRpc.DraftAnnounceTurn)]
    public static void RpcAnnounceTurn(PlayerControl sender, int turnNumber, int slot, byte pickerId, ushort[] roleIds)
    {
        if (AmongUsClient.Instance.AmHost) return;
        DraftManager.SetClientTurn(turnNumber, slot);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftSlotNotify)]
    public static void RpcSlotNotify(PlayerControl sender, int totalSlots, byte[] playerIds, int[] slotNumbers)
    {
        if (AmongUsClient.Instance.AmHost) return;
        if (playerIds == null || slotNumbers == null) return;
        DraftManager.UpdateSlotAssignments(totalSlots, playerIds, slotNumbers);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftPickerReady)]
    public static void RpcPickerReady(PlayerControl sender)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sender == null) return;
        DraftManager.NotifyPickerReady(sender.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftPickConfirmed)]
    public static void RpcPickConfirmed(PlayerControl sender, int slot, ushort roleId)
    {
        if (AmongUsClient.Instance.AmHost) return;
        DraftManager.ConfirmPick(slot, roleId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftForceRole)]
    public static void RpcForceRole(PlayerControl sender, string roleName, byte targetId)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (string.IsNullOrEmpty(roleName)) return;
        DraftManager.SetForcedDraftRole(roleName, targetId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftCancel)]
    public static void RpcCancelDraft(PlayerControl sender)
    {
        if (AmongUsClient.Instance.AmHost) return;
        DraftManager.Reset(cancelledBeforeCompletion: true);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftEnd)]
    public static void RpcEndDraft(PlayerControl sender)
    {
        DraftManager.Reset(cancelledBeforeCompletion: true);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftCreateNotif)]
    public static void RpcCreateNotif(PlayerControl sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        Helpers.CreateAndShowNotification(message, Color.white,
            new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Traitor.LoadAsset());
    }

    [MethodRpc((uint)TownOfUsRpc.DraftBroadcastRecap)]
    public static void RpcBroadcastRecap(PlayerControl sender, bool showRecap, string[] recapData)
    {
        if (AmongUsClient.Instance.AmHost) return;
        DraftManager.Reset(cancelledBeforeCompletion: false);
    }
}

public static class DraftNetworkHelper
{
    public static void SendPickToHost(int index)
    {
        if (AmongUsClient.Instance.AmHost)
            DraftManager.SubmitPick(PlayerControl.LocalPlayer.PlayerId, (byte)index);
        else
            DraftRpcs.RpcSubmitPick(PlayerControl.LocalPlayer, index);
    }

    public static void BroadcastDraftStart(int totalSlots, List<byte> playerIds, List<int> slotNumbers)
    {
        if (playerIds == null || slotNumbers == null) return;
        DraftManager.SetDraftStateFromHost(totalSlots, playerIds, slotNumbers);
        DraftRpcs.RpcStartDraft(PlayerControl.LocalPlayer, totalSlots, playerIds.ToArray(), slotNumbers.ToArray());
    }

    public static void BroadcastCreateNotif(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        Helpers.CreateAndShowNotification(message, Color.white,
            new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Traitor.LoadAsset());
        DraftRpcs.RpcCreateNotif(PlayerControl.LocalPlayer, message);
    }

    public static void SendTurnAnnouncement(int slot, byte playerId, List<ushort> roleIds, int turnNumber)
    {
        if (roleIds == null) return;
        DraftManager.SetClientTurn(turnNumber, slot);
        DraftRpcs.RpcAnnounceTurn(PlayerControl.LocalPlayer, turnNumber, slot, playerId, roleIds.ToArray());
    }

    public static void BroadcastSlotNotifications(int totalSlots, Dictionary<byte, int> pidToSlot)
    {
        if (pidToSlot == null) return;
        var pids = pidToSlot.Keys.ToArray();
        var slots = pidToSlot.Values.ToArray();
        DraftRpcs.RpcSlotNotify(PlayerControl.LocalPlayer, totalSlots, pids, slots);
    }

    public static void BroadcastPickConfirmed(int slot, ushort roleId)
    {
        DraftManager.ConfirmPick(slot, roleId);
        DraftRpcs.RpcPickConfirmed(PlayerControl.LocalPlayer, slot, roleId);
    }

    public static void NotifyPickerReady()
    {
        if (AmongUsClient.Instance.AmHost)
            DraftManager.NotifyPickerReady(PlayerControl.LocalPlayer.PlayerId);
        else
            DraftRpcs.RpcPickerReady(PlayerControl.LocalPlayer);
    }

    public static void SendForceRoleToHost(string roleName, byte targetId)
    {
        if (string.IsNullOrEmpty(roleName)) return;
        if (AmongUsClient.Instance.AmHost)
            DraftManager.SetForcedDraftRole(roleName, targetId);
        else
            DraftRpcs.RpcForceRole(PlayerControl.LocalPlayer, roleName, targetId);
    }

    public static void BroadcastCancelDraft()
    {
        DraftRpcs.RpcCancelDraft(PlayerControl.LocalPlayer);
        DraftManager.Reset(cancelledBeforeCompletion: true);
    }

    public static void BroadcastRecap(List<RecapEntry> entries, bool showRecap)
    {
        var recapData = new List<string>();
        if (showRecap && entries != null)
        {
            foreach (var e in entries)
            {
                recapData.Add(e.SlotNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
                recapData.Add(e.RoleName ?? string.Empty);
            }
        }

        DraftRpcs.RpcBroadcastRecap(PlayerControl.LocalPlayer, showRecap, recapData.ToArray());
        DraftManager.Reset(cancelledBeforeCompletion: false);
    }

    public static void BroadcastDraftEnd()
    {
        DraftRpcs.RpcEndDraft(PlayerControl.LocalPlayer);
        DraftManager.Reset(cancelledBeforeCompletion: true);
    }
}