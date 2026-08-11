using HarmonyLib;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(GameData))]
public static class SummaryDisconnectFixPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        EndGamePatches.ContainedMeetingData.AddPlayerData(player);
        if (GameHistory.PlayerStats.TryGetValue(player.PlayerId, out var stats))
        {
            stats.LockDeathInfo = true;
            if (stats.PlayerState is StoredPlayerState.Alive)
            {
                stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
                stats.DeathString = TouLocale.Get("DiedToDisconnect");
            }
            stats.PlayerState = StoredPlayerState.Disconnected;
        }
    }
}
