using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Other;

namespace TownOfUs.Patches.Modifiers;

[HarmonyPatch]
public static class DrunkPhysicsPatch
{
    private static Func<RoleblockedModifier, bool> RoleblockPredicate { get; } =
        rbMod => rbMod.InvertControls;
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerPhysics __instance)
    {
        var player = __instance.myPlayer;
        if (!__instance.AmOwner || !GameData.Instance || !player.CanMove || player.Data.IsDead)
        {
            return;
        }

        if (player.HasModifier(RoleblockPredicate) || player.HasModifier<DrunkModifier>())
        {
            __instance.body.velocity *= -1;
        }
    }
}