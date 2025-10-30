using HarmonyLib;
using Object = Il2CppSystem.Object;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(Palette), nameof(Palette.GetColorName))]
public static class PaletteColorsPatch
{
    [HarmonyPatch(typeof(Palette), nameof(Palette.GetColorName))]
    public static bool Prefix(int colorId, ref string __result)
    {
        if (colorId < 0 || colorId >= Palette.ColorNames.Length)
        {
            Error(string.Format(TownOfUsPlugin.Culture, "Color ID {0}, but Palette Length of {1}", colorId, Palette.ColorNames.Length));
            __result = "???";
            return false;
        }
        var vanillaString = DestroyableSingleton<TranslationController>.Instance.GetString(Palette.ColorNames[colorId], Array.Empty<Object>());
        
        if (vanillaString != null)
        {
            var name = TouLocale.Get($"{vanillaString}");
            if (name.Contains("STRMISS"))
            {
                __result = vanillaString;
                return false;
            }

            __result = name;
            return false;
        }
        __result = vanillaString ?? "???";
        return false;
    }
}
