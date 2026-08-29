namespace TownOfUs.Modules.Localization;

[Obsolete("Please use MiraAPI's MiraLocaleManager instead.")]
public static class TouLocale
{
    public static string Get(string name, string? defaultValue = null)
    {
        return MiraLocaleManager.Get(name, defaultValue ?? string.Empty);
    }

    public static string Get(SupportedLangs language, string name, string? defaultValue = null)
    {
        return MiraLocaleManager.Get((MiraLanguage)language, name, defaultValue ?? string.Empty);
    }
    public static string GetParsed(string name, string? defaultValue = null,
        Dictionary<string, string>? parseList = null)
    {
        return MiraLocaleManager.GetParsed(name, parseList ?? [], defaultValue ?? string.Empty);
    }

    public static string GetParsed(SupportedLangs language, string name, string? defaultValue = null,
        Dictionary<string, string>? parseList = null)
    {
        return MiraLocaleManager.GetParsed((MiraLanguage)language, name, parseList ?? [], defaultValue ?? string.Empty);
    }

    public static void LoadExternalLocale()
    {
        MiraLocaleManager.LoadExternalLocale();
    }
}