using HarmonyLib;
using TownOfUs.Modules;
using TownOfUs.Roles;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class SubmergedHudPatch
{
    public static void Postfix(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
        {
            return;
        }

        if (ModCompatibility.IsSubmerged() && PlayerControl.LocalPlayer.Data.IsDead &&
            PlayerControl.LocalPlayer.Data.Role is IGhostRole ghost)
        {
            __instance.MapButton.transform.parent.Find(__instance.MapButton.name + "(Clone)")?.gameObject
                ?.SetActive(!ghost.GhostActive);
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics))]
[HarmonyPriority(Priority.Low)] // make sure it occurs after other patches
public static class SubmergedLateUpdatePhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.HandleAnimation))]
    [HarmonyPatch(nameof(PlayerPhysics.LateUpdate))]
    public static void Postfix(PlayerPhysics __instance)
    {
        if (!ModCompatibility.IsSubmerged())
        {
            return;
        }

        ModCompatibility.GhostRoleFix(__instance);
    }
}