using HarmonyLib;
using TownOfUs.Modules;
using TownOfUs.Roles.Other;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
public static class FirstDeadPatch
{
    public static List<string> PlayerNames { get; set; } = [];

    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        if (PlayerNames.Count < 4 && !PlayerNames.Contains(__instance.Data.PlayerName) &&
            !SpectatorRole.TrackedSpectators.Contains(__instance.Data.PlayerName))
        {
            PlayerNames.Add(__instance.Data.PlayerName);
        }

        GameHistory.DeathHistory.Add((__instance.PlayerId, reason));
    }
}