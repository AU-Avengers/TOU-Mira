using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Roles;
using TownOfUs.Roles.Other;

namespace TownOfUs.Utilities;

/// <summary>
/// Stores /up command requests for role assignment.
/// Maps player names to requested role names (which are then resolved to role types).
/// </summary>
public static class UpCommandRequests
{
    /// <summary>
    /// Dictionary mapping player names to requested role names.
    /// </summary>
    private static readonly Dictionary<string, string> Requests = new();

    /// <summary>
    /// Clears all /up requests. Should be called when entering lobby.
    /// </summary>
    public static void Clear()
    {
        Requests.Clear();
    }

    /// <summary>
    /// Adds or updates a /up request for a player.
    /// </summary>
    /// <param name="playerName">The name of the player requesting the role.</param>
    /// <param name="roleName">The role name requested.</param>
    public static void SetRequest(string playerName, string roleName)
    {
        Requests[playerName] = roleName;
    }

    /// <summary>
    /// Gets the requested role type for a player, if any.
    /// </summary>
    /// <param name="playerName">The name of the player.</param>
    /// <param name="roleType">The requested role type, if found.</param>
    /// <returns>True if the player has a /up request, false otherwise.</returns>
    public static bool TryGetRequest(string playerName, out RoleTypes roleType)
    {
        roleType = RoleTypes.Crewmate;

        if (!Requests.TryGetValue(playerName, out var roleName))
        {
            return false;
        }

        // Find the role by name or locale key (filtered to current mode to avoid collisions like Classic vs HnS "Snitch").
        // Use the same matching logic as in ChatCommandsPatch to ensure consistency between lobby and in-game.
        var allRoles = MiscUtils.AllRegisteredRoles
            .Where(r => !r.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(r))
            .ToList();
        
        // First try exact match (with and without spaces)
        var role = allRoles.FirstOrDefault(r =>
            r.GetRoleName().Equals(roleName, StringComparison.OrdinalIgnoreCase) ||
            r.GetRoleName().Replace(" ", "").Equals(roleName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
            (r is ITownOfUsRole touRole && touRole.LocaleKey.Equals(roleName, StringComparison.OrdinalIgnoreCase)));

        // If exact match fails, try fuzzy matching (contains)
        if (role == null)
        {
            role = allRoles.FirstOrDefault(r =>
                r.GetRoleName().Contains(roleName, StringComparison.OrdinalIgnoreCase) ||
                roleName.Contains(r.GetRoleName(), StringComparison.OrdinalIgnoreCase) ||
                (r is ITownOfUsRole touRole2 && (touRole2.LocaleKey.Contains(roleName, StringComparison.OrdinalIgnoreCase) ||
                                                 roleName.Contains(touRole2.LocaleKey, StringComparison.OrdinalIgnoreCase))));
        }

        if (role == null)
        {
            return false;
        }

        roleType = role.Role;
        return true;
    }

    /// <summary>
    /// Gets the requested role type for a player by their NetworkedPlayerInfo.
    /// </summary>
    /// <param name="playerInfo">The player info.</param>
    /// <param name="roleType">The requested role type, if found.</param>
    /// <returns>True if the player has a /up request, false otherwise.</returns>
    public static bool TryGetRequest(NetworkedPlayerInfo playerInfo, out RoleTypes roleType)
    {
        // Exclude spectators from /up requests
        if (SpectatorRole.TrackedSpectators.Contains(playerInfo.PlayerName))
        {
            roleType = RoleTypes.Crewmate;
            return false;
        }

        return TryGetRequest(playerInfo.PlayerName, out roleType);
    }

    /// <summary>
    /// Gets the requested role object for a player, if any.
    /// </summary>
    /// <param name="playerName">The name of the player.</param>
    /// <param name="role">The requested role object, if found.</param>
    /// <returns>True if the player has a /up request, false otherwise.</returns>
    public static bool TryGetRequestRole(string playerName, out RoleBehaviour role)
    {
        role = null!;

        if (!Requests.TryGetValue(playerName, out var roleName))
        {
            return false;
        }

        // Find the role by name or locale key (filtered to current mode to avoid collisions like Classic vs HnS "Snitch").
        // Use the same matching logic as in ChatCommandsPatch to ensure consistency between lobby and in-game.
        var allRoles = MiscUtils.AllRegisteredRoles
            .Where(r => !r.IsDead && CustomRoleUtils.CanSpawnOnCurrentMode(r))
            .ToList();
        
        // First try exact match (with and without spaces)
        var foundRole = allRoles.FirstOrDefault(r =>
            r.GetRoleName().Equals(roleName, StringComparison.OrdinalIgnoreCase) ||
            r.GetRoleName().Replace(" ", "").Equals(roleName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
            (r is ITownOfUsRole touRole && touRole.LocaleKey.Equals(roleName, StringComparison.OrdinalIgnoreCase)));

        // If exact match fails, try fuzzy matching (contains)
        if (foundRole == null)
        {
            foundRole = allRoles.FirstOrDefault(r =>
                r.GetRoleName().Contains(roleName, StringComparison.OrdinalIgnoreCase) ||
                roleName.Contains(r.GetRoleName(), StringComparison.OrdinalIgnoreCase) ||
                (r is ITownOfUsRole touRole2 && (touRole2.LocaleKey.Contains(roleName, StringComparison.OrdinalIgnoreCase) ||
                                                 roleName.Contains(touRole2.LocaleKey, StringComparison.OrdinalIgnoreCase))));
        }

        if (foundRole == null)
        {
            return false;
        }

        role = foundRole;
        return true;
    }

    /// <summary>
    /// Removes a /up request for a player.
    /// </summary>
    /// <param name="playerName">The name of the player.</param>
    public static void RemoveRequest(string playerName)
    {
        Requests.Remove(playerName);
    }

    /// <summary>
    /// Gets all current /up requests.
    /// </summary>
    /// <returns>A dictionary of all current requests (player name -> role name).</returns>
    public static Dictionary<string, string> GetAllRequests()
    {
        return new Dictionary<string, string>(Requests);
    }
}