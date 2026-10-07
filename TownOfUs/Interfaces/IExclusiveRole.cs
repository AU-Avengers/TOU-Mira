using TownOfUs.Patches;

namespace TownOfUs.Interfaces;

/// <summary>
///     Marks a role as mutually exclusive with other roles for spawning.
///     A role can name specific roles it cannot spawn alongside, join exclusion buckets registered in
///     <see cref="RoleExclusionRegistry"/>, or both. Exclusivity is symmetric: only one side needs to declare it.
/// </summary>
public interface IExclusiveRole
{
    /// <summary>
    ///     Specific role types this role cannot spawn alongside. Types that are not registered roles are ignored.
    /// </summary>
    IEnumerable<Type> ExclusiveWith => [];

    /// <summary>
    ///     Ids of exclusion buckets this role belongs to (e.g. <see cref="RoleExclusionRegistry.Cleaning"/>).
    ///     The bucket's host toggle controls whether the exclusion applies.
    /// </summary>
    IEnumerable<string> ExclusionBuckets => [];
}
