// https://github.com/eDonnes124/Town-Of-Us-R/blob/ee0935bfbd35199b5d4f6f4ad9cf98621acb6d21/source/Patches/LadderFix.cs

using System.Collections;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.HnsGame.Crewmate;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoClimbLadder))]
public static class LadderFix
{
    public static void Postfix(PlayerPhysics __instance, ref IEnumerator __result, Ladder source, byte climbLadderSid)
    {
        __result = Helpers.CreateWrapper(__result, () => 
        {
            var player = __instance.myPlayer;

            if (!source.IsTop && (player.HasModifier<GiantModifier>() || player.HasModifier<HnsGiantModifier>()))
            {
                Error("Giant player on ladder detected, snapping position.");
                player.NetTransform.SnapTo(player.transform.position + new Vector3(0, 0.25f));
            }

            if (source.IsTop && (player.HasModifier<MiniModifier>() || player.HasModifier<HnsMiniModifier>()))
            {
                Error("Mini player on ladder detected, snapping position.");
                player.NetTransform.SnapTo(player.transform.position + new Vector3(0, -0.25f));
            }
        });
    }
}