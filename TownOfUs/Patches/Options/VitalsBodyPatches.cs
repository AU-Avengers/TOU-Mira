using AmongUs.Data;
using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using UnityEngine;

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

    [HarmonyPatch(typeof(ViperDeadBody), nameof(ViperDeadBody.FixedUpdate))]
    [HarmonyPrefix]
    public static bool ViperBodyFixedUpdatePrefix(ViperDeadBody __instance)
    {
        if (__instance.victimDissolving && __instance.dissolveCurrentTime > 0f)
        {
            __instance.dissolveCurrentTime -= Time.fixedDeltaTime;
            if (__instance.dissolveCurrentTime <= 0f)
            {
                __instance.myController.DisableCurrentTrackers();
                __instance.victimDissolving = false;
                __instance.spriteAnim.gameObject.SetActive(false);
                __instance.myController.cosmetics.HidePetViper();
                var result = (BodyVitalsMode)OptionGroupSingleton<GameMechanicOptions>.Instance.CleanedBodiesAppearance.Value;

                CrimeSceneComponent.ClearCrimeScene(__instance);
                if (result is BodyVitalsMode.Disconnected)
                {
                    if (__instance.myKiller.AmOwner)
                    {
                        DataManager.Player.Stats.IncrementStat(StatID.Role_Viper_BodiesDissolved);
                    }
                    __instance.gameObject.Destroy();
                    return false;
                }
                else
                {
                    __instance.Reported = true;
                    __instance.myCollider.enabled = false;
                    if (result is BodyVitalsMode.Missing)
                    {
                        var player = MiscUtils.PlayerById(__instance.ParentId);
                        if (player != null)
                        {
                            AddMissingPlayer(player.Data);
                        }
                    }
                    if (__instance.myKiller.AmOwner)
                    {
                        DataManager.Player.Stats.IncrementStat(StatID.Role_Viper_BodiesDissolved);
                        return false;
                    }
                }
            }
            else
            {
                float num = __instance.dissolveCurrentTime / __instance.maxDissolveTime;
                if (num <= 0.8f && num > 0.3f)
                {
                    if (__instance.dissolveStage == 1)
                    {
                        return false;
                    }
                    __instance.dissolveStage = 1;
                    __instance.spriteAnim.Play(__instance.dissolveAnims[0], 1f);
                    return false;
                }
                else if (num <= 0.3f)
                {
                    if (__instance.dissolveStage == 2)
                    {
                        return false;
                    }
                    __instance.dissolveStage = 2;
                    __instance.spriteAnim.Play(__instance.dissolveAnims[1], 1f);
                }
            }
        }

        return false;
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
