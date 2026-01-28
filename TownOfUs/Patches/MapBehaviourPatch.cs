using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Utilities;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    public static class MapBehaviourPatch
    {
        public static bool IsEvilCrew => PlayerControl.LocalPlayer.HasModifier<EgotistModifier>();
        public static void Postfix(MapBehaviour __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.IsImpostorAligned())
            {
                __instance.ColorControl.SetColor(Palette.ImpostorRed);
            }
            else if (player.IsCrewmate() && !IsEvilCrew)
            {
                __instance.ColorControl.SetColor(Palette.Blue);
            }
            else
            {
                __instance.ColorControl.SetColor(TownOfUsColors.Neutral);
            }
        }
    }
}