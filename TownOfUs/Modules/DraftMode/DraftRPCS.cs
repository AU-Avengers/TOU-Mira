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
        DraftManager.SubmitPick(sender.PlayerId, (byte)index);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftStart)]
    public static void RpcStartDraft(PlayerControl sender, int totalSlots)
    {
        DraftManager.IsDraftActive = true;
        DraftAudio.PlayDraftStart();
        DraftSidebarManager.Activate();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftSlotNotify)]
    public static void RpcSlotNotify(PlayerControl sender, byte playerId, int slotNumber)
    {
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
        DraftEngineBehaviour.Instance?.RequestShuffle(sender.PlayerId);
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
        DraftManager.Reset(cancelledBeforeCompletion: true);
        DraftScreenController.Hide();
        DraftCancelButton.Hide();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftEnd)]
    public static void RpcEndDraft(PlayerControl sender)
    {
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


        DraftManager.SetClientTurn(data.TurnNumber, data.Slot);

        var offeredList = data.RoleIds;
        var offeredNames = data.RoleNames;

        var localPlayerId = PlayerControl.LocalPlayer?.PlayerId ?? 255;

        bool isMyTurn = localPlayerId == data.PickerId;
        bool isLocalGame = AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;

        if (!isMyTurn && isLocalGame)
        {
            var p = MiscUtils.PlayerById(data.PickerId);
            if (p != null)
            {
                var client = AmongUsClient.Instance?.GetClient(p.OwnerId);
                if (client == null)
                {
                    isMyTurn = true;
                }
            }
        }

        if (isMyTurn && offeredList.Count > 0)
        {
            var draftScreenController = Object.FindObjectOfType<DraftScreenController>();
            draftScreenController?.CacheOfferedRoles(offeredList.ToArray(), offeredNames.ToArray());

            DraftAudio.PlayYourTurn();
            try
            {
                DraftScreenController.TargetPickerId = data.PickerId;
                DraftScreenController.Show(offeredList.ToArray(), offeredNames.ToArray());
            }
            catch (Exception e)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftRpc] Exception showing picker screen: {e}");
            }
        }
        else
        {

            DraftStatusOverlay.SetState(OverlayState.Waiting);
        }
    }
}

public sealed class DraftPickConfirmedData
{
    public int Slot;
    public ushort RoleId;
    public bool TimedOut;
}

[RegisterCustomRpc((uint)TownOfUsRpc.DraftPickConfirmed)]
public sealed class DraftPickConfirmedRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, DraftPickConfirmedData>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    public override void Write(MessageWriter writer, DraftPickConfirmedData? data)
    {
        if (data == null)
        {
            writer.Write(0);
            writer.Write(0);
            writer.Write(false);
            return;
        }

        writer.Write(data.Slot);
        writer.Write((int)data.RoleId);
        writer.Write(data.TimedOut);
    }

    public override DraftPickConfirmedData Read(MessageReader reader)
    {
        return new DraftPickConfirmedData
        {
            Slot = reader.ReadInt32(),
            RoleId = (ushort)reader.ReadInt32(),
            TimedOut = reader.ReadBoolean()
        };
    }

    public override void Handle(PlayerControl innerNetObject, DraftPickConfirmedData? data)
    {
        if (data == null) return;

        DraftManager.ConfirmPick(data.Slot, data.RoleId);

        var localSlot = DraftManager.GetSlotForPlayer(PlayerControl.LocalPlayer.PlayerId);
        if (data.Slot == localSlot && data.RoleId != 0)
        {
            DraftScreenController.Hide();
            DraftScreenController.ShowFinalPickNotification(data.RoleId);
        }
    }
}

public static class DraftNetworkHelper
{
    private static bool TryGetClientId(byte playerId, out int clientId)
    {
        var player = MiscUtils.PlayerById(playerId);
        var client = player != null ? AmongUsClient.Instance.GetClient(player.OwnerId) : null;
        clientId = client?.Id ?? -1;
        return client != null;
    }

    public static void SendPickToHost(int index, byte pickerId = 255)
    {
        byte idToSend = pickerId == 255 ? PlayerControl.LocalPlayer.PlayerId : pickerId;
        if (AmongUsClient.Instance.AmHost)
            DraftManager.SubmitPick(idToSend, (byte)index);
        else
            DraftRpcs.RpcSubmitPick(PlayerControl.LocalPlayer, index);
    }

    public static void BroadcastSlotNotifications(int totalSlots, Dictionary<byte, int> pidToSlot)
    {
        if (pidToSlot == null) return;

        DraftManager.IsDraftActive = true;
        DraftAudio.PlayDraftStart();
        DraftSidebarManager.Activate();

        DraftRpcs.RpcStartDraft(PlayerControl.LocalPlayer, totalSlots);

        foreach (var kvp in pidToSlot)
        {
            DraftRpcs.RpcSlotNotify(PlayerControl.LocalPlayer, kvp.Key, kvp.Value);
        }
    }

    public static void SendTurnAnnouncement(int slot, byte playerId, List<ushort> roleIds, List<string> roleNames, int turnNumber)
    {
        if (roleIds == null) return;


        DraftManager.SetClientTurn(turnNumber, slot);

        var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
        int allowed = Math.Max(1, (int)(roleOpts?.OfferedRolesCount.Value ?? 3));
        if (roleIds.Count > allowed)
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftNetworkHelper] {roleIds.Count} roles offered but only {allowed} are configured, truncating");

        var count = Math.Min(allowed, roleIds.Count);

        var publicAnnouncement = new DraftTurnAnnouncement
        {
            TurnNumber = turnNumber,
            Slot = slot,
            PickerId = playerId
        };
        Rpc<DraftAnnounceTurnRpc>.Instance.Send(PlayerControl.LocalPlayer, publicAnnouncement);

        var privateAnnouncement = new DraftTurnAnnouncement
        {
            TurnNumber = turnNumber,
            Slot = slot,
            PickerId = playerId
        };
        for (int i = 0; i < count; i++)
        {
            privateAnnouncement.RoleIds.Add(roleIds[i]);
            privateAnnouncement.RoleNames.Add(roleNames != null && i < roleNames.Count ? (roleNames[i] ?? string.Empty) : string.Empty);
        }

        if (TryGetClientId(playerId, out var pickerClientId))
        {
            Rpc<DraftAnnounceTurnRpc>.Instance.SendTo(PlayerControl.LocalPlayer, pickerClientId, privateAnnouncement);
        }
        else
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                $"[DraftNetworkHelper] Could not resolve client id for picker {playerId}, falling back to broadcasting offered roles");
            Rpc<DraftAnnounceTurnRpc>.Instance.Send(PlayerControl.LocalPlayer, privateAnnouncement);
        }
    }

    public static void BroadcastPickConfirmed(int slot, ushort roleId, bool timedOut = false)
    {

        var hideFromOthers = OptionGroupSingleton<RoleOptions>.Instance?.DraftSidebarDisplay.Value == DraftRecapMode.Nothing;
        var publicRoleId = hideFromOthers ? (ushort)0 : roleId;

        if (hideFromOthers)
        {
            var pickerId = DraftManager.GetStateForSlot(slot)?.PlayerId;
            if (pickerId.HasValue && TryGetClientId(pickerId.Value, out var pickerClientId))
            {
                Rpc<DraftPickConfirmedRpc>.Instance.SendTo(PlayerControl.LocalPlayer, pickerClientId,
                    new DraftPickConfirmedData { Slot = slot, RoleId = roleId, TimedOut = timedOut });
            }
            else
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftNetworkHelper] Could not resolve client id for slot {slot} picker, falling back to broadcasting picked role");
                publicRoleId = roleId;
            }
        }

        Rpc<DraftPickConfirmedRpc>.Instance.Send(PlayerControl.LocalPlayer,
            new DraftPickConfirmedData { Slot = slot, RoleId = publicRoleId, TimedOut = timedOut });

        foreach (Action cleanup in new Action[] { DraftScreenController.Hide, DraftSidebarManager.InvalidateCache, DraftStatusOverlay.Refresh })
        {
            try
            {
                cleanup();
            }
            catch
            {
                // ignored
            }
        }
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
        DraftSidebarManager.Deactivate();
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
        DraftSidebarManager.Deactivate();
    }

    public static void BroadcastDraftEnd()
    {
        DraftRpcs.RpcEndDraft(PlayerControl.LocalPlayer);
        DraftManager.Reset(cancelledBeforeCompletion: true);
        DraftCancelButton.Hide();
    }
}