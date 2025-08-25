using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace TownOfUs.Patches.Misc;

// code credit https://github.com/Yumenopai/TownOfHost_Y
[HarmonyPatch]
public class TouMiraModNews
{
    public TouMiraModNews(int Number, string Title, string SubTitle, string ShortTitle, string Text, string Date)
    {
        this.Number = Number;
        this.Title = Title;
        this.SubTitle = SubTitle;
        this.ShortTitle = ShortTitle;
        this.Text = Text;
        this.Date = Date;
    }

    public Announcement ToAnnouncement()
    {
        return new Announcement
        {
            Date = Date,
            Number = Number,
            ShortTitle = ShortTitle,
            SubTitle = SubTitle,
            Title = Title,
            Text = Text,
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Id = "TouMiraModNews"
        };
    }

    // ReSharper disable UnassignedField.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public string Date { get; set; }
    public int Number { get; set; }
    public string ShortTitle { get; set; }
    public string SubTitle { get; set; }
    public string Title { get; set; }

    public string Text { get; set; }
    // ReSharper restore UnassignedField.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}

public static class ModNewsFetcher
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private static string TouMiraModNewsURL =
        "https://raw.githubusercontent.com/AU-Avengers/TOU-Mira/refs/heads/main/TownOfUs/Resources/Announcements/modNews-";
#pragma warning restore S1075 // URIs should not be hardcoded

    private static bool downloaded;

    public static void CheckForNews()
    {
        Logger<TownOfUsPlugin>.Error($"Running Mod News Fetcher...");
        if (/*Environment.Is64BitProcess || */TownOfUsPlugin.IsDevBuild)
        {
            Logger<TownOfUsPlugin>.Error($"Loading News Locally, as this is a DEVELOPER BUILD");
            LoadTouMiraModNewsFromResources();
            return;
        }
        Coroutines.Start(FetchNews());
    }
    public static IEnumerator FetchNews()
    {
        if (downloaded)
        {
            yield break;
        }

        downloaded = true;
        /* TouMiraModNewsURL += TranslationController.Instance.currentLanguage.languageID switch
        {
            SupportedLangs.German => "de_DE.json",
            SupportedLangs.Latam => "es_419.json",
            SupportedLangs.Spanish => "es_ES.json",
            SupportedLangs.Filipino => "fil_PH.json",
            SupportedLangs.French => "fr_FR.json",
            SupportedLangs.Italian => "it_IT.json",
            SupportedLangs.Japanese => "ja_JP.json",
            SupportedLangs.Korean => "ko_KR.json",
            SupportedLangs.Dutch => "nl_NL.json",
            SupportedLangs.Brazilian => "pt_BR.json",
            SupportedLangs.Russian => "ru_RU.json",
            SupportedLangs.SChinese => "zh_CN.json",
            SupportedLangs.TChinese => "zh_TW.json",
            _ => "en_US.json", //English and any other unsupported language
        }; */
        TouMiraModNewsURL += "en_US.json";
        var request = UnityWebRequest.Get(TouMiraModNewsURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            downloaded = false;
            Logger<TownOfUsPlugin>.Error($"Couldn't fetch mod news from github: {request.error}");
            LoadTouMiraModNewsFromResources();
            yield break;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(request.downloadHandler.text);
            var newsArray = jsonDocument.RootElement.GetProperty("News");

            foreach (var newsElement in newsArray.EnumerateArray())
            {
                var dateString = newsElement.GetProperty("Date").GetString() != null
                    ? newsElement.GetProperty("Date").GetString()!
                    : "Unknown Date";
                var numberString = newsElement.GetProperty("Number").GetString();
                var number = numberString != null ? int.Parse(numberString, CultureInfo.InvariantCulture) : 0;
                var shortTitle = numberString != null && newsElement.GetProperty("ShortTitle").GetString() != null
                    ? newsElement.GetProperty("ShortTitle").GetString()!
                    : "No Short Title";
                var subTitle = numberString != null && newsElement.GetProperty("SubTitle").GetString() != null
                    ? newsElement.GetProperty("SubTitle").GetString()!
                    : "No Subtitle";
                var title = numberString != null && newsElement.GetProperty("Title").GetString() != null
                    ? newsElement.GetProperty("Title").GetString()!
                    : "No Title";
                var body = string.Join(" ",
                    newsElement.GetProperty("Text").EnumerateArray().Select(element => element.GetString()));
                // Create ModNews object
                var modNew = new TouMiraModNews(number, title, subTitle, shortTitle, body, dateString);
                ModNewsHistory.AllModNews = ModNewsHistory.AllModNews.Add(modNew);
            }
        }
        catch (Exception ex)
        {
            Logger<TownOfUsPlugin>.Error(
                $"Couldn't fetch mod news from github, loading from resources instead: {ex.Message}");
            // Use local Mod news instead
            LoadTouMiraModNewsFromResources();
        }
    }

    private static void LoadTouMiraModNewsFromResources()
    {
        /* string filename = TranslationController.Instance.currentLanguage.languageID switch
        {
            SupportedLangs.German => "de_DE.json",
            SupportedLangs.Latam => "es_419.json",
            SupportedLangs.Spanish => "es_ES.json",
            SupportedLangs.Filipino => "fil_PH.json",
            SupportedLangs.French => "fr_FR.json",
            SupportedLangs.Italian => "it_IT.json",
            SupportedLangs.Japanese => "ja_JP.json",
            SupportedLangs.Korean => "ko_KR.json",
            SupportedLangs.Dutch => "nl_NL.json",
            SupportedLangs.Brazilian => "pt_BR.json",
            SupportedLangs.Russian => "ru_RU.json",
            SupportedLangs.SChinese => "zh_CN.json",
            SupportedLangs.TChinese => "zh_TW.json",
            _ => "en_US.json", //English and any other unsupported language
        }; */

        var filename = "en_US.json";

        var assembly = Assembly.GetExecutingAssembly();
        using var resourceStream =
            assembly.GetManifestResourceStream("TownOfUs.Resources.Announcements.modNews-" + filename)
            ?? throw new InvalidOperationException(
                $"Resource not found: TownOfUs.Resources.Announcements.modNews-{filename}");
        using StreamReader reader = new(resourceStream);
        using var jsonDocument = JsonDocument.Parse(reader.ReadToEnd());
        var newsArray = jsonDocument.RootElement.GetProperty("News");

        foreach (var newsElement in newsArray.EnumerateArray())
        {
            var dateString = newsElement.GetProperty("Date").GetString() != null
                ? newsElement.GetProperty("Date").GetString()!
                : "Unknown Date";
            var numberString = newsElement.GetProperty("Number").GetString();
            var number = numberString != null ? int.Parse(numberString, CultureInfo.InvariantCulture) : 0;
            var shortTitle = numberString != null && newsElement.GetProperty("ShortTitle").GetString() != null
                ? newsElement.GetProperty("ShortTitle").GetString()!
                : "No Short Title";
            var subTitle = numberString != null && newsElement.GetProperty("SubTitle").GetString() != null
                ? newsElement.GetProperty("SubTitle").GetString()!
                : "No Subtitle";
            var title = numberString != null && newsElement.GetProperty("Title").GetString() != null
                ? newsElement.GetProperty("Title").GetString()!
                : "No Title";
            var body = string.Join(" ",
                newsElement.GetProperty("Text").EnumerateArray().Select(element => element.GetString()));
            // Create ModNews object
            var modNew = new TouMiraModNews(number, title, subTitle, shortTitle, body, dateString);
            ModNewsHistory.AllModNews = ModNewsHistory.AllModNews.Add(modNew);
        }
    }

    [HarmonyPatch]
    public static class ModNewsHistory
    {
        public static ImmutableList<TouMiraModNews> AllModNews = ImmutableList<TouMiraModNews>.Empty;

        [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
        [HarmonyPrefix]
        public static void SetModAnnouncements_Prefix(ref Il2CppReferenceArray<Announcement> aRange)
        {
            if (AllModNews.Count == 0)
            {
                Logger<TownOfUsPlugin>.Error($"No mod news were found.");
                return;
            }

            var finalAllNews = AllModNews.Select(n => n.ToAnnouncement()).ToList();
            finalAllNews.AddRange(aRange.Where(news => AllModNews.All(x => x.Number != news.Number)));
            finalAllNews.Sort((a1, a2) => DateTime.Compare(DateTime.Parse(a2.Date, CultureInfo.InvariantCulture),
                DateTime.Parse(a1.Date, CultureInfo.InvariantCulture)));

            aRange = new Il2CppReferenceArray<Announcement>(finalAllNews.Count);

            for (var i = 0; i < finalAllNews.Count; i++)
            {
                aRange[i] = finalAllNews[i];
            }
        }

        [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp))]
        [HarmonyPostfix]
        public static void SetUpPanel_Postfix(AnnouncementPanel __instance,
            [HarmonyArgument(0)] Announcement announcement)
        {
            if (announcement.Number < 100000)
            {
                return;
            }

            var obj = new GameObject("ModLabel");
            //obj.layer = -1;
            obj.transform.SetParent(__instance.transform);
            obj.transform.localPosition = new Vector3(-0.8f, 0.13f, 0.5f);
            obj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = TouAssets.AuAvengersSprite.LoadAsset();
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }
}