using Reactor.Localization;
using Reactor.Utilities;

namespace TownOfUs.Modules.Localization;

public class TouLocalizationProvider : LocalizationProvider
{
    internal static List<IMiraTranslation> ActiveTexts = [];
    public override int Priority => ReactorPriority.Normal;

    public override void OnLanguageChanged(SupportedLangs newLanguage)
    {
        for (int i = 0; i < ActiveTexts.Count; i++)
        {
            ActiveTexts[i].ResetText();
        }
    }
}