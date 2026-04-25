using HarmonyLib;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch]
public static class DleksMapOptionPickerPatches
{
    public static StringNames DleksName => CustomStringName.CreateAndRegister("dlekS");
    // TODO: localize this, probably by stealing from the sloth translations
    public static StringNames DleksTooltip => CustomStringName.CreateAndRegister(".sepor eht gninrael rof taerG .srodirroc dna smoor elpitlum htiw pihs dezis-muidem a :dlekS ehT");
    
    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SetupMapButtons))]
    [HarmonyPrefix]
    public static void AddToGameOptionsUI(GameOptionsMapPicker __instance)
    {
        if (__instance.AllMapIcons.ToArray().Any(x => x.Name == MapNames.Dleks))
        {
            return;
        }

        __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
        {
            Name = MapNames.Dleks,
            MapImage = TouAssets.DleksBanner.LoadAsset(),
            MapIcon = TouAssets.DleksIcon.LoadAsset(),
            NameImage = TouAssets.DleksText.LoadAsset(),
        });
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void GameManagerDleksPatch(GameStartManager __instance)
    {
        if (__instance.AllMapIcons.ToArray().Any(x => x.Name == MapNames.Dleks))
        {
            return;
        }

        __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
        {
            Name = MapNames.Dleks,
            MapIcon = TouAssets.DleksTextAlt.LoadAsset(),
        });
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPostfix]
    public static void GameManagerScalePatch(GameStartManager __instance)
    {
        var aspect = __instance.LobbyInfoPane.transform.GetChild(0);
        aspect.GetChild(1).localScale = new Vector3(1.0209f, 0.8609f, 1.0209f);
        aspect.GetChild(2).localPosition = new Vector3(-3.0475f, -1.4414f, -1f);
        aspect.GetChild(3).localPosition = new Vector3(-2.0085f, -2.17f, -2f);
        aspect.GetChild(4).gameObject.SetActive(false);
        var mapLogo = aspect.GetChild(5);
        mapLogo.localPosition = new Vector3(-1.5224f, -2.6892f, -2f);
        mapLogo.localScale = new Vector3(1.2074f, 1.2074f, 0.9799f);
        var gameSettings = aspect.GetChild(11);
        gameSettings.GetChild(0).gameObject.SetActive(false);
        gameSettings.GetChild(3).localPosition = new Vector3(0, 0.4f, 0);
        gameSettings.GetChild(4).localPosition = new Vector3(0, 0.4f, 0);
        var mapLabel = UnityEngine.Object.Instantiate(aspect.GetChild(8).gameObject, aspect);
        mapLabel.name = "MapLabel";
        mapLabel.transform.localPosition = new Vector3(-3.378f, -2.7074f, -2f);
        var labelBg = mapLabel.transform.GetChild(0);
        labelBg.localPosition = new Vector3(0.0215f, -0.0071f, 0);
        labelBg.localScale = new Vector3(0.7f, 1, 1);
        var labelText = mapLabel.transform.GetChild(1);
        labelText.GetComponent<TextTranslatorTMP>().Destroy();
        var tmpLabel = labelText.GetComponent<TextMeshPro>();
        tmpLabel.text = "Map";
    }

    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(typeof(MapSelectionGameSetting), nameof(MapSelectionGameSetting.GetValueString))]
    [HarmonyPrefix]
    public static void AddToActualOptions(MapSelectionGameSetting __instance)
    {
        if (__instance.Values.All(x => (int)x != (int)DleksName))
        {
            var list = __instance.Values.ToList();
            list.Insert((int)MapNames.Dleks, DleksName);
            __instance.Values = list.ToArray();
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.MapChanged))]
    [HarmonyPrefix]
    public static bool MapChangedPrefix(CreateGameOptions __instance, OptionBehaviour behaviour)
    {
        if (__instance.mapPicker.GetSelectedID() is (int)MapNames.Dleks)
        {
            __instance.mapBanner.flipX = false;
            __instance.rendererBGCrewmates.sprite = __instance.bgCrewmates[0];
            __instance.mapBanner.sprite = TouAssets.DleksText.LoadAsset();
            __instance.TurnOffCrewmates();
            __instance.currentCrewSprites = __instance.skeldCrewSprites;
            __instance.SetCrewmateGraphic(__instance.capacityOption.Value - 1f);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
    [HarmonyPrefix]
    public static void SetupMapBackground(CreateGameOptions __instance)
    {
        if (__instance.currentCrewSprites == null)
        {
            __instance.mapBanner.sprite = TouAssets.DleksText.LoadAsset();
        }
        __instance.currentCrewSprites ??= __instance.skeldCrewSprites;
        __instance.mapTooltips[3] = DleksTooltip;
    }

    // __instance method is patched to fix issues on Epic Games
    [HarmonyPatch(typeof(FreeplayPopover), nameof(FreeplayPopover.OnMapButtonPressed))]
    [HarmonyPrefix]
    public static bool ButtonPressPatch(FreeplayPopover __instance, FreeplayPopoverButton button)
    {
        __instance.background.GetComponent<PassiveButton>().OnClick
            .RemoveListener((UnityAction)(() => __instance.Close()));
        FreeplayPopoverButton[] array = __instance.buttons;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].Button.enabled = false;
        }

        AmongUsClient.Instance.TutorialMapId = (int)button.Map;
        __instance.hostGameButton.OnClick();
        return false;
    }

    private static FreeplayPopover _lastInstance;

    [HarmonyPatch(typeof(FreeplayPopover), nameof(FreeplayPopover.Show))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void AdjustFreeplayMenuPatch(FreeplayPopover __instance)
    {
        if (_lastInstance == __instance) return;
        _lastInstance = __instance;

        FreeplayPopoverButton fungleButton = __instance.buttons[4];
        FreeplayPopoverButton dleksButton = UnityEngine.Object.Instantiate(fungleButton, fungleButton.transform.parent);

        dleksButton.name = "DleksButton";
        dleksButton.map = MapNames.Dleks;
        dleksButton.GetComponent<SpriteRenderer>().sprite = TouAssets.DleksTextAlt.LoadAsset();
        dleksButton.OnPressEvent = fungleButton.OnPressEvent;

        dleksButton.transform.position = new Vector3(fungleButton.transform.position.x, __instance.buttons[0].transform.position.y + 0.7f, fungleButton.transform.position.z);

        __instance.buttons = new List<FreeplayPopoverButton>(__instance.buttons) { dleksButton }.ToArray();
    }
}