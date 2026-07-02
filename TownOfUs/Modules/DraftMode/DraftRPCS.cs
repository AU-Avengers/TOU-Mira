using Reactor.Networking.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;
using MiraAPI.Utilities;

namespace TownOfUs.Modules.DraftMode;

public static class DraftRpcs
{
    [MethodRpc((uint)TownOfUsRpc.DraftSubmitPick)]
    public static void RpcSubmitPick(PlayerControl sender, int index)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sender == null) return;
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcSubmitPick: player {sender.PlayerId} picked index {index}");
        DraftManager.SubmitPick(sender.PlayerId, (byte)index);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftRequestReroll)]
    public static void RpcRequestReroll(PlayerControl sender)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sender == null) return;
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcRequestReroll: player {sender.PlayerId}");
        DraftEngineBehaviour.Instance?.RequestReroll(sender.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftStart)]
    public static void RpcStartDraft(PlayerControl sender, int totalSlots)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcStartDraft received (isHost={AmongUsClient.Instance.AmHost})");
        DraftManager.IsDraftActive = true;
    }

    [MethodRpc((uint)TownOfUsRpc.DraftSlotNotify)]
    public static void RpcSlotNotify(PlayerControl sender, byte playerId, int slotNumber)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] Player {playerId} assigned to slot {slotNumber} (isHost={AmongUsClient.Instance.AmHost})");
        var state = DraftManager.GetStateForPlayer(playerId);
        if (state == null)
        {
            var newState = new DraftSlotState { PlayerId = playerId, SlotNumber = slotNumber };
            DraftManager.AddSlotState(newState);
        }
        else
        {
            state.SlotNumber = slotNumber;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.DraftAnnounceTurn)]
    public static void RpcAnnounceTurn(PlayerControl sender, int turnNumber, int slot, byte pickerId, byte offeredCount,
        ushort roleId1, ushort roleId2, ushort roleId3, ushort roleId4, ushort roleId5,
        ushort roleId6, ushort roleId7, ushort roleId8, ushort roleId9)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcAnnounceTurn: Turn {turnNumber}, Slot {slot}, PickerId {pickerId} (isHost={AmongUsClient.Instance.AmHost})");

        DraftManager.SetClientTurn(turnNumber, slot);
        var allIds = new[] { roleId1, roleId2, roleId3, roleId4, roleId5, roleId6, roleId7, roleId8, roleId9 };
        var count = Math.Clamp((int)offeredCount, 0, allIds.Length);
        var offeredList = new List<ushort>(count);
        for (int i = 0; i < count; i++)
            offeredList.Add(allIds[i]);

        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] Caching {offeredList.Count} offered roles");
        var draftScreenController = Object.FindObjectOfType<DraftScreenController>();
        draftScreenController?.CacheOfferedRoles(offeredList.ToArray());

        var localPlayerId = PlayerControl.LocalPlayer?.PlayerId ?? 255;
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] Checking if it's my turn. Local: {localPlayerId}, Picker: {pickerId}");

        if (localPlayerId == pickerId)
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] IT'S MY TURN! Showing picker screen with {offeredList.Count} roles");
            try
            {
                DraftScreenController.Show(offeredList.ToArray());
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftRpc] Picker screen shown successfully!");
            }
            catch (Exception e)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftRpc] Exception showing picker screen: {e}");
            }
        }
        else
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] Not my turn, just caching roles");
        }
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
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcPickConfirmed: slot {slot}, roleId {roleId}");
        DraftManager.ConfirmPick(slot, roleId);
        DraftScreenController.Hide();
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
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftRpc] RpcCancelDraft");
        DraftManager.Reset(cancelledBeforeCompletion: true);
        DraftScreenController.Hide();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftEnd)]
    public static void RpcEndDraft(PlayerControl sender)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftRpc] RpcEndDraft");
        DraftManager.Reset(cancelledBeforeCompletion: true);
        DraftScreenController.Hide();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftCreateNotif)]
    public static void RpcCreateNotif(PlayerControl sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        Helpers.CreateAndShowNotification(message, Color.white,
            new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Traitor.LoadAsset());
    }

    [MethodRpc((uint)TownOfUsRpc.DraftBroadcastRecap)]
    public static void RpcBroadcastRecap(PlayerControl sender, string recapData)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftRpc] RpcBroadcastRecap");
        DraftScreenController.Hide();
        DraftManager.Reset(cancelledBeforeCompletion: false);
    }
}

public static class DraftNetworkHelper
{
    public static void SendPickToHost(int index)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] SendPickToHost: index {index}");
        if (AmongUsClient.Instance.AmHost)
            DraftManager.SubmitPick(PlayerControl.LocalPlayer.PlayerId, (byte)index);
        else
            DraftRpcs.RpcSubmitPick(PlayerControl.LocalPlayer, index);
    }

    public static void BroadcastDraftStart(int totalSlots, List<byte> playerIds, List<int> slotNumbers)
    {
        if (playerIds == null || slotNumbers == null) return;
        
        DraftRpcs.RpcStartDraft(PlayerControl.LocalPlayer, totalSlots);
        
        for (int i = 0; i < playerIds.Count; i++)
        {
            DraftRpcs.RpcSlotNotify(PlayerControl.LocalPlayer, playerIds[i], slotNumbers[i]);
        }
    }

    public static void BroadcastSlotNotifications(int totalSlots, Dictionary<byte, int> pidToSlot)
    {
        if (pidToSlot == null) return;
        
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] Broadcasting {pidToSlot.Count} slot assignments");
        
        DraftRpcs.RpcStartDraft(PlayerControl.LocalPlayer, totalSlots);
        
        foreach (var kvp in pidToSlot)
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] Sending slot: Player {kvp.Key} -> Slot {kvp.Value}");
            DraftRpcs.RpcSlotNotify(PlayerControl.LocalPlayer, kvp.Key, kvp.Value);
        }
    }

    public static void SendTurnAnnouncement(int slot, byte playerId, List<ushort> roleIds, int turnNumber)
    {
        if (roleIds == null) return;
        
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] SendTurnAnnouncement: turn {turnNumber}, slot {slot}, picker {playerId}");
        
        DraftManager.SetClientTurn(turnNumber, slot);
        
        const int maxOffered = 9;
        if (roleIds.Count > maxOffered)
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftNetworkHelper] {roleIds.Count} roles offered but the RPC only carries {maxOffered}, truncating");

        var padded = new ushort[maxOffered];
        var count = Math.Min(maxOffered, roleIds.Count);
        for (int i = 0; i < count; i++)
            padded[i] = roleIds[i];
        
        DraftRpcs.RpcAnnounceTurn(PlayerControl.LocalPlayer, turnNumber, slot, playerId, (byte)count,
            padded[0], padded[1], padded[2], padded[3], padded[4], padded[5], padded[6], padded[7], padded[8]);
    }

    public static void RequestReroll()
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftNetworkHelper] RequestReroll");
        if (AmongUsClient.Instance.AmHost)
        {
            DraftEngineBehaviour.Instance?.RequestReroll(PlayerControl.LocalPlayer.PlayerId);
        }
        else
        {
            DraftRpcs.RpcRequestReroll(PlayerControl.LocalPlayer);
        }
    }

    public static void BroadcastPickConfirmed(int slot, ushort roleId)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] BroadcastPickConfirmed: slot {slot}, roleId {roleId}");
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
        var recapData = "";
        if (showRecap && entries != null)
        {
            var lines = new List<string>();
            foreach (var e in entries)
            {
                lines.Add($"{e.SlotNumber}:{e.RoleName ?? ""}");
            }
            recapData = string.Join("|", lines);
        }

        DraftRpcs.RpcBroadcastRecap(PlayerControl.LocalPlayer, recapData);
        DraftManager.Reset(cancelledBeforeCompletion: false);
    }

    public static void BroadcastDraftEnd()
    {
        DraftRpcs.RpcEndDraft(PlayerControl.LocalPlayer);
        DraftManager.Reset(cancelledBeforeCompletion: true);
    }
}