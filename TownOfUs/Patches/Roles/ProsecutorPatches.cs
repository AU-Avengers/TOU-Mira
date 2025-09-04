using HarmonyLib;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class ProsecutorPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.VoteForMe))]
    public static bool VotePatch(PlayerVoteArea __instance)
    {
        // Players who are dead can no longer vote, and dead player can't be voted either
        var votedPlayer = __instance.GetPlayer();
        if (PlayerControl.LocalPlayer.HasDied() || (votedPlayer != null && votedPlayer.HasDied()))
        {
            return false;
        }
        
        if (PlayerControl.LocalPlayer.Data.Role is not ProsecutorRole prosecutor)
        {
            return true;
        }

        if (__instance.Parent.state is MeetingHud.VoteStates.Proceeding or MeetingHud.VoteStates.Results)
        {
            return false;
        }

        if (__instance == prosecutor.ProsecuteButton && !prosecutor.SelectingProsecuteVictim)
        {
            prosecutor.SelectingProsecuteVictim = true;
            return false;
        }

        if (__instance != prosecutor.ProsecuteButton && __instance != MeetingHud.Instance.SkipVoteButton &&
            prosecutor.SelectingProsecuteVictim)
        {
            ProsecutorRole.RpcProsecute(PlayerControl.LocalPlayer, __instance.TargetPlayerId);
        }

        if (__instance == MeetingHud.Instance.SkipVoteButton && prosecutor.SelectingProsecuteVictim)
        {
            prosecutor.SelectingProsecuteVictim = false;
            prosecutor.ProsecuteVictim = byte.MaxValue;
        }

        return true;
    }
}