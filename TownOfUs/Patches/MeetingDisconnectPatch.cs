using HarmonyLib;
using MiraAPI.Utilities;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(GameData))]
public static class MeetingDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (MeetingHud.Instance)
        {
            foreach (var pva in MeetingHud.Instance.playerStates)
            {
                if (pva.VotedForId != player.PlayerId || pva.AmDead)
                {
                    continue;
                }

                pva.UnsetVote();

                var voteAreaPlayer = MiscUtils.PlayerById(pva.PlayerId);

                if (voteAreaPlayer == null)
                {
                    continue;
                }

                var voteData = voteAreaPlayer.GetVoteData();
                var votes = voteData.Votes.RemoveAll(x => x.Suspect == player.PlayerId);
                voteData.VotesRemaining += votes;

                MeetingHud.Instance.ClearVote(pva.PlayerId, voteAreaPlayer.AmOwner);
            }
        }
    }
}