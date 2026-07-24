using System.Collections;
using Hazel;
using HarmonyLib;
using InnerNet;
using Reactor.Networking;
using Reactor.Utilities;
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

        var reason = TouLocale.GetParsed("TouDraftKickReason", "You were kicked because you tried to join mid-draft. Please try again when lobby is open");

        Error($"Client {client.Id} ({client.PlayerName}) was kicked due to joining mid-draft.");

        Coroutines.Start(KickWithReason(__instance, client.Id, reason));
    }

    private static IEnumerator KickWithReason(InnerNetClient innerNetClient, int targetClientId, string reason)
    {
        var writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(Tags.GameDataTo);
        writer.Write(innerNetClient.GameId);
        writer.WritePacked(targetClientId);
        writer.StartMessage(byte.MaxValue);
        writer.Write((byte)ReactorGameDataFlag.SetKickReason);
        writer.Write(reason);
        writer.EndMessage();
        writer.EndMessage();
        innerNetClient.SendOrDisconnect(writer);
        writer.Recycle();

        yield return null;
        yield return new UnityEngine.WaitForSeconds(1f);

        innerNetClient.KickPlayer(targetClientId, false);
    }
}
