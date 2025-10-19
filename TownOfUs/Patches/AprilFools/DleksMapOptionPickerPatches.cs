using HarmonyLib;

namespace TownOfUs.Patches.AprilFools;

[HarmonyPatch]
public static class DleksMapOptionPickerPatches
{
    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.Initialize))]
    [HarmonyPrefix]
    public static void AddToGameOptionsUI(GameOptionsMapPicker __instance)
    {
        __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
        {
            Name = MapNames.Dleks,
            MapImage = TouAssets.DleksBanner.LoadAsset(),
            MapIcon = TouAssets.DleksIcon.LoadAsset(),
            NameImage = TouAssets.DleksText.LoadAsset(),
        });
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPrefix]
    public static void AddToOptionsDisplay(GameStartManager __instance)
    {
        __instance.AllMapIcons.Insert((int)MapNames.Dleks, new MapIconByName
        {
            Name = MapNames.Dleks,
            MapIcon = TouAssets.DleksTextAlt.LoadAsset(),
        });
    }
}
