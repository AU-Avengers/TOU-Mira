using HarmonyLib;
using TownOfUs.Modules;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingMenuUpdatePatch
{
    public static void Postfix()
    {
        MeetingMenu.UpdateAll();
    }
}