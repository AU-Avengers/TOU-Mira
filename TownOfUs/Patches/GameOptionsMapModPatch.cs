

using HarmonyLib;
using TownOfUs.Modules;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(CreateGameMapPicker), nameof(CreateGameMapPicker.Initialize))]
public static class GameOptionsMapModPatch
{
    public static bool Prefix(CreateGameMapPicker __instance, int maskLayer)
    {
        var id = (int)GameOptionsManager.Instance.GameHostOptions.MapId;
        
        // If Submerged is not loaded, and you were previously using Submerged, hosting a game shouldn't softlock
        // The same deal with Level Impostor
        if ((id == (byte)ModCompatibility.SubmergedMapType && !ModCompatibility.SubLoaded)
            || (id == (byte)ModCompatibility.LevelImpostorMapType && !ModCompatibility.LILoaded))
        {
            id = 0;
        }
        __instance.selectedMapId = id;
        __instance.SetupMapButtons(maskLayer);
        return false;
    }
}
