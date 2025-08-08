using HarmonyLib;

namespace TownOfUs.Patches
{

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    public static class MapBehaviourPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            __instance.ColorControl.SetColor(PlayerControl.LocalPlayer.Data.Role.TeamColor);
        }
    }
}