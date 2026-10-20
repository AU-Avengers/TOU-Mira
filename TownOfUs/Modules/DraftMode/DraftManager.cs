using System.Reflection;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Modules.DraftMode;

public static class DraftManager
{
    private const float DisconnectConfirmSeconds = 1.5f;
    private static readonly List<DraftSlotState> SlotStates = [];
    private static readonly Dictionary<byte, float> DisconnectSuspectSince = [];
    private static int _currentTurn;

    public static bool IsDraftActive;
    public static int CurrentTurnNumber => _currentTurn;
    public static int CurrentTurnSlot { get; private set; } = -1;
    public static int TotalSlots { get; private set; }
    public static float TurnDuration { get; private set; } = 10f;
    public static float TurnTimeLeft { get; set; }
    public static IEnumerable<int> TurnOrder => SlotStates.Select(s => s.SlotNumber).OrderBy(x => x);
    public static IReadOnlyList<DraftSlotState> States => SlotStates;

    public static void Begin(Dictionary<byte, int> playerSlots)
    {
        SlotStates.Clear();
        DisconnectSuspectSince.Clear();
        foreach (var (playerId, slot) in playerSlots)
        {
            SlotStates.Add(new DraftSlotState { PlayerId = playerId, SlotNumber = slot });
        }

        IsDraftActive = true;
        TotalSlots = playerSlots.Count;
        _currentTurn = 0;
        CurrentTurnSlot = -1;
        TurnDuration = OptionGroupSingleton<RoleOptions>.Instance.TurnDurationSeconds.Value;
        DraftTicker.EnsureExists();
        DraftSidebarManager.InvalidateCache();
    }

    public static void Remove(int slot)
    {
        SlotStates.RemoveAll(s => s.SlotNumber == slot);
        DraftSidebarManager.InvalidateCache();
        DraftStatusOverlay.Refresh();
    }

    public static void SetClientTurn(int turnNumber, int slot)
    {
        if (turnNumber != _currentTurn)
        {
            _currentTurn = turnNumber;
            TurnDuration = OptionGroupSingleton<RoleOptions>.Instance.TurnDurationSeconds.Value;
            TurnTimeLeft = TurnDuration;
            foreach (var s in SlotStates)
            {
                s.IsPickingNow = false;
                s.PendingPickIndex = 255;
                s.PendingPickRoleId = 0;
                s.PendingPickTurnNumber = -1;
            }
        }

        CurrentTurnSlot = slot;
        var target = GetStateForSlot(slot);
        if (target != null)
        {
            target.IsPickingNow = true;
            target.PendingPickIndex = 255;
            target.PendingPickRoleId = 0;
            target.PendingPickTurnNumber = turnNumber;
        }

        DraftSidebarManager.InvalidateCache();
        DraftStatusOverlay.Refresh();
    }

    public static void ConfirmPick(int slot, ushort roleId)
    {
        var state = GetStateForSlot(slot);
        if (state == null) return;

        if (!AmongUsClient.Instance.AmHost && (roleId != 0 || !state.HasPicked))
        {
            state.ChosenRoleId = roleId;
        }

        state.HasPicked = true;
        state.IsPickingNow = false;
        state.PendingPickIndex = 255;
        state.PendingPickRoleId = 0;
        state.PendingPickTurnNumber = -1;

        if (PlayerControl.LocalPlayer != null && state.PlayerId == PlayerControl.LocalPlayer.PlayerId && roleId != 0)
            DraftStatusOverlay.NotifyLocalPlayerPicked(roleId);

        DraftSidebarManager.InvalidateCache();
        DraftStatusOverlay.Refresh();
    }

    public static void NotifyPickerReady(byte playerId)
    {
        var state = GetStateForPlayer(playerId);
        if (state != null) state.IsPickerReady = true;
    }

    public static int GetSlotForPlayer(byte playerId) => GetStateForPlayer(playerId)?.SlotNumber ?? -1;

    public static DraftSlotState GetStateForSlot(int slot) => SlotStates.FirstOrDefault(s => s.SlotNumber == slot)!;

    public static DraftSlotState GetStateForPlayer(byte playerId) => SlotStates.FirstOrDefault(s => s.PlayerId == playerId)!;

    public static bool IsBot(byte playerId)
    {
        var data = MiscUtils.PlayerById(playerId)?.Data;
        if (data == null) return false;

        var type = data.GetType();
        var flag = type.GetProperty("IsBot", BindingFlags.Public | BindingFlags.Instance)
                   ?? type.GetProperty("IsDummy", BindingFlags.Public | BindingFlags.Instance);
        return flag?.GetValue(data) is true;
    }

    public static bool IsPlayerGone(byte playerId)
    {
        var player = MiscUtils.PlayerById(playerId);
        if (player == null || player.Data == null || player.Data.Disconnected) return true;

        var offline = AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame or NetworkModes.FreePlay;
        if (offline || AmongUsClient.Instance.GetClient(player.OwnerId) != null)
        {
            DisconnectSuspectSince.Remove(playerId);
            return false;
        }

        if (!DisconnectSuspectSince.TryGetValue(playerId, out var since))
        {
            DisconnectSuspectSince[playerId] = Time.time;
            return false;
        }

        return Time.time - since >= DisconnectConfirmSeconds;
    }

    public static bool IsPlayerDisconnected(byte playerId) => IsPlayerGone(playerId) || IsBot(playerId);

    public static void Reset(bool cancelledBeforeCompletion)
    {
        IsDraftActive = false;
        if (cancelledBeforeCompletion)
        {
            DraftApplier.PendingDraftStates.Clear();
        }

        SlotStates.Clear();
        DisconnectSuspectSince.Clear();
        _currentTurn = 0;
        CurrentTurnSlot = -1;
        TotalSlots = 0;
        TurnTimeLeft = 0f;

        DraftStatusOverlay.SetState(OverlayState.Hidden);
        DraftSidebarManager.InvalidateCache();

        if (GameStartManager.Instance != null)
            GameStartManager.Instance.ResetStartState();
    }
}

public class RecapEntry(int slotNumber, string roleName, string teamLabel = null!, string colorHex = null!)
{
    public int SlotNumber { get; } = slotNumber;
    public string RoleName { get; } = roleName;
    public string TeamLabel { get; } = teamLabel ?? "Unknown";
    public string ColorHex { get; } = colorHex ?? "FFFFFF";
}

public class DraftSlotState
{
    public byte PlayerId;
    public int SlotNumber;
    public ushort ChosenRoleId;
    public bool HasPicked;
    public bool IsPickingNow;
    public bool IsPickerReady;
    public byte PendingPickIndex = 255;
    public ushort PendingPickRoleId;
    public int PendingPickTurnNumber = -1;
}
