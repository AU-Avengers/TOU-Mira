using System.Reflection;

namespace TownOfUs.Modules.DraftMode;

public static class DraftManager
{
    public static bool IsDraftActive;
    public static int CurrentTurnNumber { get; private set; }
    public static int CurrentTurnSlot { get; private set; }
    public static int TotalSlots { get; private set; }
    public static float TurnDuration { get; set; } = 10f;
    public static float TurnTimeLeft { get; set; }
    public static bool ShowRandomOption { get; set; } = true;
    public static IEnumerable<int> TurnOrder => SlotStates.Select(s => s.SlotNumber).OrderBy(x => x);

    private static readonly List<DraftSlotState> SlotStates = [];
    private static readonly Dictionary<byte, int> PlayerToSlot = [];
    private static int _currentTurn;

    public static void SetDraftStateFromHost(int totalSlots, List<byte> playerIds, List<int> slotNumbers)
    {
        if (playerIds == null || slotNumbers == null) return;
        if (playerIds.Count != slotNumbers.Count) return;

        SlotStates.Clear();
        PlayerToSlot.Clear();

        for (var i = 0; i < playerIds.Count; i++)
        {
            var state = new DraftSlotState { PlayerId = playerIds[i], SlotNumber = slotNumbers[i] };
            SlotStates.Add(state);
            PlayerToSlot[playerIds[i]] = slotNumbers[i];
        }

        IsDraftActive = true;
        TotalSlots = Math.Max(totalSlots, playerIds.Count);
        CurrentTurnNumber = 0;
        CurrentTurnSlot = -1;
        DraftSidebarManager.InvalidateCache();
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
        DraftSidebarManager.InvalidateCache();
        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
            $"[DraftManager] AddSlotState: player {state.PlayerId} -> slot {state.SlotNumber}");
    }

    public static void SubmitPick(byte playerId, byte index)
    {
        var state = GetStateForPlayer(playerId);
        if (state == null) return;
        if (state.HasPicked || state.ChosenRoleId != 0) return;
        if (!state.IsPickingNow) return;
        if (state.PendingPickIndex != 255 && state.PendingPickTurnNumber == _currentTurn) return;

        state.PendingPickIndex = index;
        state.PendingPickTurnNumber = _currentTurn;

        if (AmongUsClient.Instance.AmHost && DraftEngineBehaviour.Instance != null)
        {
            DraftEngineBehaviour.Instance.TryApplySubmittedPick(playerId, index);
        }
    }

    public static void ConfirmPick(int slot, ushort roleId)
    {
        var state = GetStateForSlot(slot);
        if (state == null) return;

        if (state.HasPicked && state.ChosenRoleId != 0)
        {
            if (state.ChosenRoleId == roleId)
            {
                state.PendingPickIndex = 255;
                state.PendingPickTurnNumber = -1;
                return;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                $"[DraftManager] Ignoring duplicate pick confirmation for slot {slot}: existing role {state.ChosenRoleId}, new role {roleId}");
            return;
        }

        state.ChosenRoleId = roleId;
        state.HasPicked = true;
        state.IsPickingNow = false;
        state.PendingPickIndex = 255;
        state.PendingPickTurnNumber = -1;

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
            CurrentTurnNumber = turnNumber;
            CurrentTurnSlot = slot;
            foreach (var s in SlotStates)
            {
                s.IsPickingNow = false;
                s.PendingPickIndex = 255;
                s.PendingPickTurnNumber = -1;
            }
        }

        var target = SlotStates.FirstOrDefault(s => s.SlotNumber == slot);
        if (target != null)
        {
            CurrentTurnSlot = slot;
            target.IsPickingNow = true;
            target.PendingPickIndex = 255;
            target.PendingPickTurnNumber = turnNumber;
        }

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
        SlotStates.FirstOrDefault(s => s.SlotNumber == slot)!;

    public static DraftSlotState GetStateForPlayer(byte playerId) =>
        SlotStates.FirstOrDefault(s => s.PlayerId == playerId)!;

    public static IReadOnlyList<DraftSlotState> GetAllStates() => SlotStates.AsReadOnly();

    public static bool IsPlayerDisconnected(byte playerId)
    {
        var player = PlayerControl.AllPlayerControls?.ToArray()
            .FirstOrDefault(p => p != null && p.PlayerId == playerId);

        if (player == null) return true;
        if (player.Data == null || player.Data.Disconnected) return true;
        if (IsBotLikePlayer(player)) return true;

        try
        {
            var client = AmongUsClient.Instance.GetClient(player.OwnerId);
            if (client == null)
            {
                if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                    return false;
                return true;
            }
        }
        catch
        {
            //ignored
        }

        return false;
    }

    private static bool IsBotLikePlayer(PlayerControl player)
    {
        if (player == null) return false;

        try
        {
            var data = player.Data;
            if (data == null) return false;

            foreach (var candidate in new[] { data.GetType(), player.GetType() })
            {
                if (candidate == null) continue;

                var prop = candidate.GetProperty("IsBot", BindingFlags.Public | BindingFlags.Instance);
                if (prop?.GetValue(data) is bool isBot)
                    return isBot;

                var playerProp = candidate.GetProperty("IsDummy", BindingFlags.Public | BindingFlags.Instance);
                if (playerProp?.GetValue(data) is bool isDummy)
                    return isDummy;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    public static List<DraftSlotState> GetActivePickerStatesNonAlloc()
    {
        return SlotStates.Where(s => s != null && s.IsPickingNow).ToList();
    }

    public static void Reset(bool cancelledBeforeCompletion)
    {
        IsDraftActive = false;
        SlotStates.Clear();
        PlayerToSlot.Clear();
        _currentTurn = 0;
        CurrentTurnNumber = 0;
        CurrentTurnSlot = -1;
        TotalSlots = 0;
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
        catch
        {
            //ignored
        }
    }
}

public class RecapEntry(int slotNumber, string roleName, string teamLabel = null!, string colorHex = null!)
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
    public int PendingPickTurnNumber = -1;
    public string ForcedRoleName;
}