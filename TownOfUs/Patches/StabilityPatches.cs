using HarmonyLib;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class StabilityPatches
{
    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickUp))]
    [HarmonyPrefix]
    public static bool PrefixClick(PassiveButton __instance)
    {
        if (__instance == null || __instance.Pointer == IntPtr.Zero || __instance.WasCollected)
        {
            return false;
        }

        return true;
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
    [HarmonyPrefix]
    public static bool PrefixClick(HudManager __instance, PlayerControl reporter)
    {
        if (MeetingHud.Instance)
        {
            return false;
        }
        Info("Opening meeting room: " + ((reporter != null) ? reporter.ToString() : null));
        ShipStatus.Instance.RepairCriticalSabotages();
        MeetingHud.Instance = UnityEngine.Object.Instantiate(__instance.MeetingPrefab);
        if (reporter == null)
        {
            Error($"Meeting has a null reporter, resorting to displaying the local player!");
            MeetingHud.Instance.ServerStart(PlayerControl.LocalPlayer.PlayerId);
        }
        else
        {
            Info($"{reporter.CachedPlayerData.PlayerName} is starting a meeting!");
            MeetingHud.Instance.ServerStart(reporter.PlayerId);
        }
        AmongUsClient.Instance.Spawn(MeetingHud.Instance);
        try
        {
            GameData.OnMeetingStart();
            __instance.Chat.OnMeetingStart();
        }
        catch (Exception e)
        {
            Error(e);
        }
        return false;
    }
}
