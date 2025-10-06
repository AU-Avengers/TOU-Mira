

using MiraAPI.Roles;

namespace TownOfUs.Utilities;

public static class BackportedHelpers
{
    /// <summary>
    /// Returns the string of a role.
    /// </summary>
    /// <param name="role">The role to find.</param>
    /// <returns>The role name.</returns>
    public static string GetRoleName(this RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.RoleName;
        }

        return role.NiceName;
    }
}