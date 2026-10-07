using AmongUs.GameOptions;
using TownOfUs.Interfaces;

namespace TownOfUs.Modules;

/// <summary>
///     A named group of roles that cannot spawn together while the bucket is enabled.
///     Custom roles are matched by type, vanilla roles by <see cref="RoleTypes"/>, and roles can also
///     join a bucket via <see cref="IExclusiveRole.ExclusionBuckets"/>.
/// </summary>
public sealed class RoleExclusionBucket(string id, Func<bool> isEnabled)
{
    public string Id { get; } = id;

    /// <summary>
    ///     Whether the bucket currently applies. Built-in buckets bind this to a host option.
    /// </summary>
    public Func<bool> IsEnabled { get; } = isEnabled;

    public HashSet<Type> Roles { get; } = [];

    public HashSet<RoleTypes> VanillaRoles { get; } = [];

    public RoleExclusionBucket Add(params Type[] roles)
    {
        Roles.UnionWith(roles);
        return this;
    }

    public RoleExclusionBucket Add(params RoleTypes[] roles)
    {
        VanillaRoles.UnionWith(roles);
        return this;
    }

    public bool Contains(RoleBehaviour role)
    {
        return VanillaRoles.Contains(role.Role)
               || Roles.Any(t => t.IsInstanceOfType(role))
               || role is IExclusiveRole exclusive && exclusive.ExclusionBuckets.Contains(Id, StringComparer.OrdinalIgnoreCase);
    }
}
