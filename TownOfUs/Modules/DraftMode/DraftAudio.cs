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

        return TouLocalTabPractice.CurrentDraftAudioCueMode;
    }

    public static void PlayDraftStart() => PlayCue(DraftAudioCueMode.Start, ref _lastStartPlayedAt);

    public static void PlayYourTurn() => PlayCue(DraftAudioCueMode.YourTurn, ref _lastYourTurnPlayedAt);

    private static void PlayCue(DraftAudioCueMode cue, ref float lastPlayedAt)
    {
        if (Time.time - lastPlayedAt < DebounceSeconds) return;
        lastPlayedAt = Time.time;

        var mode = GetConfiguredCueMode();
        if (mode == cue || mode == DraftAudioCueMode.Both)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }
}