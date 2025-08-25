using Reactor.Localization;

namespace TownOfUs.Modules.Localization;

public class TouLocalizationProvider : LocalizationProvider
{
    public override bool TryGetText(StringNames stringName, out string? result)
    {
        var id = (int)stringName - TouLocale.VanillaEnumAmounts;
        if (Enum.IsDefined(typeof(TouNames), id) && CurrentLanguage != null)
        {
            var touName = (TouNames)id;
            
            if (TouLocale.TouLocalization.TryGetValue((SupportedLangs)CurrentLanguage, out var translations) &&
                translations.TryGetValue(touName, out var translation))
            {
                result = translation;
                return true;
            }
            else if (TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
                     translationsEng.TryGetValue(touName, out var englishDefault))
            {
                result = englishDefault;
                return true;
            }

            result = "STRMISS_" + touName.ToString();
            return true;
        }
        
        result = null;
        return false;
    }
}