/*using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using TMPro;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class HexBombTimerPatch
{
    public static GameObject GameTimerObj;
    public static GameObject TimerSpriteObj;
    public static SpriteRenderer TimerSprite;
    private static HexBombSabotageSystem _sabotage;
    private static float _timer => _sabotage.TimeRemaining;
    public static bool GameTimerEnabled => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled;
    
    public static void CreateHexTimer(HudManager instance, HexBombSabotageSystem saboInstance)
    {
        _sabotage = saboInstance;
        var pingTracker = Object.FindObjectOfType<PingTracker>(true);
        GameTimerObj = Object.Instantiate(pingTracker.gameObject, instance.transform);
        GameTimerObj.name = "HexTimerText";

        GameTimerObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.6f, 5.5f);
        GameTimerObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;

        TimerSpriteObj = new GameObject("HexTimerSprite");
        TimerSpriteObj.transform.SetParent(GameTimerObj.transform);
        TimerSpriteObj.transform.localScale *= 0.33f;
        TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.4f, -2f);
        TimerSpriteObj.gameObject.layer = GameTimerObj.gameObject.layer;
        TimerSpriteObj.SetActive(true);

        TimerSprite = TimerSpriteObj.AddComponent<SpriteRenderer>();
        TimerSprite.sprite = TouRoleIcons.Spellslinger.LoadAsset();

        var ts = TimeSpan.FromSeconds(_sabotage.TimeRemaining);

        var timerText = GameTimerObj.GetComponent<TextMeshPro>();
        timerText.text = $"<size=200%>Hex Time:{ts.ToString(@"mm\:ss", TownOfUsPlugin.Culture)}</size>";
        timerText.alignment = TextAlignmentOptions.TopLeft;
        timerText.verticalAlignment = VerticalAlignmentOptions.Top;

        GameTimerObj.SetActive(false);
    }

    public static void UpdateGameTimer(HudManager instance)
    {
        if (GameTimerObj == null)
        {
            return;
        }

        if (MeetingHud.Instance == null && ExileController.Instance == null)
        {
            GameTimerObj.SetActive(false);
            return;
        }
        GameTimerObj.SetActive(true);

        var ts = TimeSpan.FromSeconds(_timer);

        var timerText = GameTimerObj.GetComponent<TextMeshPro>();

        var colour = _timer switch
        {
            < 30f => Color.red,
            < 60f => Color.yellow,
            _ => Color.white
        };
        GameTimerObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.25f, 0.9f);
        GameTimerObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;
        timerText.text =
            $"<size=130%>Hex Time:{colour.ToTextColor()}{ts.ToString(@"mm\:ss", TownOfUsPlugin.Culture)}</color></size>";
        TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.25f, -2f);
        /*if (GameTimerEnabled && !TutorialManager.InstanceExists)
        {
            TimerSpriteObj.transform.localPosition = new Vector3(-1.5f, -0.25f, 0f);
            GameTimerPatch.TimerSpriteObj.transform.localPosition = new Vector3(-0.5f, -0.25f, 0f);
        }#1#
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }

        UpdateGameTimer(__instance);
    }
}*/