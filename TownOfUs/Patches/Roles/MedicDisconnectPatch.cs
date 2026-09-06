using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(GameData))]
public static class MedicDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (player.Data.Role is MedicRole)
        {
            ModifierUtils.GetActiveModifiers<MedicShieldModifier>(x => x.AllMedics.Contains(player))
                .Do(x => x.RemoveMedic(player));
        }
    }
}