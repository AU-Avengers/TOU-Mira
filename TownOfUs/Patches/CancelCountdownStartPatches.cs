using HarmonyLib;
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