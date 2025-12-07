using HarmonyLib;
using TownOfUs.Events;
using TownOfUs.Modules;
using TownOfUs.Roles.Other;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
public static class FirstDeadPatch
{
    public static List<string> PlayerNames { get; set; } = [];
    public static List<string> FirstRoundPlayerNames { get; set; } = [];

    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        if (!SpectatorRole.TrackedSpectators.Contains(__instance.Data.PlayerName))
        {
            if (!FirstRoundPlayerNames.Contains(__instance.Data.PlayerName) && DeathEventHandlers.CurrentRound == 1)
            {
                FirstRoundPlayerNames.Add(__instance.Data.PlayerName);
            }

            if (PlayerNames.Count < 4 && !PlayerNames.Contains(__instance.Data.PlayerName))
            {
                PlayerNames.Add(__instance.Data.PlayerName);
            }
        }

        GameHistory.DeathHistory.Add((__instance.PlayerId, reason));
    }
}