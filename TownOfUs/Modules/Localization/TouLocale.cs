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

    public static Dictionary<SupportedLangs, string> LangList { get; } = new()
    {
        { SupportedLangs.English, "en_US.xml" },
        { SupportedLangs.Latam, "es_419.xml" },
        { SupportedLangs.Brazilian, "pt_BR.xml" },
        { SupportedLangs.Portuguese, "pt_PT.xml" },
        { SupportedLangs.Korean, "ko_KR.xml" },
        { SupportedLangs.Russian, "ru_RU.xml" },
        { SupportedLangs.Dutch, "nl_NL.xml" },
        { SupportedLangs.Filipino, "fil_PH.xml" },
        { SupportedLangs.French, "fr_FR.xml" },
        { SupportedLangs.German, "de_DE.xml" },
        { SupportedLangs.Italian, "it_IT.xml" },
        { SupportedLangs.Japanese, "ja_JP.xml" },
        { SupportedLangs.Spanish, "es_ES.xml" },
        { SupportedLangs.SChinese, "zh_CN.xml" },
        { SupportedLangs.TChinese, "zh_TW.xml" },
        { SupportedLangs.Irish, "ga_IE.xml" },
        { (SupportedLangs)16, "pl_PL.xml" } // Polish
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

        if (TouLocalization.TryGetValue(currentLanguage, out var translations) &&
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
    public static string GetParsed(string name, string? defaultValue = null, Dictionary<string, string>? parseList = null)
    {
        var text = defaultValue ?? "STRMISS_" + name;

        var currentLanguage =
            TranslationController.InstanceExists
                ? TranslationController.Instance.currentLanguage.languageID
                : SupportedLangs.English;
        
        if (TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
            translationsEng.TryGetValue(name, out var translationEng))
        {
            text = translationEng;
        }
        
        if (TouLocalization.TryGetValue(currentLanguage, out var translations) &&
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
            
            TouLocalization.TryAdd(locale.Key, []);
            ParseXmlFile(xmlContent, locale.Key);
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
            TouLocalization.TryAdd(language, []);
            var xmlContent = File.ReadAllText(file);
            ParseXmlFile(xmlContent, language);
        }
        
        var translations = Directory.GetFiles(directory, "*.txt");
        foreach (var file in translations)
        {
            var localeName = Path.GetFileNameWithoutExtension(file);
            if (!Enum.TryParse<SupportedLangs>(localeName, out var language))
            {
                Logger.LogError($"Invalid locale name: {localeName}");
                continue;
            }

            TouLocalization.TryAdd(language, []);
            ParseFile(file, language);
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
                Logger.LogWarning($"{TouLocalization[language].Count} Localization strings added to {language.ToDisplayString()}!");
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