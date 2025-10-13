using HarmonyLib;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(GameData))]
public static class MeetingDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (MeetingHud.Instance != null)
        {
            var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == player.PlayerId);

            if (!targetVoteArea)
            {
                return;
            }

            if (targetVoteArea.DidVote)
            {
                targetVoteArea.UnsetVote();
            }
        }
    }
}