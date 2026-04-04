using HarmonyLib;

namespace TownOfUs.Patches.Options;

[HarmonyPatch]
public static class VitalsBodyPatches
{
    internal static List<NetworkedPlayerInfo> MissingPlayers = new();

    public static void AddMissingPlayer(NetworkedPlayerInfo player)
    {
        MissingPlayers.Add(player);
        Warning($"Player {player.PlayerId} is now marked as missing.");
    }

    public static void RemoveMissingPlayer(NetworkedPlayerInfo player)
    {
        MissingPlayers.Remove(player);
        Warning($"Player {player.PlayerId} is no longer marked as missing.");
    }

    public static void ClearMissingPlayers()
    {
        MissingPlayers.Clear();
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    [HarmonyPostfix]
    public static void BeginPostfix(VitalsMinigame __instance)
    {
        for (int k = 0; k < __instance.vitals.Length; k++)
        {
            VitalsPanel vitalsPanel = __instance.vitals[k];
            if (MissingPlayers.Contains(vitalsPanel.PlayerInfo))
            {
                vitalsPanel.SetMissing();
            }
            else if (!vitalsPanel.PlayerInfo.IsDead && vitalsPanel.PlayerInfo.Disconnected && !vitalsPanel.IsDiscon)
            {
                vitalsPanel.SetDisconnected();
            }
            else if (vitalsPanel.PlayerInfo.IsDead && !vitalsPanel.IsDead && !vitalsPanel.IsDiscon)
            {
                vitalsPanel.SetDead();
            }
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    [HarmonyPrefix]
    public static bool UpdatePrefix(VitalsMinigame __instance)
    {
        if (__instance.SabText.isActiveAndEnabled &&
            !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
        {
            __instance.SabText.gameObject.SetActive(false);
            for (int i = 0; i < __instance.vitals.Length; i++)
            {
                __instance.vitals[i].gameObject.SetActive(true);
            }
        }
        else if (!__instance.SabText.isActiveAndEnabled &&
                 PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
        {
            __instance.SabText.gameObject.SetActive(true);
            for (int j = 0; j < __instance.vitals.Length; j++)
            {
                __instance.vitals[j].gameObject.SetActive(false);
            }
        }

        for (int k = 0; k < __instance.vitals.Length; k++)
        {
            VitalsPanel vitalsPanel = __instance.vitals[k];
            if (MissingPlayers.Contains(vitalsPanel.PlayerInfo))
            {
                vitalsPanel.SetMissing();
            }
            else if (!vitalsPanel.PlayerInfo.IsDead && vitalsPanel.PlayerInfo.Disconnected && !vitalsPanel.IsDiscon)
            {
                vitalsPanel.SetDisconnected();
            }
            else if (vitalsPanel.PlayerInfo.IsDead && !vitalsPanel.IsDead && !vitalsPanel.IsDiscon)
            {
                vitalsPanel.SetDead();
            }
        }

        return false;
    }

    public static void SetMissing(this VitalsPanel panel)
    {
        panel.IsDead = true;
        panel.IsDiscon = false;
        panel.Background.sprite = TouAssets.VitalBgMissin.LoadAsset();
        panel.Cardio.gameObject.SetActive(false);
    }
}
