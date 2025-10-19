using HarmonyLib;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch]
public static class DleksMapOptionPickerPatches
{
    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.Initialize))]
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
