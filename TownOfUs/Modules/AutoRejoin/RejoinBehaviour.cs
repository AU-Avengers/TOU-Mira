
using InnerNet;
using Reactor.Utilities.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules.AutoRejoin;

[RegisterInIl2Cpp]
public class RejoinBehaviour(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public static bool            PendingRejoin;
    public static int             SavedGameId = -1;
    public static EndGameManager? CurrentEndGameManager;
    public static string          ScreenText = "";
    public static CountdownGui?   GuiObject;

    public static RejoinBehaviour? Instance;

    private bool _running;
    private float _timer;
    private int _lastShown = -1;
    private string _gameCode = "";

    public void StartRejoin()
    {
        _gameCode = GameCode.IntToGameName(SavedGameId);
        _timer = LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.AutoRejoinDelay.Value;
        _lastShown = -1;
        _running = true;
        Info($"[AutoRejoin] Countdown started ({_timer}s) for: {_gameCode}");
    }

    public void Cancel()
    {
        _running = false;
        ScreenText = "";
    }

    private void Update()
    {
        if (!_running) return;

        _timer -= Time.unscaledDeltaTime;
        int secs = Mathf.Max(1, Mathf.CeilToInt(_timer));

        if (secs != _lastShown)
        {
            _lastShown = secs;
            ScreenText = $"[AutoRejoin]  Rejoining in {secs}s  ({_gameCode})";
            Info(ScreenText);
        }

        if (_timer <= 0f)
        {
            _running = false;
            ScreenText = "";
            ClickOnce();
        }
    }

    private static void ClickOnce()
    {
        Info("[AutoRejoin] Looking for PlayAgainButton...");

        var buttons = Object.FindObjectsOfType<PassiveButton>();
        foreach (var btn in buttons)
        {
            if (btn.gameObject.name == "PlayAgainButton")
            {
                Info("[AutoRejoin] Clicking PlayAgainButton.");
                try
                {
                    btn.OnClick.Invoke();
                    Info("[AutoRejoin] Clicked! Waiting for host...");
                }
                catch (System.Exception ex)
                {
                    Warning($"[AutoRejoin] Click failed: {ex.Message}");
                    // Fallback
                    try
                    {
                        btn.ReceiveClickDown();
                        btn.ReceiveClickUp();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return;
            }
        }

        Warning("[AutoRejoin] PlayAgainButton not found after countdown.");
    }

    public static void TriggerRejoin()
    {
        if (Instance == null)
        {
            try
            {
                var go = new GameObject("AutoRejoinBehaviour");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<RejoinBehaviour>();
                Info("[AutoRejoin] RejoinBehaviour created.");
            }
            catch (System.Exception ex)
            {
                Error($"[AutoRejoin] Failed to create RejoinBehaviour: {ex.Message}");
                return;
            }
        }
        Instance.StartRejoin();
    }

    public static void CancelRejoin()
    {
        Instance?.Cancel();
        PendingRejoin         = false;
        SavedGameId           = -1;
        CurrentEndGameManager = null;
        ScreenText            = "";
    }
}
