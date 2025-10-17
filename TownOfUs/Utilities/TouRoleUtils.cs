using System.Globalization;
using System.Text;
using MiraAPI.Roles;
using TownOfUs.Roles;

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
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

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
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}{youAre}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(alignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

        return stringB;
    }
}
