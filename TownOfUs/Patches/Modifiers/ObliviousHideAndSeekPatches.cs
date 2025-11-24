using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.HnsGame.Crewmate;
using UnityEngine;

namespace TownOfUs.Patches.Modifiers;

[HarmonyPatch]
public static class ObliviousHideAndSeekPatches
{
    [HarmonyPatch(typeof(LogicHnSMusic), nameof(LogicHnSMusic.StartMusicWithIntro))]
    [HarmonyPrefix]
    public static bool StartMusicWithIntroPrefix(LogicHnSMusic __instance)
    {
        if (PlayerControl.LocalPlayer.HasModifier<HnsObliviousModifier>())
        {
            AudioClip clip = ((__instance.Manager as HideAndSeekManager)!.LogicOptionsHnS.GetEscapeTime() <= 180f)
                ? __instance.musicCollection.ImpostorShortMusic
                : __instance.musicCollection.ImpostorLongMusic;
            if (AprilFoolsMode.ShouldHorseAround())
            {
                clip = __instance.musicCollection.ImpostorRanchMusic;
            }

            SoundManager.Instance.PlaySound(clip, true, 1f, SoundManager.Instance.MusicChannel);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(LogicHnSMusic), nameof(LogicHnSMusic.SetMusicValues))]
    [HarmonyPrefix]
    public static bool SetMusicValuesPrefix()
    {
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.HasModifier<HnsObliviousModifier>())
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(LogicHnSDangerLevel), nameof(LogicHnSDangerLevel.OnGameStart))]
    [HarmonyPrefix]
    public static bool GameStartPrefix(LogicHnSDangerLevel __instance)
    {
        if (PlayerControl.LocalPlayer.HasModifier<HnsObliviousModifier>())
        {
            __instance.firstMusicActivation = true;
            __instance.impostors = new();
            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
                NetworkedPlayerInfo data = playerControl.Data;
                if (((data != null) ? data.Role : null) != null && playerControl.Data.Role.IsImpostor)
                {
                    __instance.impostors.Add(playerControl);
                }
            }
            __instance.scaryMusicDistance = __instance.hnsManager.LogicOptionsHnS.GetScaryMusicDistance() * __instance.hnsManager.LogicOptionsHnS.PlayerSpeedBase;
            __instance.veryScaryMusicDistance = __instance.hnsManager.LogicOptionsHnS.GetVeryScaryMusicDistance() * __instance.hnsManager.LogicOptionsHnS.PlayerSpeedBase;
            if (__instance.scaryMusicDistance < __instance.veryScaryMusicDistance)
            {
                float num = __instance.veryScaryMusicDistance;
                float num2 = __instance.scaryMusicDistance;
                __instance.scaryMusicDistance = num;
                __instance.veryScaryMusicDistance = num2;
            }
            return false;
        }

        return true;
    }
}