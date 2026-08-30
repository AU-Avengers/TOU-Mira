using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(GameData))]
public static class TargetDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>().Do(x => x.CheckTargetDeath(player));
        CustomRoleUtils.GetActiveRolesOfType<FairyRole>().Do(x => x.CheckTargetDeath(player));
        CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().Do(x => x.CheckTargetDeath(player));
        var otherLover = ModifierUtils.GetActiveModifiers<LoverModifier>().FirstOrDefault(x => x.OtherLover == player);
        if (otherLover != null)
        {
            otherLover.LoverDcString = MiraLocaleManager.Get("TownOfUsMira.Modifier.LoverInfoDisconnected")
                .Replace("<player>", player.Data.PlayerName);
            otherLover.LoverDisconnected = true;
            otherLover.OtherLover = null;
        }
    }
}