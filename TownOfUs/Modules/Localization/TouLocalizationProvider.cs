using Reactor.Localization;

namespace TownOfUs.Modules.Localization;

public class TouLocalizationProvider : LocalizationProvider
{
    internal static List<IMiraTranslation> ActiveTexts = [];
    private static bool _loadedStrings;

    public override void OnLanguageChanged(SupportedLangs newLanguage)
    {
        if (!_loadedStrings)
        {
            TouLocale.LoadExternalLocale();
            _loadedStrings = true;
        }

        for (int i = 0; i < ActiveTexts.Count; i++)
        {
            ActiveTexts[i].ResetText();
        }
    }
}