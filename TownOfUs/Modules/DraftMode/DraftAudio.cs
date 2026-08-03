namespace TownOfUs.Modules.DraftMode;

public static class DraftAudio
{
    public static void PlayDraftStart()
    {
        if ((LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.DraftAudioCue.Value == DraftAudioCueMode.Start) || (LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.DraftAudioCue.Value == DraftAudioCueMode.Both))
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }

    public static void PlayYourTurn()
    {
        if (LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.DraftAudioCue.Value == DraftAudioCueMode.YourTurn || LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.DraftAudioCue.Value == DraftAudioCueMode.Both)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }
}
