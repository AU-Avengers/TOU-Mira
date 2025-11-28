using HarmonyLib;
using MiraAPI.Utilities;
using TownOfUs.Utilities;

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
            foreach (var pva in MeetingHud.Instance.playerStates)
            {
                if (pva.VotedFor != player.PlayerId || pva.AmDead)
                {
                    continue;
                }

                pva.UnsetVote();

                var voteAreaPlayer = MiscUtils.PlayerById(pva.TargetPlayerId);

                if (voteAreaPlayer == null)
                {
                    continue;
                }

                var voteData = voteAreaPlayer.GetVoteData();
                var votes = voteData.Votes.RemoveAll(x => x.Suspect == player.PlayerId);
                voteData.VotesRemaining += votes;

                if (!voteAreaPlayer.AmOwner)
                {
                    continue;
                }

                MeetingHud.Instance.ClearVote();
            }
        }
    }
}