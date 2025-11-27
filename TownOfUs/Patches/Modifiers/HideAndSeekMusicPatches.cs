using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.HnsGame.Crewmate;
using TownOfUs.Modifiers.HnsImpostor;
using TownOfUs.Options.Roles.HnsImpostor;
using UnityEngine;

namespace TownOfUs.Patches.Modifiers;

[HarmonyPatch]
public static class HideAndSeekMusicPatches
{
    [HarmonyPatch(typeof(LogicHnSMusic), nameof(LogicHnSMusic.InitMusic))]
    [HarmonyPrefix]
    public static bool InitMusic(LogicHnSMusic __instance)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<HnsObliviousModifier>())
        {
            return true;
        }
        AudioClip clip = ((__instance.Manager as HideAndSeekManager)!.LogicOptionsHnS.GetEscapeTime() <= 180f)
            ? __instance.musicCollection.ImpostorShortMusic
            : __instance.musicCollection.ImpostorLongMusic;
        if (AprilFoolsMode.ShouldHorseAround())
        {
            clip = __instance.musicCollection.ImpostorRanchMusic;
        }

        if (__instance.normalSource == null)
        {
            __instance.normalSource = SoundManager.Instance.GetNamedSfxSource(__instance.musicNames[LogicHnSMusic.HideAndSeekMusicTrack.Normal]);
        }
        __instance.normalSource.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        __instance.normalSource.clip = clip;
        __instance.normalSource.loop = true;
        if (__instance.taskSource == null)
        {
            __instance.taskSource = SoundManager.Instance.GetNamedSfxSource(__instance.musicNames[LogicHnSMusic.HideAndSeekMusicTrack.Task]);
        }
        __instance.taskSource.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        __instance.taskSource.volume = 0f;
        __instance.taskSource.clip = clip;
        __instance.taskSource.loop = true;
        if (__instance.dangerLevel1Source == null)
        {
            __instance.dangerLevel1Source = SoundManager.Instance.GetNamedSfxSource(__instance.musicNames[LogicHnSMusic.HideAndSeekMusicTrack.DangerLevel1]);
        }
        __instance.dangerLevel1Source.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        __instance.dangerLevel1Source.volume = 0f;
        __instance.dangerLevel1Source.clip = clip;
        __instance.dangerLevel1Source.loop = true;
        if (__instance.dangerLevel2Source == null)
        {
            __instance.dangerLevel2Source = SoundManager.Instance.GetNamedSfxSource(__instance.musicNames[LogicHnSMusic.HideAndSeekMusicTrack.DangerLevel2]);
        }
        __instance.dangerLevel2Source.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        __instance.dangerLevel2Source.volume = 0f;
        __instance.dangerLevel2Source.clip = clip;
        __instance.dangerLevel2Source.loop = true;
        __instance.normalSource.Play();
        __instance.taskSource.Play();
        __instance.dangerLevel1Source.Play();
        __instance.dangerLevel2Source.Play();
        __instance.SyncMusic();
        return false;
    }

    /*[HarmonyPatch(typeof(LogicHnSMusic), nameof(LogicHnSMusic.FixedUpdate))]
    [HarmonyPrefix]
    public static bool FixedUpdatePrefix(LogicHnSMusic __instance)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<HnsGlobalCamouflageModifier>() || !OptionGroupSingleton<HnsCamouflagerOptions>.Instance.CamoDisablesProxBar)
        {
            return true;
        }
        if (__instance.normalSource == null || __instance.taskSource == null || __instance.dangerLevel1Source == null || __instance.dangerLevel2Source == null)
        {
            return false;
        }
        if (Time.unscaledTime > __instance.lastMusicSyncTime + 1f)
        {
            __instance.SyncMusic();
        }
        __instance.normalSource.volume = 0;
        __instance.taskSource.volume = 1;
        __instance.dangerLevel1Source.volume = 0;
        __instance.dangerLevel2Source.volume = 0;
        return false;
    }*/


    [HarmonyPatch(typeof(LogicHnSDangerLevel), nameof(LogicHnSDangerLevel.FixedUpdate))]
    [HarmonyPrefix]
    public static bool FixedUpdatePrefix(LogicHnSDangerLevel __instance)
    {
        if ((!PlayerControl.LocalPlayer.HasModifier<HnsGlobalCamouflageModifier>() ||
             !OptionGroupSingleton<HnsCamouflagerOptions>.Instance.CamoDisablesProxBar) &&
            !PlayerControl.LocalPlayer.HasModifier<HnsObliviousModifier>())
        {
            return true;
        }

        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        if (__instance.impostors == null || localPlayer == null)
        {
            return false;
        }

        if (__instance.impostors.Count <= 0)
        {
            return false;
        }

        __instance.dangerLevel1 = 0f;
        __instance.dangerLevel2 = 0f;
        __instance.UpdateDangerMusic();
        if (__instance.dangerMeter != null)
        {
            __instance.dangerMeter.SetDangerValue(0f, 0f);
        }

        return false;
    }
}