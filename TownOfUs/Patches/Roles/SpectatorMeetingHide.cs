using HarmonyLib;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(MeetingHud))]
public static class HideSpecVoteAreas
{
    private static List<PlayerVoteArea> _spectatorAreas = [];
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void Postfix(MeetingHud __instance)
    {
        if (SpectatorRole.TrackedSpectators.Count == 0 || __instance.playerStates.Length == 0)
        {
            return;
        }
        if (_spectatorAreas.Count == 0)
        {
            _spectatorAreas = __instance.playerStates.Where(x => GameData.Instance.GetPlayerById(x.PlayerId)
                .Role is SpectatorRole).ToList();
        }
        foreach (var voteArea in _spectatorAreas)
        {
            if (!voteArea.gameObject.active)
            {
                continue;
            }
            voteArea.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(nameof(MeetingHud.SortButtons))]
    public static bool Prefix(MeetingHud __instance)
    {
        _spectatorAreas.Clear();
        var stateOrders = new Dictionary<PlayerVoteArea, int>();
        foreach (var state in __instance.playerStates)
        {
            if (!state.AmDead)
            {
                stateOrders.Add(state, state.PlayerId);
            }
            else if (GameData.Instance.GetPlayerById(state.PlayerId)?
                         .Role is SpectatorRole)
            {
                stateOrders.Add(state, state.PlayerId + 600);
                _spectatorAreas.Add(state);
            }
            else
            {
                stateOrders.Add(state, state.PlayerId + 300);
            }
        }

        var finalArray = stateOrders.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
        for (int i = 0; i < finalArray.Length; i++)
        {
            int num = i % 3;
            int num2 = i / 3;
            finalArray[i].transform.localPosition = __instance.VoteOrigin + new Vector3(__instance.VoteButtonOffsets.x * num,
                __instance.VoteButtonOffsets.y * num2, -0.9f - num2 * 0.01f);
        }

        return false;
    }
}