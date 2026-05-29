using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Events;
using TownOfUs.Patches.Misc;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch]
public static class AprilFoolsPatches
{
    public static int CurrentMode;
#pragma warning disable S1075 // URIs should not be hardcoded
    public static string DiscordServerUrl =
        "https://discord.gg/XtfYcAkfSR";
    public static string SourceCodeUrl =
        "https://github.com/AU-Avengers/TOU-Mira";
#pragma warning restore S1075 // URIs should not be hardcoded

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    public static void Prefix(MainMenuManager __instance)
    {
        if (TownOfUsEventHandlers.LogBuffer.Count != 0)
        {
            foreach (var log in TownOfUsEventHandlers.LogBuffer)
            {
                var text = log.Value;
                switch (log.Key)
                {
                    case TownOfUsEventHandlers.LogLevel.Error:
                        Error(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Warning:
                        Warning(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Debug:
                        Debug(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Info:
                        Info(text);
                        break;
                    case TownOfUsEventHandlers.LogLevel.Message:
                        Message(text);
                        break;
                }
            }

            TownOfUsEventHandlers.LogBuffer.Clear();
        }
        if (__instance.newsButton != null)
        {
            var legacytoggle = __instance.newsButton.CloneMenuItem("LegacyModeButton", new Vector2(0.815f, 0.775f), TouAssets.LegacyMenuSprite(TownOfUsPlugin.LegacyMode.Value).LoadAsset(), $"LegacyTheme{TownOfUsPlugin.LegacyMode.Value}", "Legacy Theming");

            var legacyHighlightObj = legacytoggle.transform.GetChild(1).gameObject;
            var legacyBaseObj = legacytoggle.transform.GetChild(2).gameObject;
            var legacySprite = legacyHighlightObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
            var legacySprite2 = legacyBaseObj.transform.GetChild(0).GetComponent<SpriteRenderer>();

            var legacyPassive = legacytoggle.GetComponent<PassiveButton>();
            legacyPassive.OnClick = new Button.ButtonClickedEvent();
            var translator = legacytoggle.transform.GetChild(0).GetChild(0).GetComponent<TmpMiraTranslator>();

            legacyPassive.OnClick.AddListener((Action)(() =>
            {
                var next = TownOfUsPlugin.LegacyMode.Value + 1;
                var value = !Enum.IsDefined(next) ? LegacyVisuals.Disabled : next;
                TownOfUsPlugin.LegacyMode.Value = value;
                legacySprite.sprite = TouAssets.LegacyMenuSprite(TownOfUsPlugin.LegacyMode.Value).LoadAsset();
                legacySprite2.sprite = TouAssets.LegacyMenuSprite(TownOfUsPlugin.LegacyMode.Value).LoadAsset();
                translator.stringName = $"LegacyTheme{TownOfUsPlugin.LegacyMode.Value}";
                translator.ResetText();
                LogoPatch.GameLogo.sprite = TouAssets.Banner.LoadAsset();
            }));
            /*var aprilfoolstoggle = __instance.newsButton.CloneMenuItem("AprilFoolsButton", new Vector2(0.815f, 0.775f), TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset(), "FoolsMode", "Fools Mode");

            var foolsHighlightObj = aprilfoolstoggle.transform.GetChild(1).gameObject;
            var foolsBaseObj = aprilfoolstoggle.transform.GetChild(2).gameObject;
            var foolsSprite = foolsHighlightObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
            var foolsSprite2 = foolsBaseObj.transform.GetChild(0).GetComponent<SpriteRenderer>();

            var foolsPassive = aprilfoolstoggle.GetComponent<PassiveButton>();
            foolsPassive.OnClick = new Button.ButtonClickedEvent();

            foolsPassive.OnClick.AddListener((Action)(() =>
            {
                var num = CurrentMode + 1;
                CurrentMode = num > 3 ? 0 : num;
                foolsSprite.sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
                foolsSprite2.sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
            }));*/

            var discordButton = __instance.newsButton.CloneMenuItem("DiscordJoinButton", new Vector2(0.815f, 0.69f), TouAssets.DiscordServer.LoadAsset(), "DiscordServer", "Discord Server");

            var discordPassive = discordButton.GetComponent<PassiveButton>();
            discordPassive.OnClick = new Button.ButtonClickedEvent();

            discordPassive.OnClick.AddListener((Action)(() =>
            {
                Constants.OpenURL(DiscordServerUrl);
            }));

            var githubButton = __instance.newsButton.CloneMenuItem("GithubCodeButton", new Vector2(0.815f, 0.605f), TouAssets.SourceCode.LoadAsset(), "SourceCode", "Source Code");

            var githubPassive = githubButton.GetComponent<PassiveButton>();
            githubPassive.OnClick = new Button.ButtonClickedEvent();

            githubPassive.OnClick.AddListener((Action)(() =>
            {
                Constants.OpenURL(SourceCodeUrl);
            }));

            var uiList = new Il2CppSystem.Collections.Generic.List<PassiveButton>();
            uiList.Add(__instance.playButton);
            uiList.Add(__instance.inventoryButton);
            uiList.Add(__instance.shopButton);
            uiList.Add(__instance.newsButton);
            // uiList.Add(aprilfoolstoggle);
            uiList.Add(discordPassive);
            uiList.Add(githubButton);

            foreach (var ogButton in __instance.mainButtons)
            {
                if (ogButton == __instance.playButton || ogButton == __instance.inventoryButton ||
                    ogButton == __instance.shopButton || ogButton == __instance.newsButton)
                {
                    continue;
                }

                uiList.Add(ogButton);
            }

            __instance.mainButtons = uiList;
            __instance.SetUpControllerNav();
        }
    }

    private static bool IsLegacyPlayer =>
        TownOfUsPlugin.LegacyMode.Value is LegacyVisuals.Players or LegacyVisuals.Full;
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetBodyType))]
    [HarmonyPrefix]
    public static void SetBodyPrefix(ref PlayerBodyTypes bodyType)
    {
        if (!IsLegacyPlayer)
        {
            return;
        }

        bodyType = PlayerBodyTypes.Classic;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.BodyType), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool BodyPrefix(ref PlayerBodyTypes __result)
    {
        if (IsLegacyPlayer)
        {
            __result = PlayerBodyTypes.Classic;
            return false;
        }
        return true;
    }

    public static PassiveButton CloneMenuItem(this PassiveButton newsButton, string objName, Vector2 pos, Sprite image, string localeKey, string? defaultText)
    {
        var obj = Object.Instantiate(newsButton,
            GameObject.Find("Main Buttons").transform.Find("BottomButtonBounds").transform);
        obj.name = objName;
        var positioner = obj.gameObject.AddComponent<AspectPosition>();
        positioner.Alignment = AspectPosition.EdgeAlignments.Center;
        positioner.anchorPoint = pos;
        positioner.updateAlways = true;

        obj.transform.GetChild(0).GetChild(0)
            .AddMiraTranslator(localeKey, false, defaultText);

        var highlightObj = obj.transform.GetChild(1).gameObject;
        var baseObj = obj.transform.GetChild(2).gameObject;
        highlightObj.GetComponent<SpriteRenderer>().sprite = TouAssets.MenuOptionActive.LoadAsset();
        highlightObj.transform.localScale = new Vector3(0.48f, 0.96f, 1f);
        baseObj.GetComponent<SpriteRenderer>().sprite = TouAssets.MenuOption.LoadAsset();
        baseObj.transform.localScale = new Vector3(0.48f, 0.96f, 1f);
        obj.GetComponent<BoxCollider2D>().size = new Vector2(1.9f, 0.5205f);
        obj.transform.GetChild(0).GetChild(0).transform.localPosition =
            new Vector3(-1.0159f, -0.0818f, 0f);
        obj.transform.GetChild(0).GetChild(0).GetComponent<AspectPosition>().anchorPoint =
            new Vector2(0.48f, 0.505f);
        obj.transform.GetChild(0).transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        var text = obj.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
        text.fontSize = 3f;
        text.fontSizeMin = 3f;
        text.fontSizeMax = 3f;
        var sprite = highlightObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        sprite.sprite = image;
        var sprite2 = baseObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        sprite2.sprite = image;
        highlightObj.transform.GetChild(0).transform.localScale = new Vector3(1.2404f, 0.62023f, 0.62023f);
        baseObj.transform.GetChild(0).transform.localScale = new Vector3(1.2404f, 0.62023f, 0.62023f);
        obj.GetComponent<NewsCountButton>().DestroyImmediate();
        obj.transform.GetChild(3).gameObject.DestroyImmediate();
        return obj;
    }
}