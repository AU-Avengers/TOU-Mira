using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;
using MiraAPI.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Options;
using TownOfUs.Patches;
using System.Globalization;


namespace TownOfUs.Modules.DraftMode;

public static class DraftRpcs
{
    private static bool FromHost(PlayerControl sender) =>
        sender != null && sender.OwnerId == AmongUsClient.Instance.HostId;

    [MethodRpc((uint)TownOfUsRpc.DraftSubmitPick)]
    public static void RpcSubmitPick(PlayerControl sender, int index, int roleId)
    {
        if (!AmongUsClient.Instance.AmHost || sender == null) return;
        if (index is < byte.MinValue or > byte.MaxValue || roleId is < ushort.MinValue or > ushort.MaxValue) return;

        DraftEngine.OnPick(sender.PlayerId, index, (ushort)roleId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftStart)]
    public static void RpcStartDraft(PlayerControl sender, string slots)
    {
        if (!FromHost(sender)) return;

        if (!AmongUsClient.Instance.AmHost)
        {
            var playerSlots = slots.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(':'))
                .ToDictionary(x => byte.Parse(x[0], NumberFormatInfo.InvariantInfo), x => int.Parse(x[1], NumberFormatInfo.InvariantInfo));
            DraftManager.Begin(playerSlots);
        }

        HudManagerPatches.ResetZoom();
        DraftAudio.PlayDraftStart();
        DraftSidebarManager.Activate();
        DraftCancelButton.Show();
        CustomButtonSingleton<DraftShuffleButton>.Instance.SetUses(OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftPickerReady)]
    public static void RpcPickerReady(PlayerControl sender)
    {
        if (!AmongUsClient.Instance.AmHost || sender == null) return;
        DraftManager.NotifyPickerReady(sender.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftRequestShuffle)]
    public static void RpcRequestShuffle(PlayerControl sender)
    {
        if (!AmongUsClient.Instance.AmHost || sender == null) return;
        DraftEngine.RequestShuffle(sender.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.DraftCancel)]
    public static void RpcCancelDraft(PlayerControl sender)
    {
        if (!FromHost(sender)) return;

        DraftManager.Reset(cancelledBeforeCompletion: true);
        DraftScreenController.Hide();
        DraftCancelButton.Hide();
        DraftSidebarManager.Deactivate();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftCreateNotif)]
    public static void RpcCreateNotif(PlayerControl sender, string message)
    {
        if (!FromHost(sender) || string.IsNullOrEmpty(message)) return;

        var notif = Helpers.CreateAndShowNotification(message, Color.white,
            new Vector3(0f, 1f, -20f), spr: TouAssets.IconDraftMode);
        notif?.AdjustNotification();
    }

    [MethodRpc((uint)TownOfUsRpc.DraftBroadcastRecap)]
    public static void RpcBroadcastRecap(PlayerControl sender, string recapData)
    {
        if (!FromHost(sender)) return;

        var mode = DraftRecapMode.Nothing;
        var entries = new List<(int slot, string label, string colorHex)>();
        var tokens = (recapData ?? string.Empty).Split('|');
        if (int.TryParse(tokens[0], out var modeInt) && Enum.IsDefined(typeof(DraftRecapMode), modeInt))
        {
            mode = (DraftRecapMode)modeInt;
            foreach (var token in tokens.Skip(1))
            {
                var parts = token.Split(':');
                if (parts.Length >= 3 && int.TryParse(parts[0], out var slot) && slot > 0)
                {
                    entries.Add((slot, parts[1], parts[2]));
                }
            }
        }

        DraftScreenController.Hide();
        DraftSidebarManager.Deactivate();
        DraftStatusOverlay.DestroyRoleCard();

        if (mode == DraftRecapMode.Nothing || entries.Count == 0)
        {
            DraftManager.Reset(cancelledBeforeCompletion: false);
            return;
        }

        DraftStatusOverlay.SetState(OverlayState.BackgroundOnly);
        DraftRecapScreen.Show(entries, mode, onComplete: () => DraftManager.Reset(cancelledBeforeCompletion: false));
    }
}

public sealed class DraftTurnAnnouncement
{
    public int TurnNumber;
    public int Slot;
    public byte PickerId;
    public List<ushort> RoleIds { get; } = [];
}

[RegisterCustomRpc((uint)TownOfUsRpc.DraftAnnounceTurn)]
public sealed class DraftAnnounceTurnRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, DraftTurnAnnouncement>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    public override void Write(MessageWriter writer, DraftTurnAnnouncement? data)
    {
        data ??= new DraftTurnAnnouncement();
        writer.Write(data.TurnNumber);
        writer.Write(data.Slot);
        writer.Write(data.PickerId);
        writer.Write((byte)data.RoleIds.Count);
        foreach (var roleId in data.RoleIds)
        {
            writer.Write((int)roleId);
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
        }

        return data;
    }

    public override void Handle(PlayerControl innerNetObject, DraftTurnAnnouncement? data)
    {
        if (data == null || innerNetObject == null || innerNetObject.OwnerId != AmongUsClient.Instance.HostId) return;

        DraftManager.SetClientTurn(data.TurnNumber, data.Slot);

        var offeredList = data.RoleIds;

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
            draftScreenController?.CacheOfferedRoles(offeredList.ToArray());

            DraftAudio.PlayYourTurn();
            DraftScreenController.TargetPickerId = data.PickerId;
            DraftScreenController.Show(offeredList.ToArray());
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
        if (data == null || innerNetObject == null || innerNetObject.OwnerId != AmongUsClient.Instance.HostId) return;

        DraftManager.ConfirmPick(data.Slot, data.RoleId);

        if (data.Slot == DraftManager.GetSlotForPlayer(PlayerControl.LocalPlayer.PlayerId) && data.RoleId != 0)
        {
            DraftScreenController.Hide();
            DraftScreenController.ShowFinalPickNotification(data.RoleId);
        }
    }
}

public static class DraftNetworkHelper
{
    public static void BroadcastStart(Dictionary<byte, int> playerSlots)
    {
        var slots = string.Join(",", playerSlots.Select(x => $"{x.Key}:{x.Value}"));
        DraftRpcs.RpcStartDraft(PlayerControl.LocalPlayer, slots);
    }

    public static void SendPickToHost(int index, ushort roleId, byte pickerId = 255)
    {
        var id = pickerId == 255 ? PlayerControl.LocalPlayer.PlayerId : pickerId;
        if (AmongUsClient.Instance.AmHost)
            DraftEngine.OnPick(id, index, roleId);
        else
            DraftRpcs.RpcSubmitPick(PlayerControl.LocalPlayer, index, roleId);
    }

    public static void SendTurnAnnouncement(int slot, byte playerId, List<ushort> roleIds, int turnNumber)
    {
        var announcement = new DraftTurnAnnouncement { TurnNumber = turnNumber, Slot = slot, PickerId = playerId };
        announcement.RoleIds.AddRange(roleIds);
        Rpc<DraftAnnounceTurnRpc>.Instance.Send(PlayerControl.LocalPlayer, announcement);
    }

    public static void BroadcastPickConfirmed(int slot, ushort roleId, bool timedOut = false)
    {
        var hideFromOthers = OptionGroupSingleton<RoleOptions>.Instance.DraftSidebarDisplay == DraftRecapMode.Nothing;
        var picker = DraftManager.GetStateForSlot(slot);
        var pickerClient = picker != null ? AmongUsClient.Instance.GetClient(MiscUtils.PlayerById(picker.PlayerId)?.OwnerId ?? -1) : null;

        var publicRoleId = roleId;
        if (hideFromOthers && pickerClient != null)
        {
            Rpc<DraftPickConfirmedRpc>.Instance.SendTo(PlayerControl.LocalPlayer, pickerClient.Id,
                new DraftPickConfirmedData { Slot = slot, RoleId = roleId, TimedOut = timedOut });
            publicRoleId = 0;
        }

        Rpc<DraftPickConfirmedRpc>.Instance.Send(PlayerControl.LocalPlayer,
            new DraftPickConfirmedData { Slot = slot, RoleId = publicRoleId, TimedOut = timedOut });
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
            DraftEngine.RequestShuffle(PlayerControl.LocalPlayer.PlayerId);
        else
            DraftRpcs.RpcRequestShuffle(PlayerControl.LocalPlayer);
    }

    public static void BroadcastCancelDraft()
    {
        DraftRpcs.RpcCancelDraft(PlayerControl.LocalPlayer);
    }

    public static void BroadcastRecap(List<RecapEntry> entries, DraftRecapMode mode)
    {
        var recapData = ((int)mode).ToString(CultureInfo.InvariantCulture);
        if (mode != DraftRecapMode.Nothing)
        {
            var lines = entries.Select(e =>
            {
                var label = (mode == DraftRecapMode.Role ? e.RoleName : e.TeamLabel).Replace(":", "").Replace("|", "");
                return $"{e.SlotNumber}:{label}:{e.ColorHex}";
            });
            recapData += "|" + string.Join("|", lines);
        }

        DraftRpcs.RpcBroadcastRecap(PlayerControl.LocalPlayer, recapData);
    }
}
