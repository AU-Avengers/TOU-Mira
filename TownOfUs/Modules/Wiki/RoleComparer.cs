using TownOfUs.Utilities;

namespace TownOfUs.Modules.Wiki;

public class RoleComparer(List<ushort> currentRoles) : IComparer<RoleBehaviour>
{
    public int Compare(RoleBehaviour? x, RoleBehaviour? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        var xActive = currentRoles.Contains((ushort)x.Role);
        var yActive = currentRoles.Contains((ushort)y.Role);

        if (xActive && !yActive)
        {
            return -1;
        }

        if (!xActive && yActive)
        {
            return 1;
        }

        return string.Compare(x.GetRoleName(), y.GetRoleName(), StringComparison.OrdinalIgnoreCase);
    }
}