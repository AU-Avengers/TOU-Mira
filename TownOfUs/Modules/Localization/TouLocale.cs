using System.Reflection;
using System.Text.Json;
using BepInEx.Logging;
using Reactor.Localization;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Localization;

public static class TouLocale
{
    // C# classes for deserialization
    public class RootObject
    {
        public List<Dictionary<string, string>> RawTouNames { get; set; }
    }

    public static string LocaleDirectory => Path.Combine(Application.persistentDataPath, "TownOfUs", "Locales");

    public static string BepinexLocaleDirectory =>
        Path.Combine(BepInEx.Paths.BepInExRootPath, "MiraLocales", "TownOfUs");

    public static Dictionary<SupportedLangs, Dictionary<TouNames, string>> TouLocalization { get; } = [];
    public static int VanillaEnumAmounts;

    private static ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource("TouLocale");

    public static string Get(TouNames name, string? defaultValue = null)
    {
        if (TranslationController.InstanceExists)
        {
            var id = (int)name + VanillaEnumAmounts;
            return TranslationController.Instance.GetString((StringNames)id) ?? defaultValue ?? "STRMISS_" + name;
        }

        var currentLanguage =
            TranslationController.InstanceExists
                ? TranslationController.Instance.currentLanguage.languageID
                : SupportedLangs.English;

        if (!TouLocalization.TryGetValue(currentLanguage, out var translations) ||
            !translations.TryGetValue(name, out var translation))
        {
            return defaultValue ?? "STRMISS_" + name;
        }

        return translation;
    }

    public static void Initialize()
    {
        SearchInteralLocale();
        SearchDirectory(BepInEx.Paths.PluginPath);
        SearchDirectory(BepInEx.Paths.BepInExRootPath);
        SearchDirectory(BepinexLocaleDirectory);
        SearchDirectory(BepInEx.Paths.GameRootPath);
        SearchDirectory(LocaleDirectory);
        LocalizationManager.Register(new TouLocalizationProvider());
    }

    public static void SearchInteralLocale()
    {
        List<KeyValuePair<SupportedLangs, string>> list =
        [
            new(SupportedLangs.English, "en_US.json"),
            //new(SupportedLangs.German, "de_DE.json"),
            //new(SupportedLangs.Latam, "es_419.json"),
            new(SupportedLangs.Spanish, "es_ES.json"),
            //new(SupportedLangs.Filipino, "fil_PH.json"),
            //new(SupportedLangs.French, "fr_FR.json"),
            //new(SupportedLangs.Italian, "it_IT.json"),
            //new(SupportedLangs.Japanese, "ja_JP.json"),
            //new(SupportedLangs.Korean, "ko_KR.json"),
            //new(SupportedLangs.Dutch, "nl_NL.json"),
            //new(SupportedLangs.Brazilian, "pt_BR.json"),
            //new(SupportedLangs.Russian, "ru_RU.json"),
            //new(SupportedLangs.SChinese, "zh_CN.json"),
            //new(SupportedLangs.TChinese, "zh_TW.json")
        ];

        var assembly = Assembly.GetExecutingAssembly();
        foreach (var locale in list)
        {
            using var resourceStream =
                assembly.GetManifestResourceStream("TownOfUs.Resources.Locale." + locale.Value)
                ?? throw new InvalidOperationException(
                    $"Resource not found: TownOfUs.Resources.Locale.{locale.Value}");
            using StreamReader reader = new(resourceStream);
            using var jsonDocument = JsonDocument.Parse(reader.ReadToEnd());
            TouLocalization.TryAdd(locale.Key, []);
            ParseJsonFile(jsonDocument, locale.Key);
        }

    }

    public static void SearchDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Logger.LogWarning($"Directory does not exist: {directory}");
            return;
        }

        var translations = Directory.GetFiles(directory, "*.txt");
        foreach (var file in translations)
        {
            var localeName = Path.GetFileNameWithoutExtension(file);
            if (!Enum.TryParse<SupportedLangs>(localeName, out var language))
            {
                Logger.LogWarning($"Invalid locale name: {localeName}");
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

                if (!Enum.TryParse<TouNames>(key, out var touName))
                {
                    Logger.LogWarning("Invalid key value in translation: " + translation);
                }

                if (TouLocalization[language].ContainsKey(touName))
                {
                    var ogValuePair = TouLocalization[language].FirstOrDefault(x => x.Key == touName);
                    TouLocalization[language].Remove(ogValuePair.Key);
                }

                TouLocalization[language].TryAdd(touName, value);
            }
            else
            {
                Logger.LogWarning("Invalid translation format: " + translation);
            }
        }
    }

    public static void ParseJsonFile(JsonDocument json, SupportedLangs language)
    {
        JsonElement root = json.RootElement;

        if (root.TryGetProperty("TouNames", out JsonElement touNamesElement) &&
            touNamesElement.ValueKind == JsonValueKind.Array)
        {
            JsonElement touNamesObject = touNamesElement.EnumerateArray().FirstOrDefault();

            if (touNamesObject.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in touNamesObject.EnumerateObject())
                {
                    if (Enum.TryParse(property.Name, out TouNames touName))
                    {
                        string value = property.Value.GetString()!;

                        // Logger.LogWarning($"Found a match: Enum.{touName} with JSON value: {value}");

                        TouLocalization[language].TryAdd(touName, value);
                        if (language is SupportedLangs.English)
                        {
                            var stringName = CustomStringName.CreateAndRegister(touName.ToString());
                            if (VanillaEnumAmounts == 0)
                            {
                                VanillaEnumAmounts = (int)stringName;
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Invalid translation format: " + property.Name);
                    }
                }
            }
        }
        else
        {
            Logger.LogError("Could not find 'TouNames' array in JSON or it is not in the expected format.");
        }
    }
}