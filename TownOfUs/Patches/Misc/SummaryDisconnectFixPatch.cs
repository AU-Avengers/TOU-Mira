using HarmonyLib;
using MiraAPI.Modifiers;
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
            var newList = new List<BaseModifier>();
            foreach (var modifier in stats.LastKnownModifiers)
            {
                newList.Add(MiscUtils.AllModifiers.First(x => x.GetType() == modifier.GetType()));
            }
            stats.LastKnownModifiers = newList;
            stats.LockDeathInfo = true;
            if (stats.PlayerState is StoredPlayerState.Alive)
            {
                stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
                stats.DeathString = MiraLocaleManager.Get("DiedToDisconnect");
            }
            stats.PlayerState = StoredPlayerState.Disconnected;
        }
    }
}
