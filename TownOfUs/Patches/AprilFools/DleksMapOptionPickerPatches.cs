using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch]
public static class DleksMapOptionPickerPatches
{
    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.Initialize))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void LobbyOptionsDleksPatch(GameOptionsMapPicker __instance)
    {
        if (!__instance.AllMapIcons._items.Any(x => x.Name == MapNames.Dleks))
        {
            __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
            {
                Name = MapNames.Dleks,
                MapImage = TouAssets.DleksBanner.LoadAsset(),
                MapIcon = TouAssets.DleksIcon.LoadAsset(),
                NameImage = TouAssets.DleksText.LoadAsset(),
            });
        }
    }
    
    [HarmonyPatch(typeof(CreateGameMapPicker), nameof(CreateGameMapPicker.Initialize))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void CreateGameDleksPatch(CreateGameMapPicker __instance)
    {
        if (!__instance.AllMapIcons._items.Any(x => x.Name == MapNames.Dleks))
        {
            __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
            {
                Name = MapNames.Dleks,
                MapImage = TouAssets.DleksBanner.LoadAsset(),
                MapIcon = TouAssets.DleksIcon.LoadAsset(),
                NameImage = TouAssets.DleksText.LoadAsset(),
            });
        }
    }

    // This method is patched to fix issues on Epic Games
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

    /*[HarmonyPatch(typeof(CreateGameMapPicker), nameof(CreateGameMapPicker.SetupMapButtons))]
    [HarmonyPostfix]
    public static void SetupMapButtons(CreateGameMapPicker __instance, int maskLayer)
    {
        if (__instance.mapButtons != null)
        {
            for (int i = 0; i < __instance.mapButtons.Count; i++)
            {
                __instance.mapButtons[i].gameObject.Destroy();
            }
        }
        if (!__instance.AllMapIcons._items.Any(x => x.Name == MapNames.Dleks))
        {
            __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
            {
                Name = MapNames.Dleks,
                MapImage = TouAssets.DleksBanner.LoadAsset(),
                MapIcon = TouAssets.DleksIcon.LoadAsset(),
                NameImage = TouAssets.DleksText.LoadAsset(),
            });
        }
        __instance.mapButtons = new Il2CppSystem.Collections.Generic.List<MapSelectButton>();
        for (int j = 0; j < __instance.AllMapIcons.Count; j++)
        {
            MapIconByName instanceVal = __instance.AllMapIcons[j];
            MapSelectButton mapButton = UnityEngine.Object.Instantiate<MapSelectButton>(__instance.MapButtonOrigin, Vector3.zero, Quaternion.identity, __instance.transform);
            mapButton.SetImage(instanceVal.MapIcon, maskLayer);
            mapButton.transform.localPosition = new Vector3(__instance.StartPosX + (float)j * __instance.SpacingX, __instance.MapButtonY, -2f);
            mapButton.Button.ClickMask = __instance.ButtonClickMask;
            mapButton.MapID = (int)instanceVal.Name;
            mapButton.Button.OnClick.AddListener((UnityAction)(() => 
            {
                if (__instance.selectedButton)
                {
                    __instance.selectedButton.Button.SelectButton(false);
                }
                __instance.selectedButton = mapButton;
                __instance.selectedButton.Button.SelectButton(true);
                __instance.SelectMap(instanceVal);
            }));
            if (j > 0)
            {
                mapButton.Button.ControllerNav.selectOnLeft = __instance.mapButtons[j - 1].Button;
                __instance.mapButtons[j - 1].Button.ControllerNav.selectOnRight = mapButton.Button;
            }
            __instance.mapButtons.Add(mapButton);
            if (instanceVal.Name == (MapNames)__instance.selectedMapId)
            {
                mapButton.Button.SelectButton(true);
                __instance.SelectMap(__instance.selectedMapId);
                __instance.selectedButton = mapButton;
            }
        }
    }*/

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void GameManagerDleksPatch(GameStartManager __instance)
    {
        if (!__instance.AllMapIcons._items.Any(x => x.Name == MapNames.Dleks))
        {
            __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
            {
                Name = MapNames.Dleks,
                MapIcon = TouAssets.DleksTextAlt.LoadAsset(),
            });
        }
    }
}
