using AmongUs.GameOptions;

namespace TownOfUs.Modules.DraftMode;

public static class DraftApplier
{
    internal static List<DraftSlotState> PendingDraftStates = [];

    public static void StorePendingDraftStates(IReadOnlyList<DraftSlotState> slotStates)
    {
        PendingDraftStates = [.. slotStates];
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
}
