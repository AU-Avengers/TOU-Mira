using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Visible), MethodType.Setter)]
[HarmonyPriority(Priority.Last)]
public static class EnsureSpecAlwaysInvis
{
    public static void Prefix(PlayerControl __instance, ref bool value)
    {
        if (value)
        {
            value &= !__instance.Is((RoleTypes)RoleId.Get<SpectatorRole>());
        }
    }
}