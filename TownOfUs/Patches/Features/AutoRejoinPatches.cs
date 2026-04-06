using HarmonyLib;
using InnerNet;
using TownOfUs.Modules.AutoRejoin;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Features;

// Thanks Maxi0fc (https://github.com/Maxi0fc), taken from https://github.com/Maxi0fc/AutoRejoin/blob/main/AutoRejoin.cs
[HarmonyPatch]
public static class AutoRejoinPatches
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    [HarmonyPostfix]
    public static void EndGameStartPostfix(EndGameManager __instance)
    {
        if (AmongUsClient.Instance == null) return;
        var rejoinSelection = LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.AutoRejoinMode.Value;
        var willRejoin = rejoinSelection is AutoRejoinSelection.Always;
        if (rejoinSelection is AutoRejoinSelection.Never)
        {
            willRejoin = false;
        }
        else if (rejoinSelection is AutoRejoinSelection.Client && !AmongUsClient.Instance.AmHost)
        {
            willRejoin = true;
        }
        else if (rejoinSelection is AutoRejoinSelection.Host && AmongUsClient.Instance.AmHost)
        {
            willRejoin = true;
        }
        Warning($"Rejoining condition: {rejoinSelection} | Will Rejoin: {willRejoin} | Is Host: {AmongUsClient.Instance.AmHost}");
        if (!willRejoin) return;

        RejoinBehaviour.PendingRejoin = true;
        RejoinBehaviour.SavedGameId = AmongUsClient.Instance.GameId;
        RejoinBehaviour.CurrentEndGameManager = __instance;

        Info(
            $"[AutoRejoin] Game ended. Code: {GameCode.IntToGameName(RejoinBehaviour.SavedGameId)}");

        // Create GUI here while scene is active
        if (RejoinBehaviour.GuiObject == null)
        {
            var go = new GameObject("AutoRejoinGUI");
            Object.DontDestroyOnLoad(go);
            RejoinBehaviour.GuiObject = go.AddComponent<CountdownGui>();
            Info("[AutoRejoin] CountdownGui created.");
        }

        RejoinBehaviour.TriggerRejoin();
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void LobbyStartPostfix()
    {
        if (!RejoinBehaviour.PendingRejoin) return;
        Info("[AutoRejoin] Back in lobby — done.");
        RejoinBehaviour.CancelRejoin();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    [HarmonyPrefix]
    public static void ExitGamePrefix()
    {
        if (RejoinBehaviour.PendingRejoin)
        {
            Info("[AutoRejoin] Manual exit — cancelling.");
            RejoinBehaviour.CancelRejoin();
        }
    }
}
