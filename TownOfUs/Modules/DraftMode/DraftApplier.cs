using AmongUs.GameOptions;

namespace TownOfUs.Modules.DraftMode;

public static class DraftApplier
{
    internal static List<DraftSlotState> PendingDraftStates = [];

    public static void Clear()
    {
        PendingDraftStates.Clear();
    }

    public static void StorePendingDraftStates(IReadOnlyList<DraftSlotState> slotStates)
    {
        PendingDraftStates = [.. slotStates.Where(state => state != null && MiscUtils.PlayerById(state.PlayerId) != null)];
    }

    public static void ApplyDraftResults(IReadOnlyList<DraftSlotState> slotStates)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        foreach (var state in slotStates)
        {
            var player = MiscUtils.PlayerById(state.PlayerId);
            if (player == null) continue;

            player.RpcSetRole((RoleTypes)state.ChosenRoleId);
        }
    }

    public static void VerifyAppliedDraftResults()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        foreach (var state in PendingDraftStates)
        {
            var player = MiscUtils.PlayerById(state.PlayerId);
            if (player?.Data?.Role == null || state.ChosenRoleId == 0 ||
                (ushort)player.Data.Role.Role == state.ChosenRoleId)
            {
                continue;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                $"[DraftApplier] Player {state.PlayerId} drafted role {state.ChosenRoleId} but ended with {(ushort)player.Data.Role.Role}; reapplying draft role");
            player.RpcSetRole((RoleTypes)state.ChosenRoleId);
        }

        PendingDraftStates.Clear();
    }
}