using HarmonyLib;
using MiraAPI.Patches;

namespace TownOfUs.Patches.Misc;

/// <summary>
/// Preloads the Mira wiki/role guide panels when a game starts so opening the guide
/// in-game does not cause a noticeable hitch.
/// </summary>
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
internal static class WikiPreloadPatch
{
    public static void Postfix()
    {
        if (MatchInfoGuide.Instance)
        {
            RoleGuidePatches.TriggerPreload(MatchInfoGuide.Instance);
        }
    }
}
