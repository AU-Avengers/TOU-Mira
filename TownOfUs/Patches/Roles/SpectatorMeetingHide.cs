using HarmonyLib;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(MeetingHud))]
public static class HideSpecVoteAreas
{
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var voteArea in __instance.playerStates)
        {
            if (SpectatorRole.TrackedSpectators.Contains(GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).PlayerName))
                voteArea.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(nameof(MeetingHud.SortButtons))]
    public static bool Prefix(MeetingHud __instance)
    {
        var array = __instance.playerStates.OrderBy(delegate(PlayerVoteArea p)
        {
            if (!p.AmDead)
                return 0;

            if (SpectatorRole.TrackedSpectators.Contains(GameData.Instance.GetPlayerById(p.TargetPlayerId).PlayerName))
                return 100;

            return 50;
        }).ThenBy(p => p.TargetPlayerId).ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            int num = i % 3;
            int num2 = i / 3;
            array[i].transform.localPosition = __instance.VoteOrigin + new Vector3(__instance.VoteButtonOffsets.x * num, __instance.VoteButtonOffsets.y * num2, -0.9f - num2 * 0.01f);
        }
        return false;
    }
}