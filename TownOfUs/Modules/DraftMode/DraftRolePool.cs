using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Roles;


namespace TownOfUs.Modules.DraftMode
{
    public static class DraftRolePool
    {
        public static Func<string, List<string>> ResolveDelegate;
        public static Func<string, ushort> IdResolver;
        public static Func<ushort, string> NameResolver;

        private static readonly Dictionary<string, ushort> RoleNameToIdCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, List<string>> BucketToNamesCache = new(StringComparer.OrdinalIgnoreCase);

        public static void ClearNameCache()
    {
        RoleNameToIdCache.Clear();
        BucketToNamesCache.Clear();
    }

        public static List<string> ResolveBucketToRoleNames(string bucket)
        {
            if (string.IsNullOrWhiteSpace(bucket)) return new List<string>();

            if (BucketToNamesCache.TryGetValue(bucket, out var cached))
                return new List<string>(cached);

            List<string> result;
            if (ResolveDelegate != null)
            {
                try
                {
                    result = ResolveDelegate(bucket) ?? new List<string>();
                }
                catch (Exception e)
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"DraftRolePool.ResolveDelegate threw: {e}");
                    result = new List<string>();
                }
            }
            else if (TryResolveBucketToConcreteRoles(bucket, out var resolvedNames))
            {
                result = resolvedNames;
            }
            else
            {
                var separators = new[] { '|', ';', ',' };
                if (bucket.IndexOfAny(separators) >= 0)
                {
                    result = bucket.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                }
                else
                {
                    result = new List<string> { bucket };
                }
            }

            result = FilterRoleNamesByBucket(result, bucket);

            BucketToNamesCache[bucket] = new List<string>(result);
            return new List<string>(result);
        }

        public static ushort ResolveRoleIdFromName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return 0;

            var baseName = roleName;
            int pipeIdx = baseName.IndexOf('|');
            if (pipeIdx >= 0) baseName = baseName.Substring(0, pipeIdx);

            if (RoleNameToIdCache.TryGetValue(baseName, out var cachedId) && cachedId != 0)
                return cachedId;

            if (ushort.TryParse(baseName, out var directId))
            {
                try
                {
                    var directRole = MiscUtils.GetRegisteredRole((RoleTypes)directId);
                    if (directRole != null)
                    {
                        CacheRoleId(baseName, directId);
                        return directId;
                    }
                }
                catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
            }

            var resolved = FindRoleByName(baseName);
            if (resolved != null)
            {
                var resolvedId = (ushort)resolved.Role;
                CacheRoleId(baseName, resolvedId);
                return resolvedId;
            }

            return 0;
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
                    catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
                }
            }

            foreach (var nm in roleNames)
            {
                var resolvedId = ResolveRoleIdFromName(nm);
                if (resolvedId != 0) return resolvedId;
            }

            return 0;
        }

        public static string GetRoleNameFromId(ushort id)
        {
            if (NameResolver != null)
            {
                try { return NameResolver(id); }
                catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
            }

            if (id == 0) return null!;
            try
            {
                var role = MiscUtils.GetRegisteredRole((RoleTypes)id);
                return (role?.GetRoleName() ?? role?.NiceName)!;
            }
            catch (Exception) { return null!; }
        }

        private static bool TryResolveBucketToConcreteRoles(string bucket, out List<string> resolvedNames)
        {
            resolvedNames = new List<string>();
            if (string.IsNullOrWhiteSpace(bucket)) return false;

            if (TryMatchBucketToRoleListOption(bucket, out var roleListOption))
            {
                var roleBehaviours = GetRolesForBucket(roleListOption);
                var names = new List<string>();
                foreach (var role in roleBehaviours)
                {
                    var name = role?.GetRoleName();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        CacheRoleId(name, (ushort)role!.Role);

                        int count = Math.Max(1, GetRoleCount(role));
                        for (int i = 0; i < count; i++)
                        {
                            names.Add(name);
                        }
                    }
                }

                UnityRng rng = new();
                for (int i = names.Count - 1; i > 0; i--)
                {
                    int j = rng.NextInt(i + 1);
                    (names[i], names[j]) = (names[j], names[i]);
                }

                resolvedNames = FilterRoleNamesByBucket(names, bucket);
                return resolvedNames.Count > 0;
            }

            var directRole = FindRoleByName(bucket);
            if (directRole != null)
            {
                var name = directRole.GetRoleName();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    CacheRoleId(name, (ushort)directRole.Role);
                    resolvedNames.Add(name);
                }
            }

            return resolvedNames.Count > 0;
        }

        public static int GetMaxCountForRoleName(string name)
        {
            var role = FindRoleByName(name);
            return role != null ? Math.Max(1, GetRoleCount(role)) : int.MaxValue;
        }

        private static void CacheRoleId(string roleName, ushort roleId)
        {
            if (string.IsNullOrWhiteSpace(roleName) || roleId == 0) return;
            RoleNameToIdCache[roleName] = roleId;
        }

        public static bool IsImpostorRoleName(string name)
        {
            var role = FindRoleByName(name);
            if (role == null) return false;

            return role.IsImpostor();
        }

        public static bool IsImpostorRoleId(ushort id)
        {
            try
            {
                var r = MiscUtils.GetRegisteredRole((RoleTypes)id);
                return r != null && r.IsImpostor();
            }
            catch { return false; }
        }

        public static bool IsExclusiveImpostorRoleName(string name)
        {
            var role = FindRoleByName(name);
            return role != null && DraftExclusiveImpostorRoles.IsRegistered(role.Role);
        }

        public static bool IsExclusiveImpostorRoleId(ushort id)
        {
            return DraftExclusiveImpostorRoles.IsRegistered(id);
        }

        public static bool IsNeutralRoleName(string name)
        {
            var role = FindRoleByName(name);
            if (role == null) return false;

            return role.IsNeutral();
        }

        public static bool IsNeutralRoleId(ushort id)
        {
            try
            {
                var r = MiscUtils.GetRegisteredRole((RoleTypes)id);
                return r != null && r.IsNeutral();
            }
            catch { return false; }
        }

        public static bool IsImpostorRoleListOption(RoleListOption opt) => opt switch
        {
            RoleListOption.ImpConceal or
            RoleListOption.ImpKilling or
            RoleListOption.ImpPower or
            RoleListOption.ImpSupport or
            RoleListOption.ImpCommon or
            RoleListOption.ImpSpecial or
            RoleListOption.ImpRandom => true,
            _ => false
        };

        public static bool IsNeutralRoleListOption(RoleListOption opt) => opt switch
        {
            RoleListOption.NeutBenign or
            RoleListOption.NeutEvil or
            RoleListOption.NeutKilling or
            RoleListOption.NeutOutlier or
            RoleListOption.NeutCommon or
            RoleListOption.NeutSpecial or
            RoleListOption.NeutWildcard or
            RoleListOption.NeutRandom => true,
            _ => false
        };


        private static RoleBehaviour FindRoleByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null!;
            int pipeIdx = name.IndexOf('|');
            if (pipeIdx >= 0) name = name.Substring(0, pipeIdx);

            if (ushort.TryParse(name, out var id))
            {
                try
                {
                    var r = MiscUtils.GetRegisteredRole((RoleTypes)id);
                    if (r != null) return r;
                }
                catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
            }

            var normalized = NormalizeName(name);

            return MiscUtils.AllRoles.FirstOrDefault(role =>
            {
                if (role == null) return false;
                var roleName = role.GetRoleName();
                if (string.IsNullOrWhiteSpace(roleName)) return false;
                return NormalizeName(roleName) == normalized ||
                       NormalizeName(roleName.Replace(" ", string.Empty)) == normalized ||
                       NormalizeName(roleName.Replace("-", string.Empty)) == normalized;
            })!;
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var clean = System.Text.RegularExpressions.Regex.Replace(value, "<.*?>", string.Empty);
            return clean.Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("-", string.Empty);
        }

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

        private static List<string> FilterRoleNamesByBucket(IEnumerable<string> names, string bucket)
        {
            if (names == null) return new List<string>();
            if (string.IsNullOrWhiteSpace(bucket)) return names.ToList();

            if (!TryGetBucketAlignments(bucket, out var alignments) || alignments == null || alignments.Length == 0)
                return names.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

            var allowed = new HashSet<RoleAlignment>(alignments);
            return names
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Where(n => IsRoleNameInAlignments(n, allowed))
                .ToList();
        }

        private static bool TryGetBucketAlignments(string bucket, out RoleAlignment[]? alignments)
        {
            alignments = null;
            if (string.IsNullOrWhiteSpace(bucket)) return false;

            if (TryMatchBucketToRoleListOption(bucket, out var roleListOption))
            {
                alignments = roleListOption switch
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

                return alignments != null;
            }

            return false;
        }

        private static bool IsRoleNameInAlignments(string roleName, HashSet<RoleAlignment> alignments)
        {
            if (string.IsNullOrWhiteSpace(roleName) || alignments == null || alignments.Count == 0)
                return false;

            var role = FindRoleByName(roleName);
            if (role == null) return false;

            var currentAlignment = role.GetRoleAlignment();
            return alignments.Contains(currentAlignment);
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
                roles.AddRange(MiscUtils.SpawnableRoles.Where(IsUsableRole));
            }
            else
            {
                foreach (var alignment in alignments)
                    roles.AddRange(MiscUtils.GetRegisteredRoles(alignment).Where(IsUsableRole));
            }

            if (bucket is RoleListOption.ImpSupport or RoleListOption.ImpRandom)
                roles.AddRange(GetUncategorizedImpostors());

            var unique = new List<RoleBehaviour>();
            foreach (var role in roles)
            {
                if (role == null) continue;
                if (unique.Any(existing => existing.Role == role.Role)) continue;
                unique.Add(role);
            }

            return unique;
        }

        private static readonly RoleAlignment[] KnownImpSubAlignments =
        [
            RoleAlignment.ImpostorConcealing, RoleAlignment.ImpostorKilling,
            RoleAlignment.ImpostorPower, RoleAlignment.ImpostorSupport
        ];

        private static IEnumerable<RoleBehaviour> GetUncategorizedImpostors()
        {
            var known = new HashSet<RoleBehaviour>();
            foreach (var alignment in KnownImpSubAlignments)
                foreach (var role in MiscUtils.GetRegisteredRoles(alignment))
                    known.Add(role);

            return MiscUtils.SpawnableRoles
                .Where(IsUsableRole)
                .Where(r => r.IsImpostor() && !known.Contains(r));
        }

        private static bool IsUsableRole(RoleBehaviour role)
        {
            if (!role) return false;
            if (role.IsDead)
                return false;
            if (role is ITownOfUsRole touRole && (!touRole.IsDraftable || touRole.RoleAlignment > RoleAlignment.GameOutlier))
                return false;

            return role.GetRoleName() is { Length: > 0 } && CustomRoleUtils.CanSpawnOnCurrentMode(role) && IsRoleEnabled(role);
        }

        public static bool IsRoleEnabled(RoleBehaviour role)
        {
            if (role == null) return false;
            try
            {
                if (role is ICustomRole customRole && customRole.Configuration.MaxRoleCount != 0)
                {
                    var countObj = customRole.GetCount();
                    var chanceObj = customRole.GetChance();
                    int count = countObj != null ? (int)countObj : 0;
                    int chance = chanceObj != null ? (int)chanceObj : 0;
                    return count > 0 && chance > 0;
                }
                else
                {
                    var roleOptions = GameOptionsManager.Instance?.CurrentGameOptions?.RoleOptions;
                    if (roleOptions != null)
                    {
                        int count = roleOptions.GetNumPerGame(role.Role);
                        int chance = roleOptions.GetChancePerGame(role.Role);
                        return count > 0 && chance > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftRolePool] Exception in IsRoleEnabled for {role.GetType().Name}: {ex}");
            }
            return false;
        }

        public static int GetRoleChance(RoleBehaviour role)
        {
            if (role == null) return 0;
            try
            {
                if (role is ICustomRole customRole && customRole.Configuration.MaxRoleCount != 0)
                {
                    var chanceObj = customRole.GetChance();
                    return chanceObj != null ? (int)chanceObj : 0;
                }
                else
                {
                    var roleOptions = GameOptionsManager.Instance?.CurrentGameOptions?.RoleOptions;
                    if (roleOptions != null)
                    {
                        return roleOptions.GetChancePerGame(role.Role);
                    }
                }
            }
            catch (Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftRolePool] Exception in GetRoleChance for {role.GetType().Name}: {ex}");
            }
            return 0;
        }

        public static int GetChanceForRoleName(string name)
        {
            var role = FindRoleByName(name);
            if (role == null) return 100;
            return Math.Clamp(GetRoleChance(role), 1, 100);
        }

        public static int GetRoleCount(RoleBehaviour role)
        {
            if (role == null) return 0;
            try
            {
                if (role is ICustomRole customRole && customRole.Configuration.MaxRoleCount != 0)
                {
                    var countObj = customRole.GetCount();
                    return countObj != null ? (int)countObj : 0;
                }
                else
                {
                    var roleOptions = GameOptionsManager.Instance?.CurrentGameOptions?.RoleOptions;
                    if (roleOptions != null)
                    {
                        return roleOptions.GetNumPerGame(role.Role);
                    }
                }
            }
            catch (Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftRolePool] Exception in GetRoleCount for {role.GetType().Name}: {ex}");
            }
            return 0;
        }
    }
}

