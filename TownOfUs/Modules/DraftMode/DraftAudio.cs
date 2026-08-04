namespace TownOfUs.Modules.DraftMode;

public static class DraftAudio
{
    private static DraftAudioCueMode GetConfiguredCueMode()
    {
        try
        {
            return TouLocalTabPractice.CurrentDraftAudioCueMode;
        }
        catch (Exception)
        {
            try
            {
                return LocalSettingsTabSingleton<TouLocalTabPractice>.Instance?.DraftAudioCue?.Value ?? DraftAudioCueMode.None;
            }
            catch (Exception)
            {
                return DraftAudioCueMode.None;
            }
        }
    }

    public static void PlayDraftStart()
    {
        var mode = GetConfiguredCueMode();
        if (mode == DraftAudioCueMode.Start || mode == DraftAudioCueMode.Both)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }

    public static void PlayYourTurn()
    {
        var mode = GetConfiguredCueMode();
        if (mode == DraftAudioCueMode.YourTurn || mode == DraftAudioCueMode.Both)
        {
            TouAudio.PlaySound(TouAudio.TribunalSound);
        }
    }
}
