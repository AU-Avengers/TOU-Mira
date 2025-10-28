using System.Text;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Utilities;

public static class TouRoleUtils
{
    public static StringBuilder SetTabText(ICustomRole role)
    {
        var alignment = MiscUtils.GetRoleAlignment(role);

        var youAre = "Your role is";
        if (role is ITownOfUsRole touRole2)
        {
            youAre = touRole2.YouAreText;
        }

        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"{role.RoleLongDescription}");

        return stringB;
    }

    public static StringBuilder SetDeadTabText(ICustomRole role)
    {
        var alignment = MiscUtils.GetRoleAlignment(role);

        var youAre = "Your role was";
        if (role is ITownOfUsRole touRole2)
        {
            youAre = touRole2.YouWereText;
        }

        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"{role.RoleLongDescription}");

        return stringB;
    }

    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);

    public static Vent? GetClosestUsableVent
    {
        get
        {
            var player = PlayerControl.LocalPlayer;
            Vector2 truePosition = player.GetTruePosition();
            var closestVents = Helpers.GetNearestObjectsOfType<Vent>(truePosition, player.MaxReportDistance, Filter);
            Vent? vent = null;
            if (closestVents.Count == 0)
            {
                return null;
            }
            foreach (var closeVent in closestVents)
            {
                if (player.CanUseVent(closeVent))
                {
                    vent = closeVent;
                    break;
                }
            }
            return vent;
        }
    }
}
