namespace TownOfUs.Interfaces;

/// <summary>
///     Marks a role as mutually exclusive with other roles for spawning.
///     If any role in an exclusive group is chosen for a game, the others cannot spawn.
///     Exclusivity is symmetric: only one side of the relationship needs to declare it.
/// </summary>
public interface IExclusiveRole
{
    /// <summary>
    ///     The role types this role cannot spawn alongside.
    ///     Types that are not registered roles are ignored.
    /// </summary>
    IEnumerable<Type> ExclusiveWith { get; }
}
