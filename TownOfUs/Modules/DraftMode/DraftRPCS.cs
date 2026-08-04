using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;
using MiraAPI.Utilities;
using MiraAPI.GameOptions;
using TownOfUs.Options;


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

    [MethodRpc((uint)TownOfUsRpc.DraftStart)]
    public static void RpcStartDraft(PlayerControl sender, int totalSlots)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcStartDraft received (isHost={AmongUsClient.Instance.AmHost})");
        DraftManager.IsDraftActive = true;
        DraftAudio.PlayDraftStart();
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

            DraftManager.AddSlotState(state);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.DraftPickerReady)]
    public static void RpcPickerReady(PlayerControl sender)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sender == null) return;
        DraftManager.NotifyPickerReady(sender.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftRequestShuffle)]
    public static void RpcRequestShuffle(PlayerControl sender)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sender == null) return;
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcRequestShuffle: player {sender.PlayerId}");
        DraftEngineBehaviour.Instance?.RequestShuffle(sender.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftPickConfirmed)]
    public static void RpcPickConfirmed(PlayerControl sender, int slot, ushort roleId, bool timedOut)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcPickConfirmed: slot {slot}, roleId {roleId}, timedOut {timedOut}");
        DraftManager.ConfirmPick(slot, roleId);

        var localSlot = DraftManager.GetSlotForPlayer(PlayerControl.LocalPlayer.PlayerId);
        if (slot == localSlot)
        {
            DraftScreenController.Hide();
            DraftScreenController.ShowFinalPickNotification((ushort)roleId);
        }
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
        DraftCancelButton.Hide();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftEnd)]
    public static void RpcEndDraft(PlayerControl sender)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftRpc] RpcEndDraft");
        DraftManager.Reset(cancelledBeforeCompletion: true);
        DraftScreenController.Hide();
        DraftCancelButton.Hide();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftCreateNotif)]
    public static void RpcCreateNotif(PlayerControl sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        var notif = Helpers.CreateAndShowNotification(message, Color.white,
            new Vector3(0f, 1f, -20f), spr: TouAssets.IconDraftMode.LoadAsset());
        notif?.AdjustNotification();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftBroadcastRecap)]
    public static void RpcBroadcastRecap(PlayerControl sender, string recapData)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftRpc] RpcBroadcastRecap");

        var mode = DraftRecapMode.Nothing;
        var entries = new List<(int slot, string label, string colorHex)>();
        if (!string.IsNullOrEmpty(recapData))
        {

            var tokens = recapData.Split('|');
            if (tokens.Length >= 1 && int.TryParse(tokens[0], out var modeInt))
            {
                mode = (DraftRecapMode)modeInt;
                for (int i = 1; i < tokens.Length; i++)
                {
                    var parts = tokens[i].Split(':');
                    if (parts.Length < 3) continue;
                    if (!int.TryParse(parts[0], out var slot)) continue;
                    entries.Add((slot, parts[1], parts[2]));
                }
            }
        }

        DraftScreenController.Hide();
        DraftSidebarManager.Deactivate();
        DraftStatusOverlay.DestroyRoleCard();
        bool willShowRecap = mode != DraftRecapMode.Nothing && entries.Count > 0;

        if (!willShowRecap)
        {

            DraftManager.Reset(cancelledBeforeCompletion: false);
            return;
        }

        DraftStatusOverlay.SetState(OverlayState.BackgroundOnly);

        try
        {
            DraftRecapScreen.Show(entries, mode, onComplete: () =>
            {

                DraftManager.Reset(cancelledBeforeCompletion: false);
            });
        }
        catch (Exception e)
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftRpc] Failed to show recap screen: {e}");
            DraftManager.Reset(cancelledBeforeCompletion: false);
        }
    }
}

public sealed class DraftTurnAnnouncement
{
    public int TurnNumber;
    public int Slot;
    public byte PickerId;
    public List<ushort> RoleIds { get; } = [];
    public List<string> RoleNames { get; } = [];
}

[RegisterCustomRpc((uint)TownOfUsRpc.DraftAnnounceTurn)]
public sealed class DraftAnnounceTurnRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, DraftTurnAnnouncement>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    public override void Write(MessageWriter writer, DraftTurnAnnouncement? data)
    {
        if (data == null)
        {
            writer.Write(0);
            writer.Write(0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            return;
        }

        writer.Write(data.TurnNumber);
        writer.Write(data.Slot);
        writer.Write(data.PickerId);

        var count = (byte)Math.Min(data.RoleIds.Count, data.RoleNames.Count);
        writer.Write(count);
        for (var i = 0; i < count; i++)
        {
            writer.Write((int)data.RoleIds[i]);
            writer.Write(data.RoleNames[i] ?? string.Empty);
        }
    }

    public override DraftTurnAnnouncement Read(MessageReader reader)
    {
        var data = new DraftTurnAnnouncement
        {
            TurnNumber = reader.ReadInt32(),
            Slot = reader.ReadInt32(),
            PickerId = reader.ReadByte()
        };

        var count = reader.ReadByte();
        for (var i = 0; i < count; i++)
        {
            data.RoleIds.Add((ushort)reader.ReadInt32());
            data.RoleNames.Add(reader.ReadString());
        }

        return data;
    }

    public override void Handle(PlayerControl innerNetObject, DraftTurnAnnouncement? data)
    {
        if (data == null) return;

        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] RpcAnnounceTurn: Turn {data.TurnNumber}, Slot {data.Slot}, PickerId {data.PickerId} (isHost={AmongUsClient.Instance.AmHost})");

        DraftManager.SetClientTurn(data.TurnNumber, data.Slot);

        var offeredList = data.RoleIds;
        var offeredNames = data.RoleNames;

        var localPlayerId = PlayerControl.LocalPlayer?.PlayerId ?? 255;
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] Checking if it's my turn. Local: {localPlayerId}, Picker: {data.PickerId}");

        bool isMyTurn = localPlayerId == data.PickerId;
        bool isLocalGame = AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;

        if (!isMyTurn && isLocalGame)
        {
            var p = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == data.PickerId);
            if (p != null)
            {
                var client = AmongUsClient.Instance?.GetClient(p.OwnerId);
                if (client == null)
                {
                    isMyTurn = true;
                }
            }
        }

        if (isMyTurn)
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] Caching {offeredList.Count} offered roles for my turn");
            var draftScreenController = Object.FindObjectOfType<DraftScreenController>();
            draftScreenController?.CacheOfferedRoles(offeredList.ToArray(), offeredNames.ToArray());

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] IT'S MY TURN! Showing picker screen with {offeredList.Count} roles");
            DraftAudio.PlayYourTurn();
            try
            {
                DraftScreenController.TargetPickerId = data.PickerId;
                DraftScreenController.Show(offeredList.ToArray(), offeredNames.ToArray());
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftRpc] Picker screen shown successfully!");
            }
            catch (Exception e)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftRpc] Exception showing picker screen: {e}");
            }
        }
        else
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftRpc] Not my turn");

            DraftStatusOverlay.SetState(OverlayState.Waiting);
        }
    }
}

public static class DraftNetworkHelper
{
    public static void SendPickToHost(int index, byte pickerId = 255)
    {
        byte idToSend = pickerId == 255 ? PlayerControl.LocalPlayer.PlayerId : pickerId;
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] SendPickToHost: index {index} for player {idToSend}");
        if (AmongUsClient.Instance.AmHost)
            DraftManager.SubmitPick(idToSend, (byte)index);
        else
            DraftRpcs.RpcSubmitPick(PlayerControl.LocalPlayer, index);
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

    public static void SendTurnAnnouncement(int slot, byte playerId, List<ushort> roleIds, List<string> roleNames, int turnNumber)
    {
        if (roleIds == null) return;

        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] SendTurnAnnouncement: turn {turnNumber}, slot {slot}, picker {playerId}");

        DraftManager.SetClientTurn(turnNumber, slot);

        var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
        int allowed = Math.Max(1, (int)(roleOpts?.OfferedRolesCount.Value ?? 3));
        if (roleIds.Count > allowed)
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftNetworkHelper] {roleIds.Count} roles offered but only {allowed} are configured, truncating");

        var count = Math.Min(allowed, roleIds.Count);
        var announcement = new DraftTurnAnnouncement
        {
            TurnNumber = turnNumber,
            Slot = slot,
            PickerId = playerId
        };

        for (int i = 0; i < count; i++)
        {
            announcement.RoleIds.Add(roleIds[i]);
            announcement.RoleNames.Add(roleNames != null && i < roleNames.Count ? (roleNames[i] ?? string.Empty) : string.Empty);
        }

        Rpc<DraftAnnounceTurnRpc>.Instance.Send(PlayerControl.LocalPlayer, announcement);
    }

    public static void BroadcastPickConfirmed(int slot, ushort roleId, bool timedOut = false)
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftNetworkHelper] BroadcastPickConfirmed: slot {slot}, roleId {roleId}, timedOut {timedOut}");
        // Confirm locally first so host UI updates immediately
        DraftManager.ConfirmPick(slot, roleId);
        // Send RPC to clients
        DraftRpcs.RpcPickConfirmed(PlayerControl.LocalPlayer, slot, roleId, timedOut);

        // Ensure local UI and sidebar reflect the confirmed pick in local lobbies
        try
        {
            DraftScreenController.Hide();
        }
        catch { }

        try
        {
            DraftSidebarManager.InvalidateCache();
        }
        catch { }

        try
        {
            DraftStatusOverlay.Refresh();
        }
        catch { }
    }

    public static void NotifyPickerReady()
    {
        if (AmongUsClient.Instance.AmHost)
            DraftManager.NotifyPickerReady(PlayerControl.LocalPlayer.PlayerId);
        else
            DraftRpcs.RpcPickerReady(PlayerControl.LocalPlayer);
    }

    public static void RequestShuffle()
    {
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftNetworkHelper] RequestShuffle");
        if (AmongUsClient.Instance.AmHost)
            DraftEngineBehaviour.Instance?.RequestShuffle(PlayerControl.LocalPlayer.PlayerId);
        else
            DraftRpcs.RpcRequestShuffle(PlayerControl.LocalPlayer);
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
        DraftCancelButton.Hide();
    }

    public static void BroadcastRecap(List<RecapEntry> entries, DraftRecapMode mode)
    {
        var recapData = ((int)mode).ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (mode != DraftRecapMode.Nothing && entries != null)
        {
            var lines = new List<string>();
            foreach (var e in entries)
            {

                var label = mode == DraftRecapMode.Role ? (e.RoleName ?? "Unknown") : (e.TeamLabel ?? "Unknown");

                label = label.Replace(":", "").Replace("|", "");
                lines.Add($"{e.SlotNumber}:{label}:{e.ColorHex}");
            }
            recapData += "|" + string.Join("|", lines);
            DraftCancelButton.Hide();
        }

        DraftRpcs.RpcBroadcastRecap(PlayerControl.LocalPlayer, recapData);
    }

    public static void BroadcastDraftEnd()
    {
        DraftRpcs.RpcEndDraft(PlayerControl.LocalPlayer);
        DraftManager.Reset(cancelledBeforeCompletion: true);
        DraftCancelButton.Hide();
    }
}

