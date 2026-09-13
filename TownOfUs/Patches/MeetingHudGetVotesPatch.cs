using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(MeetingHud))]
public static class MeetingHudGetVotesPatch
{
    public static MeetingHud.VoterState[] States { get; private set; } = [];

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.VotingComplete))]
    public static void VotingCompletePrefix(Il2CppStructArray<MeetingHud.VoterState> states)
    {
        // CODE REVIEW 22/2/2025 AEDT (D/M/Y)
        // ---------------------------------
        // Why?
        States = states;
        // 4/4/2025 - XtraCube
        // Caching the states lets us use voter state to make haunt menu for Jester after exile.
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.OnDestroy))]
    public static void OnDestroyPostfix()
    {
        States = [];
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void StartPostfix()
    {
        States = [];
        Warning($"Finished running meeting patches!");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static bool StartPrefix(MeetingHud __instance)
    {
        Warning($"Running regular meeting code");
        __instance.BlackBackground.sprite = ShipStatus.Instance.MeetingBackground;
        __instance.SetMasksEnabled(false);
        foreach (var playerMaterialColors in __instance.PlayerColoredParts)
        {
            PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors);
        }
        DestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(true);
        DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
        DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
        __instance.SkipVoteButton.SetPlayerId(253);
        __instance.SkipVoteButton.Parent = __instance;
        Camera.main!.GetComponent<FollowerCamera>().Locked = true;
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            __instance.SetForegroundForDead();
        }

        var handler = __instance.TryCast<IDisconnectHandler>();
        if (!AmongUsClient.Instance.DisconnectHandlers.Contains(handler))
        {
            AmongUsClient.Instance.DisconnectHandlers.Add(handler);
        }
        foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
        {
            if (!playerVoteArea.AmDead)
            {
                __instance.ControllerSelectable.Add(playerVoteArea.PlayerButton);
            }
        }
        DestroyableSingleton<AchievementManager>.Instance.OnMeetingCalled();
        Warning($"About to run patches!");
        return false;
    }
}