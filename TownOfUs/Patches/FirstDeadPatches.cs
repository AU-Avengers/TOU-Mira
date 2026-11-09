using HarmonyLib;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Other;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
public static class FirstDeadPatch
{
    public static List<string> PlayerNames { get; set; } = [];
    public static List<string> FirstRoundPlayerNames { get; set; } = [];

    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        var bestNameFit = __instance.GetBestPlayerDenomination();
        if (!SpectatorRole.TrackedSpectators.Contains(bestNameFit))
        {
            if (!FirstRoundPlayerNames.Contains(bestNameFit) && HudManagerHelper.Instance.CurrentRound == 1)
            {
                FirstRoundPlayerNames.Add(bestNameFit);
            }

            if (PlayerNames.Count < 4 && !PlayerNames.Contains(bestNameFit))
            {
                PlayerNames.Add(bestNameFit);
            }
        }

        GameHistory.DeathHistory.Add((__instance.PlayerId, reason));
    }
}