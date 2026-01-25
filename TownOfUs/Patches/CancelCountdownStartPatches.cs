using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Patches.PrefabChanging;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
internal static class CancelCountdownStart
{
    internal static PassiveButton CancelStartButton;

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPrefix]
    public static void PrefixStart(GameStartManager __instance)
    {
        CancelStartButton = Object.Instantiate(__instance.StartButton, __instance.transform);
        CancelStartButton.name = "CancelButton";

        var cancelLabel = CancelStartButton.buttonText;
        cancelLabel.gameObject.GetComponent<TextTranslatorTMP>()?.OnDestroy();
        cancelLabel.text = "";

        var cancelButtonInactiveRenderer = CancelStartButton.inactiveSprites.GetComponent<SpriteRenderer>();
        cancelButtonInactiveRenderer.color = new Color(0.8f, 0f, 0f, 1f);

        var cancelButtonActiveRenderer = CancelStartButton.activeSprites.GetComponent<SpriteRenderer>();
        cancelButtonActiveRenderer.color = Color.red;

        var cancelButtonInactiveShine = CancelStartButton.inactiveSprites.transform.Find("Shine");

        if (cancelButtonInactiveShine)
        {
            cancelButtonInactiveShine.gameObject.SetActive(false);
        }

        CancelStartButton.activeTextColor = CancelStartButton.inactiveTextColor = Color.white;

        CancelStartButton.OnClick = new Button.ButtonClickedEvent();
        CancelStartButton.OnClick.AddListener((UnityAction)(() =>
        {
            if (__instance.countDownTimer < 4f)
            {
                __instance.ResetStartState();
            }
        }));
        CancelStartButton.gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    [HarmonyPostfix]
    public static void PostfixBeginGame(GameStartManager __instance)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            if (OptionGroupSingleton<HostSpecificOptions>.Instance.NoGameEnd && TownOfUsPlugin.IsDevBuild)
            {
                var warningText = "<color=#FF0000><b>Warning: No Game End is enabled. The game will not end automatically.</b></color>";
                var notif = Helpers.CreateAndShowNotification(warningText, Color.red, new Vector3(0f, 1f, -20f)); // I'm not good enough with vectors to place this properly
                notif.AdjustNotification();
            }

            var curMap = (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
            var defaultDoorType = curMap switch
            {
                ExpandedMapNames.Skeld or ExpandedMapNames.Dleks => MapDoorType.Skeld,
                ExpandedMapNames.Polus => MapDoorType.Polus,
                ExpandedMapNames.Airship => MapDoorType.Airship,
                ExpandedMapNames.Fungle => MapDoorType.Fungle,
                ExpandedMapNames.Submerged => MapDoorType.Submerged,
                _ => MapDoorType.None
            };
            var doorType = RandomDoorMapOptions.GetRandomDoorType(defaultDoorType);
            RpcSetRandomDoors(PlayerControl.LocalPlayer, (int)doorType);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetRandomDoors)]
    public static void RpcSetRandomDoors(PlayerControl player, int doorType)
    {
        if (!player.IsHost() || doorType == (int)MapDoorType.None)
        {
            return;
        }

        MapDoorPatches.RandomDoorType = (MapDoorType)doorType;
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPostfix]
    public static void PostfixStart(GameStartManager __instance)
    {
        /*if (MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek)
        {
        }*/
        __instance.MinPlayers = 1;
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
    [HarmonyPrefix]
    public static void Prefix(GameStartManager __instance)
    {
        if (__instance.startState is GameStartManager.StartingStates.Countdown)
        {
            SoundManager.Instance.StopSound(__instance.gameStartSound);
            if (AmongUsClient.Instance.AmHost)
            {
                GameManager.Instance.LogicOptions.SyncOptions();
            }
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.SetStartCounter))]
    [HarmonyPrefix]
    public static void Prefix(GameStartManager __instance, sbyte sec)
    {
        if (sec == -1)
        {
            SoundManager.Instance.StopSound(__instance.gameStartSound);
        }
        else
        {
            CancelStartButton.gameObject.SetActive(false);
        }
    }
}