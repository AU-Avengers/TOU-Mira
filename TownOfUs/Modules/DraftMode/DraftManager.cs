namespace TownOfUs.Modules.DraftMode;

public static class DraftManager
{
    public static bool IsDraftActive;
    public static float TurnDuration { get; set; } = 10f;
    public static float TurnTimeLeft { get; set; }
    public static bool ShowRandomOption { get; set; } = true;
    public static IEnumerable<int> TurnOrder => SlotStates.Select(s => s.SlotNumber).OrderBy(x => x);

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
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
            $"[DraftManager] SetDraftStateFromHost: [{string.Join(", ", playerIds.Zip(slotNumbers, (p, s) => $"{p}->{s}"))}]");
    }

    public static void AddSlotState(DraftSlotState state)
    {
        if (state == null) return;

        var existing = SlotStates.FirstOrDefault(s => s.PlayerId == state.PlayerId);
        if (existing != null)
            SlotStates.Remove(existing);

        SlotStates.Add(state);
        PlayerToSlot[state.PlayerId] = state.SlotNumber;
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
            $"[DraftManager] AddSlotState: player {state.PlayerId} -> slot {state.SlotNumber}");
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
        if (turnNumber != _currentTurn)
        {
            _currentTurn = turnNumber;
            foreach (var s in SlotStates)
                s.IsPickingNow = false;
        }

        var target = SlotStates.FirstOrDefault(s => s.SlotNumber == slot);
        if (target != null)
            target.IsPickingNow = true;

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

        DraftStatusOverlay.SetState(OverlayState.Hidden);
        DraftSidebarManager.InvalidateCache();

        try
        {
            if (GameStartManager.Instance != null)
            {
                GameStartManager.Instance.ResetStartState();
            }
        }
        catch { }
    }
}

public class RecapEntry(int slotNumber, string roleName, string teamLabel = null, string colorHex = null)
{
    public int SlotNumber { get; } = slotNumber;
    public string RoleName  { get; } = roleName;

    public string TeamLabel { get; } = teamLabel ?? "Unknown";

    public string ColorHex  { get; } = colorHex  ?? "FFFFFF";
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