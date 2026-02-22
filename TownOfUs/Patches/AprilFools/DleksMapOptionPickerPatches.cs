using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch]
public static class DleksMapOptionPickerPatches
{
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

    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(typeof(MapSelectionGameSetting), nameof(MapSelectionGameSetting.GetValueString))]
    [HarmonyPrefix]
    public static void AddToActualOptions(MapSelectionGameSetting __instance)
    {
        if (__instance.Values.Count(x => x is StringNames.MapNameSkeld) != 2)
        {
            var list = __instance.Values.ToList();
            list.Insert((int)MapNames.Dleks, StringNames.MapNameSkeld);
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
