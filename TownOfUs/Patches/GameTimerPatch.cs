﻿using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using UnityEngine;
using TMPro;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class GameTimerPatch
{
    public static bool Enabled { get; set; }
    public static bool TriggerEndGame { get; set; }
    public static float GameTimer { get; set; }

    public static GameObject GameTimerObj;
    public static GameObject TimerSpriteObj;
    public static SpriteRenderer TimerSprite;

    private static void CreateGameTimer(HudManager instance)
    {
        var pingTracker = UnityEngine.Object.FindObjectOfType<PingTracker>(true);
        GameTimerObj = UnityEngine.Object.Instantiate(pingTracker.gameObject, instance.transform);
        GameTimerObj.name = "GameTimerText";

        GameTimerObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.6f, 5.5f);
        GameTimerObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;

        TimerSpriteObj = new GameObject("TimerSprite");
        TimerSpriteObj.transform.SetParent(GameTimerObj.transform);
        TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.4f, 1f);
        TimerSpriteObj.gameObject.layer = GameTimerObj.gameObject.layer;
        TimerSpriteObj.SetActive(true);

        TimerSprite = TimerSpriteObj.AddComponent<SpriteRenderer>();
        TimerSprite.sprite = TouAssets.TimerDrawSprite.LoadAsset();

        TimeSpan ts = TimeSpan.FromSeconds(GameTimer);

        var timerText = GameTimerObj.GetComponent<TextMeshPro>();
        timerText.text = $"<size=200%>Time:{ts.ToString(format: @"mm\:ss", TownOfUsPlugin.Culture)}</size>";
        timerText.alignment = TextAlignmentOptions.TopLeft;
        timerText.verticalAlignment = VerticalAlignmentOptions.Top;

        GameTimerObj.SetActive(false);
    }

    public static void UpdateGameTimer(HudManager instance)
    {
        if (GameTimerObj != null)
        {
            GameTimerObj.SetActive(false);
        }

        if (!OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled)
        {
            return;
        }

        if (GameTimerObj == null)
        {
            CreateGameTimer(instance);
        }

        if (GameTimerObj == null)
        {
            return;
        }

        var inMeeting = MeetingHud.Instance || ExileController.Instance;
        if (Enabled && GameTimer > 0 && (!inMeeting || GameTimer > 240f))
        {
            GameTimer -= Time.deltaTime;
            GameTimer = Math.Max(GameTimer, 0);

            if (AmongUsClient.Instance.AmHost && GameTimer <= 0)
            {
                EndGame();
            }
        }

        TimeSpan ts = TimeSpan.FromSeconds(GameTimer);

        var timerText = GameTimerObj.GetComponent<TextMeshPro>();

        var colour = GameTimer switch
        {
            < 30f => Color.red,
            < 60f => Color.yellow,
            _ => Color.green
        };

        if (!MeetingHud.Instance)
        {
            GameTimerObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.6f, 5.5f);
            GameTimerObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;
            timerText.text = $"<size=200%>Time:{colour.ToTextColor()}{ts.ToString(format: @"mm\:ss", TownOfUsPlugin.Culture)}</color></size>";
            TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.4f, 1f);
        }
        else
        {
            GameTimerObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.25f, 0.9f);
            GameTimerObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;
            timerText.text = $"<size=130%>Time:{colour.ToTextColor()}{ts.ToString(format: @"mm\:ss", TownOfUsPlugin.Culture)}</color></size>";
            TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.25f, 1f);
        }

        GameTimerObj.SetActive(!ExileController.Instance);
    }

    private static void EndGame()
    {
        Enabled = false;
        TriggerEndGame = true;
        GameManager.Instance.ShouldCheckForGameEnd = true;
    }

    public static void BeginTimer()
    {
        Enabled = true;
        TriggerEndGame = false;
        GameTimer = OptionGroupSingleton<GameTimerOptions>.Instance.GameTimeLimit.GetFloatData() * 60f;
        
        if ((GameTimerType)OptionGroupSingleton<GameTimerOptions>.Instance.TimerEndOption.Value is GameTimerType.Impostors)
        {
            TimerSprite.sprite = TouAssets.TimerImpSprite.LoadAsset();
        }
        else
        {
            TimerSprite.sprite = TouAssets.TimerDrawSprite.LoadAsset();
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            TutorialManager.InstanceExists ||
            AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
        {
            return;
        }

        UpdateGameTimer(__instance);
    }
}
