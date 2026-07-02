using System;
using System.Collections.Generic;
using System.Linq;

namespace TownOfUs.Modules.DraftMode;

public static class DraftManager
{
    public static bool IsDraftActive;
    public static int TotalSlots => _totalSlots;
    public static int CurrentTurn => _currentTurn;

    public static float TurnDuration { get; set; } = 10f;
    public static float TurnTimeLeft { get; set; }
    public static bool ShowRandomOption { get; set; } = true;
    public static IEnumerable<int> TurnOrder { get; internal set; }

    private static readonly List<DraftSlotState> SlotStates = [];
    private static readonly Dictionary<byte, int> PlayerToSlot = [];
    private static int _totalSlots;
    private static int _currentTurn;

    public static void SetDraftStateFromHost(int totalSlots, List<byte> playerIds, List<int> slotNumbers)
    {
        if (playerIds == null || slotNumbers == null) return;
        if (playerIds.Count != slotNumbers.Count) return;

        _totalSlots = totalSlots;
        SlotStates.Clear();
        PlayerToSlot.Clear();

        for (var i = 0; i < playerIds.Count; i++)
        {
            var state = new DraftSlotState { PlayerId = playerIds[i], SlotNumber = slotNumbers[i] };
            SlotStates.Add(state);
            PlayerToSlot[playerIds[i]] = slotNumbers[i];
        }

        IsDraftActive = true;
    }

    public static void UpdateSlotAssignments(int totalSlots, byte[] playerIds, int[] slotNumbers)
    {
        if (playerIds == null || slotNumbers == null) return;
        if (playerIds.Length != slotNumbers.Length) return;

        _totalSlots = totalSlots;

        for (var i = 0; i < playerIds.Length; i++)
        {
            var existing = GetStateForPlayer(playerIds[i]);
            if (existing != null)
                existing.SlotNumber = slotNumbers[i];
        }
    }

    /// <summary>
    /// Add a slot state for a player (used when receiving individual slot assignments)
    /// </summary>
    public static void AddSlotState(DraftSlotState state)
    {
        if (state == null) return;
        
        // Remove existing state for this player if it exists
        var existing = SlotStates.FirstOrDefault(s => s.PlayerId == state.PlayerId);
        if (existing != null)
            SlotStates.Remove(existing);
        
        SlotStates.Add(state);
        PlayerToSlot[state.PlayerId] = state.SlotNumber;
    }

    public static void SubmitPick(byte playerId, byte index)
    {
        var state = GetStateForPlayer(playerId);
        if (state == null) return;
        state.PendingPickIndex = index;
    }

    public static void ConfirmPick(int slot, ushort roleId)
    {
        var state = GetStateForSlot(slot);
        if (state == null) return;
        state.ChosenRoleId = roleId;
        state.HasPicked = true;
        state.IsPickingNow = false;

        if (PlayerControl.LocalPlayer != null && state.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            DraftStatusOverlay.NotifyLocalPlayerPicked(roleId);

        DraftSidebarManager.InvalidateCache();
        DraftStatusOverlay.Refresh();
    }

    public static void NotifyPickerReady(byte playerId)
    {
        var state = GetStateForPlayer(playerId);
        if (state == null) return;
        state.IsPickerReady = true;
    }

    public static void SetClientTurn(int turnNumber, int slot)
    {
        _currentTurn = turnNumber;
        foreach (var s in SlotStates)
            s.IsPickingNow = s.SlotNumber == slot;

        DraftSidebarManager.InvalidateCache();
        DraftStatusOverlay.Refresh();
    }

    public static void SetForcedDraftRole(string roleName, byte targetId)
    {
        if (string.IsNullOrEmpty(roleName)) return;
        var state = GetStateForPlayer(targetId);
        if (state == null) return;
        state.ForcedRoleName = roleName;
    }

    public static int GetSlotForPlayer(byte playerId) =>
        PlayerToSlot.TryGetValue(playerId, out var slot) ? slot : -1;

    public static DraftSlotState GetStateForSlot(int slot) =>
        SlotStates.FirstOrDefault(s => s.SlotNumber == slot);

    public static DraftSlotState GetStateForPlayer(byte playerId) =>
        SlotStates.FirstOrDefault(s => s.PlayerId == playerId);

    public static IReadOnlyList<DraftSlotState> GetAllStates() => SlotStates.AsReadOnly();

    public static List<DraftSlotState> GetActivePickerStatesNonAlloc()
    {
        return SlotStates.Where(s => s != null && s.IsPickingNow).ToList();
    }

    public static void Reset(bool cancelledBeforeCompletion)
    {
        IsDraftActive = false;
        SlotStates.Clear();
        PlayerToSlot.Clear();
        _totalSlots = 0;
        _currentTurn = 0;
        TurnTimeLeft = 0f;

        // Reset never told the overlay the draft was over, so it stayed stuck
        // on "Waiting" (and kept GameStartManager/LobbyInfoPane hidden) forever.
        DraftStatusOverlay.SetState(OverlayState.Hidden);
        DraftSidebarManager.InvalidateCache();
    }
}

public class RecapEntry(int slotNumber, string roleName)
{
    public int SlotNumber { get; } = slotNumber;
    public string RoleName { get; } = roleName;
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
    public string ForcedRoleName;
}