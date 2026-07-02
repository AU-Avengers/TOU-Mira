using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Modules.DraftMode
{
    public static class DraftRolePool
    {
        public static Func<string, List<string>> ResolveDelegate;
        public static Func<string, ushort> IdResolver;
        public static Func<ushort, string> NameResolver;

        public static List<string> ResolveBucketToRoleNames(string bucket)
        {
            if (ResolveDelegate != null)
            {
                try { return ResolveDelegate(bucket) ?? new List<string>(); }
                catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"DraftRolePool.ResolveDelegate threw: {e}"); }
            }

            if (string.IsNullOrWhiteSpace(bucket)) return new List<string>();

            if (TryResolveBucketToConcreteRoles(bucket, out var resolvedNames))
                return resolvedNames;

            var separators = new[] { '|', ';', ',' };
            if (bucket.IndexOfAny(separators) >= 0)
            {
                return bucket.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
            }

            return new List<string> { bucket };
        }

        public static ushort ChooseRepresentativeRoleId(List<string> roleNames)
        {
            if (roleNames == null || roleNames.Count == 0) return 0;

            if (IdResolver != null)
            {
                foreach (var nm in roleNames)
                {
                    try
                    {
                        var id = IdResolver(nm);
                        if (id != 0) return id;
                    }
                    catch { /* idk */ }
                }
            }

            foreach (var nm in roleNames)
            {
                var resolved = FindRoleByName(nm);
                if (resolved != null)
                    return (ushort)resolved.Role;
            }

            var chosen = roleNames[0];
            unchecked
            {
                var hash = (uint)chosen.GetHashCode();
                return (ushort)(hash & 0xFFFF);
            }
        }

        public static string GetRoleNameFromId(ushort id)
        {
            if (NameResolver != null)
            {
                try { return NameResolver(id); }
                catch { /* idk */ }
            }

            if (id == 0) return null;
            try
            {
                var role = MiscUtils.GetRegisteredRole((RoleTypes)id) ?? RoleManager.Instance?.GetRole((RoleTypes)id);
                return role?.GetRoleName() ?? role?.NiceName;
            }
            catch { return null; }
        }

        private static bool TryResolveBucketToConcreteRoles(string bucket, out List<string> resolvedNames)
        {
            resolvedNames = new List<string>();
            if (string.IsNullOrWhiteSpace(bucket)) return false;

            if (TryMatchBucketToRoleListOption(bucket, out var roleListOption))
            {
                var roleBehaviours = GetRolesForBucket(roleListOption);
                foreach (var role in roleBehaviours)
                {
                    var name = role?.GetRoleName();
                    if (!string.IsNullOrWhiteSpace(name) && !resolvedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                        resolvedNames.Add(name);
                }
            }
            else
            {
                var directRole = FindRoleByName(bucket);
                if (directRole != null)
                {
                    var name = directRole.GetRoleName();
                    if (!string.IsNullOrWhiteSpace(name)) resolvedNames.Add(name);
                }
            }

            return resolvedNames.Count > 0;
        }

        private static RoleBehaviour FindRoleByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var normalized = NormalizeName(name);

            return MiscUtils.AllRoles.FirstOrDefault(role =>
            {
                if (role == null) return false;
                var roleName = role.GetRoleName();
                if (string.IsNullOrWhiteSpace(roleName)) return false;
                return NormalizeName(roleName) == normalized ||
                       NormalizeName(roleName.Replace(" ", string.Empty)) == normalized ||
                       NormalizeName(roleName.Replace("-", string.Empty)) == normalized;
            });
        }

        private static string NormalizeName(string value) =>
            (value ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("-", string.Empty);

        private static bool TryMatchBucketToRoleListOption(string bucket, out RoleListOption roleListOption)
        {
            roleListOption = default;
            if (string.IsNullOrWhiteSpace(bucket)) return false;

            var normalizedBucket = NormalizeName(bucket);
            for (var i = 0; i < RoleOptions.OptionStrings?.Length; i++)
            {
                if (RoleOptions.OptionStrings[i] == null) continue;
                if (NormalizeName(RoleOptions.OptionStrings[i]) == normalizedBucket)
                {
                    roleListOption = (RoleListOption)i;
                    return true;
                }
            }

            return Enum.TryParse<RoleListOption>(bucket, true, out roleListOption);
        }

        private static List<RoleBehaviour> GetRolesForBucket(RoleListOption bucket)
        {
            RoleAlignment[]? alignments = bucket switch
            {
                RoleListOption.CrewInvest => [RoleAlignment.CrewmateInvestigative],
                RoleListOption.CrewKilling => [RoleAlignment.CrewmateKilling],
                RoleListOption.CrewProtective => [RoleAlignment.CrewmateProtective],
                RoleListOption.CrewPower => [RoleAlignment.CrewmatePower],
                RoleListOption.CrewSupport => [RoleAlignment.CrewmateSupport],
                RoleListOption.CrewCommon => [RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateProtective, RoleAlignment.CrewmateSupport],
                RoleListOption.CrewSpecial => [RoleAlignment.CrewmateKilling, RoleAlignment.CrewmatePower],
                RoleListOption.CrewRandom => [RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateKilling, RoleAlignment.CrewmateProtective, RoleAlignment.CrewmatePower, RoleAlignment.CrewmateSupport],
                RoleListOption.NeutBenign => [RoleAlignment.NeutralBenign],
                RoleListOption.NeutEvil => [RoleAlignment.NeutralEvil],
                RoleListOption.NeutKilling => [RoleAlignment.NeutralKilling],
                RoleListOption.NeutOutlier => [RoleAlignment.NeutralOutlier],
                RoleListOption.NeutCommon => [RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil],
                RoleListOption.NeutSpecial => [RoleAlignment.NeutralKilling, RoleAlignment.NeutralOutlier],
                RoleListOption.NeutWildcard => [RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralOutlier],
                RoleListOption.NeutRandom => [RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralKilling, RoleAlignment.NeutralOutlier],
                RoleListOption.ImpConceal => [RoleAlignment.ImpostorConcealing],
                RoleListOption.ImpKilling => [RoleAlignment.ImpostorKilling],
                RoleListOption.ImpPower => [RoleAlignment.ImpostorPower],
                RoleListOption.ImpSupport => [RoleAlignment.ImpostorSupport],
                RoleListOption.ImpCommon => [RoleAlignment.ImpostorConcealing, RoleAlignment.ImpostorSupport],
                RoleListOption.ImpSpecial => [RoleAlignment.ImpostorKilling, RoleAlignment.ImpostorPower],
                RoleListOption.ImpRandom => [RoleAlignment.ImpostorConcealing, RoleAlignment.ImpostorKilling, RoleAlignment.ImpostorPower, RoleAlignment.ImpostorSupport],
                RoleListOption.NonImp => [RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateKilling, RoleAlignment.CrewmateProtective, RoleAlignment.CrewmatePower, RoleAlignment.CrewmateSupport, RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralKilling, RoleAlignment.NeutralOutlier],
                RoleListOption.Any => null,
                _ => null,
            };

            var roles = new List<RoleBehaviour>();
            if (alignments == null)
            {
                roles.AddRange(MiscUtils.AllRoles.Where(IsUsableRole));
            }
            else
            {
                foreach (var alignment in alignments)
                    roles.AddRange(MiscUtils.GetRegisteredRoles(alignment).Where(IsUsableRole));
            }

            var unique = new List<RoleBehaviour>();
            foreach (var role in roles)
            {
                if (role == null) continue;
                if (unique.Any(existing => existing.Role == role.Role)) continue;
                unique.Add(role);
            }

            return unique;
        }

        private static bool IsUsableRole(RoleBehaviour role)
        {
            if (role == null) return false;
            if (role.Role is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost || role.Role == (RoleTypes)RoleId.Get<NeutralGhostRole>())
                return false;
            if (role is not ITownOfUsRole touRole || !touRole.IsDraftable)
                return false;

            return role.GetRoleName() is { Length: > 0 } && CustomRoleUtils.CanSpawnOnCurrentMode(role);
        }
    }
}