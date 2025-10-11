using HarmonyLib;
using TMPro;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(MeetingHud))]
public static class ShowHostMeetingPatch
{
    private static TextMeshPro? HostName { get; set; }

    [HarmonyPatch(nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    public static void MeetingUpdatePatch(MeetingHud __instance)
    {
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
        {
            return;
        }

        var host = GameData.Instance.GetHost();

        if (host != null && HostName)
        {
            PlayerMaterial.SetColors(host.DefaultOutfit.ColorId, __instance.HostIcon);
            HostName!.text = $"{HudManagerPatches.StoredHostLocale}: {host.PlayerName}";
        }
    }

    [HarmonyPatch(nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void Setup(MeetingHud __instance)
    {
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
        {
            return;
        }

        __instance.ProceedButton.transform.localPosition = new Vector3(-2.5f, 2.2f, 0);
        __instance.ProceedButton.GetComponent<SpriteRenderer>().enabled = false;
        __instance.ProceedButton.enabled = false;
        __instance.HostIcon.enabled = true;
        __instance.HostIcon.gameObject.SetActive(true);
        __instance.ProceedButton.gameObject.SetActive(true);
        HostName = __instance.ProceedButton.GetComponentInChildren<TextMeshPro>();
    }
}