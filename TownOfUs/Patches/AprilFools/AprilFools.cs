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

    private static readonly Dictionary<int, string> Modes = new()
    {
        { 0, "Off" },
        { 1, "Horse" },
        { 2, "Long" },
        { 3, "Long Horse" }
    };

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    public static void Prefix(MainMenuManager __instance)
    {
        if (__instance.newsButton != null)
        {
            var aprilfoolstoggle = Object.Instantiate(__instance.newsButton, null);
            aprilfoolstoggle.name = "aprilfoolstoggle";

            aprilfoolstoggle.transform.localScale = new Vector3(0.44f, 0.84f, 1f);

            var passive = aprilfoolstoggle.GetComponent<PassiveButton>();
            passive.OnClick = new Button.ButtonClickedEvent();

            aprilfoolstoggle.gameObject.transform.SetParent(GameObject.Find("RightPanel").transform);
            var pos = aprilfoolstoggle.gameObject.AddComponent<AspectPosition>();
            pos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
            pos.DistanceFromEdge = new Vector3(2.1f, 2f, 8f);

            passive.OnClick.AddListener((Action)(() =>
            {
                var num = CurrentMode + 1;
                CurrentMode = num > 3 ? 0 : num;
                var text = aprilfoolstoggle.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                text.text = $"April fools mode: {Modes[CurrentMode]}";
            }));

            var text = aprilfoolstoggle.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p =>
            {
                text.text = $"April fools mode: {Modes[CurrentMode]}";
                pos.AdjustPosition();
            })));

            aprilfoolstoggle.transform.GetChild(0).transform.localScale =
                new Vector3(aprilfoolstoggle.transform.localScale.x + 1, 1f, 1f);
            aprilfoolstoggle.transform.GetChild(0).transform.localPosition -= new Vector3(1.5f, 0f, 0f);
            aprilfoolstoggle.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            aprilfoolstoggle.transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            aprilfoolstoggle.GetComponent<NewsCountButton>().DestroyImmediate();
            aprilfoolstoggle.transform.GetChild(3).gameObject.DestroyImmediate();
        }
    }

    [HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldLongAround))]
    [HarmonyPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = CurrentMode == 2;
        return true;
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