using UnityEngine;

namespace TownOfUs.Modules.DraftMode;

public static class DraftAudio
{
    public static void PlayDraftStart()
    {
        if (LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.DraftAudioCue.Value == DraftAudioCueMode.Start)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }

    public static void PlayYourTurn()
    {
        if (LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.DraftAudioCue.Value == DraftAudioCueMode.YourTurn)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }
}
