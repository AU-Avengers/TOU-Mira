namespace TownOfUs.Modules.DraftMode;

public static class DraftManager
{
    public static bool IsDraftActive;
    public static int TotalSlots => _totalSlots;
    public static int CurrentTurn => _currentTurn;

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
            var state = new DraftSlotState
            {
                PlayerId = playerIds[i],
                SlotNumber = slotNumbers[i]
            };
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
    }

    public static void SetForcedDraftRole(string roleName, byte targetId)
    {
        if (string.IsNullOrEmpty(roleName)) return;
        var state = GetStateForPlayer(targetId);
        if (state == null) return;
        state.ForcedRoleName = roleName;
    }

    public static DraftSlotState? GetStateForSlot(int slot) =>
        SlotStates.FirstOrDefault(s => s.SlotNumber == slot);

    public static DraftSlotState? GetStateForPlayer(byte playerId) =>
        SlotStates.FirstOrDefault(s => s.PlayerId == playerId);

    public static IReadOnlyList<DraftSlotState> GetAllStates() => SlotStates.AsReadOnly();

    public static void Reset(bool cancelledBeforeCompletion)
    {
        IsDraftActive = false;
        SlotStates.Clear();
        PlayerToSlot.Clear();
        _totalSlots = 0;
        _currentTurn = 0;
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
    public byte PendingPickIndex;
    public string? ForcedRoleName;
}