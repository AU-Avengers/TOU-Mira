using HarmonyLib;
using InnerNet;
using TownOfUs.Modules.DraftMode;

namespace TownOfUs.Patches.DraftMode;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public static class KickOnJoinWhileLockedPatch
{
    [HarmonyPostfix]
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        if (!DraftManager.IsDraftActive) return;
        if (!AmongUsClient.Instance.AmHost) return;

        Error($"Client {client.Id} ({client.PlayerName}) was kicked due to joining mid-draft.");

        AmongUsClient.Instance.KickPlayer(client.Id, false);
    }
}