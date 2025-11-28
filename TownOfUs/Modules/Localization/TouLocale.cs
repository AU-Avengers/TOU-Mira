using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using BepInEx.Logging;
using MiraAPI.Utilities;
using Reactor.Localization;
using TownOfUs.LocalSettings.SettingTypes;
using UnityEngine;

namespace TownOfUs.Modules.Localization;

public static class TouLocale
{
    public static string LocaleDirectory => Path.Combine(Application.persistentDataPath, "TownOfUs", "Locales");


    public static Dictionary<ExtendedLangs, string> LangList { get; } = new()
    {
        { ExtendedLangs.English, "en_US.xml" },
        { ExtendedLangs.Latam, "es_419.xml" },
        { ExtendedLangs.Brazilian, "pt_BR.xml" },
        { ExtendedLangs.Portuguese, "pt_PT.xml" },
        { ExtendedLangs.Korean, "ko_KR.xml" },
        { ExtendedLangs.Russian, "ru_RU.xml" },
        { ExtendedLangs.Dutch, "nl_NL.xml" },
        { ExtendedLangs.Filipino, "fil_PH.xml" },
        { ExtendedLangs.French, "fr_FR.xml" },
        { ExtendedLangs.German, "de_DE.xml" },
        { ExtendedLangs.Italian, "it_IT.xml" },
        { ExtendedLangs.Japanese, "ja_JP.xml" },
        { ExtendedLangs.Spanish, "es_ES.xml" },
        { ExtendedLangs.SChinese, "zh_CN.xml" },
        { ExtendedLangs.TChinese, "zh_TW.xml" },
        { ExtendedLangs.Irish, "ga_IE.xml" },
        { ExtendedLangs.Polish, "pl_PL.xml" }, // Custom
        { ExtendedLangs.Turkish, "tr_TR.xml" }, // Custom
        { ExtendedLangs.Swedish, "sv_SE.xml" }, // Custom
        { ExtendedLangs.Lithuanian, "lt_LT.xml" }, // Custom
    };
    public static Dictionary<ExtendedLangs, string> LangCultureList { get; } = new()
    {
        { ExtendedLangs.English, "en-US" },
        { ExtendedLangs.Latam, "es-419" },
        { ExtendedLangs.Brazilian, "pt-BR" },
        { ExtendedLangs.Portuguese, "pt-PT" },
        { ExtendedLangs.Korean, "ko-KR" },
        { ExtendedLangs.Russian, "ru-RU" },
        { ExtendedLangs.Dutch, "nl-NL" },
        { ExtendedLangs.Filipino, "fil-PH" },
        { ExtendedLangs.French, "fr-FR" },
        { ExtendedLangs.German, "de-DE" },
        { ExtendedLangs.Italian, "it-IT" },
        { ExtendedLangs.Japanese, "ja-JP" },
        { ExtendedLangs.Spanish, "es-ES" },
        { ExtendedLangs.SChinese, "zh-CN" },
        { ExtendedLangs.TChinese, "zh-TW" },
        { ExtendedLangs.Irish, "ga-IE" },
        { ExtendedLangs.Polish, "pl-PL" }, // Custom
        { ExtendedLangs.Turkish, "tr-TR" }, // Custom
        { ExtendedLangs.Swedish, "sv-SE" }, // Custom
        { ExtendedLangs.Lithuanian, "lt-LT" }, // Custom
    };

    public static string BepinexLocaleDirectory =>
        Path.Combine(BepInEx.Paths.BepInExRootPath, "MiraLocales", "TownOfUs");

    /*public static Dictionary<string, StringNames> TouLocaleList { get; } = [];*/

    public static Dictionary<string, string> TmpTextList { get; } = new()
    {
        { "<nl>", "\n" },
        { "<and>", "&" },
    };

    // Language, Xml Name, then Value
    public static Dictionary<SupportedLangs, Dictionary<string, string>> TouLocalization { get; } = [];
    public static Dictionary<ToggleButtonBehaviour, string> LocalizedToggles { get; } = [];
    public static Dictionary<LocalizedLocalSliderSetting, string> LocalizedSliders { get; } = [];

    internal static ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource("TouLocale");

    public static string Get(string name, string? defaultValue = null)
    {
        var currentLanguage =
            TranslationController.InstanceExists
                ? TranslationController.Instance.currentLanguage.languageID
                : SupportedLangs.English;
        return Get(currentLanguage, name, defaultValue);
    }

    public static string Get(SupportedLangs language, string name, string? defaultValue = null)
    {
        if (TouLocalization.TryGetValue(language, out var translations) &&
            translations.TryGetValue(name, out var translation))
        {
            return translation;
        }

        if (TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
            translationsEng.TryGetValue(name, out var translationEng))
        {
            return translationEng;
        }

        return defaultValue ?? "STRMISS_" + name;
    }
    public static string GetParsed(string name, string? defaultValue = null,
        Dictionary<string, string>? parseList = null)
    {
        var currentLanguage =
            TranslationController.InstanceExists
                ? TranslationController.Instance.currentLanguage.languageID
                : SupportedLangs.English;
        return GetParsed(currentLanguage, name, defaultValue, parseList);
    }

    public static string GetParsed(SupportedLangs language, string name, string? defaultValue = null,
        Dictionary<string, string>? parseList = null)
    {
        var text = defaultValue ?? "STRMISS_" + name;

        if (TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
            translationsEng.TryGetValue(name, out var translationEng))
        {
            text = translationEng;
        }

        if (language is not SupportedLangs.English && TouLocalization.TryGetValue(language, out var translations) &&
            translations.TryGetValue(name, out var translation))
        {
            text = translation;
        }

        text = Regex.Replace(text, @"\%([^%]+)\%", @"<$1>");
        if (text.Contains("\\<"))
        {
            text = text.Replace("\\<", "<");
        }

        if (text.Contains("\\>"))
        {
            text = text.Replace("\\>", ">");
        }

        foreach (var tmpText in TmpTextList.Where(x => text.Contains(x.Key)))
        {
            text = text.Replace(tmpText.Key, tmpText.Value);
        }

        if (parseList != null)
        {
            foreach (var tmpText in parseList.Where(x => text.Contains(x.Key)))
            {
                text = text.Replace(tmpText.Key, tmpText.Value);
            }
        }

        return text;
    }

    public static void Initialize()
    {
        LocalizationManager.Register(new TouLocalizationProvider());
        SearchInternalLocale();
    }

    public static void LoadExternalLocale()
    {
        SearchDirectory(BepInEx.Paths.PluginPath);
        SearchDirectory(BepInEx.Paths.BepInExRootPath);
        SearchDirectory(BepinexLocaleDirectory);
        SearchDirectory(BepInEx.Paths.GameRootPath);
        SearchDirectory(LocaleDirectory);
    }

    public static void SearchInternalLocale()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var locale in LangList)
        {
            using var resourceStream =
                assembly.GetManifestResourceStream("TownOfUs.Resources.Locale." + locale.Value);
            if (resourceStream == null)
            {
                Logger.LogError($"Language is not added: {locale.Key.ToDisplayString()}");
                continue;
            }

            Logger.LogWarning($"Language is being added: {locale.Key.ToDisplayString()}");
            using StreamReader reader = new(resourceStream);
            string xmlContent = reader.ReadToEnd();

            TouLocalization.TryAdd((SupportedLangs)locale.Key, []);
            ParseXmlFile(xmlContent, (SupportedLangs)locale.Key);
        }
    }

    public static void SearchDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Logger.LogError($"Directory does not exist: {directory}");
            return;
        }

        var xmlTranslations = Directory.GetFiles(directory, "*.xml");
        foreach (var file in xmlTranslations)
        {
            var localeName = Path.GetFileNameWithoutExtension(file);
            if (!LangList.ContainsValue(localeName + ".xml"))
            {
                Logger.LogError($"Invalid locale iso name: {localeName}");
                continue;
            }

            Logger.LogWarning($"Adding locale for: {localeName} in {file}");

            var language = LangList.FirstOrDefault(x => x.Value == localeName + ".xml").Key;
            TouLocalization.TryAdd((SupportedLangs)language, []);
            var xmlContent = File.ReadAllText(file);
            ParseXmlFile(xmlContent, (SupportedLangs)language);
        }

        var translations = Directory.GetFiles(directory, "*.txt");
        foreach (var file in translations)
        {
            var localeName = Path.GetFileNameWithoutExtension(file);
            if (!Enum.TryParse<ExtendedLangs>(localeName, out var language))
            {
                Logger.LogError($"Invalid locale name: {localeName}");
                continue;
            }

            TouLocalization.TryAdd((SupportedLangs)language, []);
            ParseFile(file, (SupportedLangs)language);
        }
    }

    public static void ParseFile(string file, SupportedLangs language)
    {
        foreach (var translation in File.ReadAllLines(file))
        {
            var parts = translation.Split('=');
            if (parts.Length >= 2)
            {
                var key = parts[0];
                var value = string.Join("=", parts.Skip(1));

                if (TouLocalization[language].ContainsKey(key))
                {
                    var ogValuePair = TouLocalization[language].FirstOrDefault(x => x.Key == key);
                    TouLocalization[language].Remove(ogValuePair.Key);
                }

                TouLocalization[language].TryAdd(key, value);
            }
            else
            {
                Logger.LogWarning("Invalid translation format: " + translation);
            }
        }
    }

    public static void ParseXmlFile(string xmlContent, SupportedLangs language)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.LoadXml(xmlContent);
            XmlNodeList? stringNodes = xmlDoc.SelectNodes("/resources/string");

            if (stringNodes != null)
            {
                Logger.LogWarning($"{stringNodes.Count} XML Nodes found!");
                foreach (XmlNode node in stringNodes)
                {
                    if (node.Attributes?["name"] != null)
                    {
                        string name = node.Attributes["name"]!.Value;
                        string value = node.InnerText;

                        if (TouLocalization[language].ContainsKey(name))
                        {
                            var ogValuePair = TouLocalization[language].FirstOrDefault(x => x.Key == name);
                            TouLocalization[language].Remove(ogValuePair.Key);
                        }

                        TouLocalization[language].TryAdd(name, value);

                        /*if (language is SupportedLangs.English && !TouLocaleList.ContainsKey(name))
                        {
                            var stringName = CustomStringName.CreateAndRegister(name);
                            TouLocaleList.TryAdd(name, stringName);
                        }*/
                    }
                }

                var customLang = (ExtendedLangs)language;

                Logger.LogWarning(
                    $"{TouLocalization[language].Count} Localization strings added to {customLang.ToDisplayString()}!");
            }
            else
            {
                Logger.LogError($"XML nodes were not found in {xmlContent}.");
            }
        }
        catch (XmlException ex)
        {
            Logger.LogError($"XML parsing error: {ex.Message}");
        }
    }
}

public enum ExtendedLangs
{
    English,
    Latam,
    Brazilian,
    Portuguese,
    Korean,
    Russian,
    Dutch,
    Filipino,
    French,
    German,
    Italian,
    Japanese,
    Spanish,
    SChinese,
    TChinese,
    Irish,
    Polish,
    Turkish,
    Swedish,
    Lithuanian,
}