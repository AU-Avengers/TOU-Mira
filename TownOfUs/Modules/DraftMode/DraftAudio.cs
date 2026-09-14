using UnityEngine;

namespace TownOfUs.Modules.DraftMode;

public static class DraftAudio
{
    private static float _lastStartPlayedAt = -999f;
    private static float _lastYourTurnPlayedAt = -999f;
    private const float DebounceSeconds = 0.5f;

    private static DraftAudioCueMode GetConfiguredCueMode()
    {
        var instance = LocalSettingsTabSingleton<TouLocalTabPractice>.Instance;
            if (instance?.DraftAudioCue != null)
            {
                return instance.DraftAudioCue.Value;
            }
        try
        {
            return TouLocalTabPractice.CurrentDraftAudioCueMode;
        }
        catch (Exception)
        {
            return DraftAudioCueMode.None;
        }
    }

    public static void PlayDraftStart()
    {
        if (Time.time - _lastStartPlayedAt < DebounceSeconds) return;
        _lastStartPlayedAt = Time.time;

        var mode = GetConfiguredCueMode();
        if (mode == DraftAudioCueMode.Start || mode == DraftAudioCueMode.Both)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }

    public static void PlayYourTurn()
    {
        if (Time.time - _lastYourTurnPlayedAt < DebounceSeconds) return;
        _lastYourTurnPlayedAt = Time.time;

        var mode = GetConfiguredCueMode();
        if (mode == DraftAudioCueMode.YourTurn || mode == DraftAudioCueMode.Both)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }
}