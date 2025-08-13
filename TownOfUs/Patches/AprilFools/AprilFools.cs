using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch]
public static class AprilFoolsPatches
{
    public static int CurrentMode;
    public static BoxCollider2D? newsCollider;

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    public static void Postfix(MainMenuManager __instance)
    {
        if (__instance.newsButton != null)
        {
            if (newsCollider == null || newsCollider.gameObject != __instance.newsButton.gameObject)
            {
                newsCollider = __instance.newsButton.GetComponent<BoxCollider2D>();
            }
            
            if (newsCollider) newsCollider.size = new Vector2(1.8f, 0.4705f);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    public static void Prefix(MainMenuManager __instance)
    {
        if (__instance.newsButton != null)
        {
            var aprilfoolstoggle = Object.Instantiate(__instance.newsButton, null);
            aprilfoolstoggle.name = "AprilFoolsButton";

            var highlightObj = aprilfoolstoggle.transform.GetChild(1).gameObject;
            var baseObj = aprilfoolstoggle.transform.GetChild(2).gameObject;
            highlightObj.GetComponent<SpriteRenderer>().sprite = TouAssets.MenuOptionActive.LoadAsset();
            highlightObj.transform.localScale = new Vector3(0.42f, 0.84f, 1f);
            baseObj.GetComponent<SpriteRenderer>().sprite = TouAssets.MenuOption.LoadAsset();
            baseObj.transform.localScale = new Vector3(0.42f, 0.84f, 1f);
            aprilfoolstoggle.GetComponent<BoxCollider2D>().size = new Vector2(1.8f, 0.4705f);

            var passive = aprilfoolstoggle.GetComponent<PassiveButton>();
            passive.OnClick = new Button.ButtonClickedEvent();

            aprilfoolstoggle.gameObject.transform.SetParent(GameObject.Find("Main Buttons").transform);
            
            /*var pos = aprilfoolstoggle.gameObject.AddComponent<AspectPosition>();
            pos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
            pos.DistanceFromEdge = new Vector3(2.1f, 2f, 8f);*/
            highlightObj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
            baseObj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
            highlightObj.transform.GetChild(0).transform.localScale = new Vector3(1.0854f, 0.5427f, 0.5427f);
            baseObj.transform.GetChild(0).transform.localScale = new Vector3(1.0854f, 0.5427f, 0.5427f);

            aprilfoolstoggle.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(-1.0159f, -0.0818f, 0f);
            var translator = aprilfoolstoggle.transform.GetChild(0).GetChild(0).GetComponent<TextTranslatorTMP>();
            aprilfoolstoggle.transform.GetChild(0).GetChild(0).GetComponent<AspectPosition>().anchorPoint = new Vector2(0.48f, 0.505f);
            var id = (int)TouNames.FoolsMode + TouLocale.VanillaEnumAmounts;
            translator.TargetText = (StringNames)id;
            translator.defaultStr = TouLocale.Get(TouNames.FoolsMode);
            var text = aprilfoolstoggle.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
            text.text = TouLocale.Get(TouNames.FoolsMode);
            text.fontSize = 3f;
            text.fontSizeMin = 3f;
            text.fontSizeMax = 3f;
            var sprite = highlightObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite.sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
            var sprite2 = baseObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
            sprite2.sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
            // pos.AdjustPosition();


            var realNews = GameObject.Find("NewsButton")!;
            var highlightObj2 = realNews.transform.GetChild(1).gameObject;
            var baseObj2 = realNews.transform.GetChild(2).gameObject;
            highlightObj2.GetComponent<SpriteRenderer>().sprite = TouAssets.MenuOptionActive.LoadAsset();
            highlightObj2.transform.localScale = new Vector3(0.42f, 0.84f, 1f);
            baseObj2.GetComponent<SpriteRenderer>().sprite = TouAssets.MenuOption.LoadAsset();
            baseObj2.transform.localScale = new Vector3(0.42f, 0.84f, 1f);
                
            highlightObj2.transform.GetChild(0).transform.localScale = new Vector3(1.0854f, 0.5427f, 0.5427f);
            baseObj2.transform.GetChild(0).transform.localScale = new Vector3(1.0854f, 0.5427f, 0.5427f);
            
            aprilfoolstoggle.transform.localPosition = new Vector3(-0.9f, -0.387f, 0f);
            realNews.transform.localPosition = new Vector3(0.9f, -0.387f, 0f);
            realNews.transform.localScale = new Vector3(1f, 1f, 1f);
            realNews.GetComponent<BoxCollider2D>().size = new Vector2(1.8f, 0.4705f);
            realNews.transform.GetChild(3).GetChild(0).localPosition = new Vector3(-1f, 0f, 0f);
            realNews.transform.GetChild(0).GetChild(0).GetComponent<AspectPosition>().anchorPoint = new Vector2(0.48f, 0.505f);
            
            passive.OnClick.AddListener((Action)(() =>
            {
                var num = CurrentMode + 1;
                CurrentMode = num > 3 ? 0 : num;
                text.text = TouLocale.Get(TouNames.FoolsMode);
                sprite.sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
                sprite2.sprite = TouAssets.FoolsMenuSprite(CurrentMode).LoadAsset();
            }));
            aprilfoolstoggle.GetComponent<NewsCountButton>().DestroyImmediate();
            aprilfoolstoggle.transform.GetChild(3).gameObject.DestroyImmediate();
            
            var text2 = realNews.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
            text2.fontSize = 3f;
            text2.fontSizeMin = 3f;
            text2.fontSizeMax = 3f;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetBodyType))]
    [HarmonyPrefix]
    public static void Prefix(ref PlayerBodyTypes bodyType)
    {
        switch (CurrentMode)
        {
            case 1:
                bodyType = PlayerBodyTypes.Horse;
                break;
            case 2:
                bodyType = PlayerBodyTypes.Long;
                break;
            case 3:
                bodyType = PlayerBodyTypes.LongSeeker;
                break;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.BodyType), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool Prefix2(ref PlayerBodyTypes __result)
    {
        switch (CurrentMode)
        {
            case 1:
                __result = PlayerBodyTypes.Horse;
                return false;
            case 2:
                __result = PlayerBodyTypes.Long;
                return false;
            case 3:
                __result = PlayerBodyTypes.LongSeeker;
                return false;
            default:
                return true;
        }
    }
}
