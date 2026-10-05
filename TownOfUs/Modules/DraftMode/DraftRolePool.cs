using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Options;
using TownOfUs.Roles;


namespace TownOfUs.Modules.DraftMode
{
    public static class DraftRolePool
    {
        private static readonly Dictionary<string, List<string>> BucketToNamesCache = new(StringComparer.OrdinalIgnoreCase);

        public static void ClearNameCache()
        {
            BucketToNamesCache.Clear();
        }

        public static List<string> ResolveBucketToRoleNames(string bucket)
        {
            if (string.IsNullOrWhiteSpace(bucket)) return new List<string>();

            if (BucketToNamesCache.TryGetValue(bucket, out var cached))
                return new List<string>(cached);

            List<string> result;
            if (TryResolveBucketToConcreteRoles(bucket, out var resolvedNames))
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

            result = TrimEmptyNames(result);

            BucketToNamesCache[bucket] = new List<string>(result);
            return new List<string>(result);
        }

        public static string BaseRoleName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            int pipeIdx = name.IndexOf('|');
            return pipeIdx >= 0 ? name.Substring(0, pipeIdx) : name;
        }

        public static ushort ResolveRoleIdFromName(string roleName)
        {
            var resolved = FindRoleByName(roleName);
            return resolved != null ? (ushort)resolved.Role : (ushort)0;
        }

        private static string RoleIdKey(RoleBehaviour role) =>
            ((ushort)role.Role).ToString(System.Globalization.CultureInfo.InvariantCulture);

        public static ushort ChooseRepresentativeRoleId(List<string> roleNames)
        {
            if (roleNames == null || roleNames.Count == 0) return 0;

            foreach (var nm in roleNames)
            {
                var resolvedId = ResolveRoleIdFromName(nm);
                if (resolvedId != 0) return resolvedId;
            }

            return 0;
        }

        public static string GetRoleNameFromId(ushort id)
        {
            if (id == 0) return null!;
            var role = MiscUtils.GetRegisteredRole((RoleTypes)id);
            return (role?.GetRoleName() ?? role?.NiceName)!;
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
                    if (role == null) continue;

                    var name = RoleIdKey(role);
                    int count = Math.Max(1, GetRoleCount(role));
                    for (int i = 0; i < count; i++)
                    {
                        names.Add(name);
                    }
                }

                UnityRng rng = new();
                for (int i = names.Count - 1; i > 0; i--)
                {
                    int j = rng.NextInt(i + 1);
                    (names[i], names[j]) = (names[j], names[i]);
                }

                resolvedNames = TrimEmptyNames(names);
                return resolvedNames.Count > 0;
            }

            var directRole = FindRoleByName(bucket);
            if (directRole != null &&
                directRole.Role != RoleTypes.Impostor &&
                directRole.Role != RoleTypes.Crewmate &&
                IsUsableRole(directRole))
            {
                resolvedNames.Add(RoleIdKey(directRole));
            }

            return resolvedNames.Count > 0;
        }

        public static int GetMaxCountForRoleName(string name)
        {
            var role = FindRoleByName(name);
            return role != null ? Math.Max(1, GetRoleCount(role)) : int.MaxValue;
        }

        public static bool IsImpostorRoleName(string name)
        {
            var role = FindRoleByName(name);
            return role != null && IsImpostorRole(role);
        }

        public static bool IsImpostorRoleId(ushort id)
        {
            var role = MiscUtils.GetRegisteredRole((RoleTypes)id);
            return role != null && IsImpostorRole(role);
        }

        public static bool IsImpostorRole(RoleBehaviour role)
        {
            if (role == null) return false;
            var alignment = role.GetRoleAlignment();
            if (
                alignment == RoleAlignment.ImpostorKilling || 
                alignment == RoleAlignment.ImpostorConcealing || 
                alignment == RoleAlignment.ImpostorPower || 
                alignment == RoleAlignment.ImpostorSupport)
            {
                return true;
            }

            return role.TeamType == RoleTeamTypes.Impostor;
        }

        public static bool IsDoubleDraftRoleName(string name)
        {
            var role = FindRoleByName(name);
            return role is IDoubleDraftRole doubleDraftRole && doubleDraftRole.IsDoubleDraftRole;
        }

        public static bool IsDoubleDraftRoleId(ushort id)
        {
            var role = MiscUtils.GetRegisteredRole((RoleTypes)id);
            return role is IDoubleDraftRole doubleDraftRole && doubleDraftRole.IsDoubleDraftRole;
        }

        public static RoleAlignment? GetRoleAlignment(string name)
        {
            var role = FindRoleByName(name);
            return role is ITownOfUsRole touRole ? touRole.RoleAlignment : null;
        }

        public static bool IsNeutralRoleName(string name)
        {
            var role = FindRoleByName(name);
            return role != null && role.IsNeutral();
        }

        public static bool IsNeutralRoleId(ushort id)
        {
            var role = MiscUtils.GetRegisteredRole((RoleTypes)id);
            return role != null && role.IsNeutral();
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
            if (!ushort.TryParse(BaseRoleName(name), out var id)) return null!;
            return MiscUtils.GetRegisteredRole((RoleTypes)id)!;
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

        private static List<string> TrimEmptyNames(IEnumerable<string> names)
        {
            return names?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
        }

        private static List<RoleBehaviour> GetRolesForBucket(RoleListOption bucket)
        {
            RoleAlignment[]? alignments = bucket switch
            {
                RoleListOption.CrewInvest => [ RoleAlignment.CrewmateInvestigative ],
                RoleListOption.CrewKilling => [ RoleAlignment.CrewmateKilling ],
                RoleListOption.CrewProtective => [ RoleAlignment.CrewmateProtective ],
                RoleListOption.CrewPower => [ RoleAlignment.CrewmatePower ],
                RoleListOption.CrewSupport => [ RoleAlignment.CrewmateSupport ],
                RoleListOption.CrewCommon => [ RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateProtective, RoleAlignment.CrewmateSupport ],
                RoleListOption.CrewSpecial => [ RoleAlignment.CrewmateKilling, RoleAlignment.CrewmatePower ],
                RoleListOption.CrewRandom => [ RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateKilling, RoleAlignment.CrewmateProtective, RoleAlignment.CrewmatePower, RoleAlignment.CrewmateSupport ],
                RoleListOption.NeutBenign => [ RoleAlignment.NeutralBenign ],
                RoleListOption.NeutEvil => [ RoleAlignment.NeutralEvil ],
                RoleListOption.NeutKilling => [ RoleAlignment.NeutralKilling ],
                RoleListOption.NeutOutlier => [ RoleAlignment.NeutralOutlier ],
                RoleListOption.NeutCommon => [ RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil ],
                RoleListOption.NeutSpecial => [ RoleAlignment.NeutralKilling, RoleAlignment.NeutralOutlier ],
                RoleListOption.NeutWildcard => [ RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralOutlier ],
                RoleListOption.NeutRandom => [ RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralKilling, RoleAlignment.NeutralOutlier ],
                RoleListOption.ImpConceal => [ RoleAlignment.ImpostorConcealing ],
                RoleListOption.ImpKilling => [ RoleAlignment.ImpostorKilling ],
                RoleListOption.ImpPower => [ RoleAlignment.ImpostorPower ],
                RoleListOption.ImpSupport => [ RoleAlignment.ImpostorSupport ],
                RoleListOption.ImpCommon => [ RoleAlignment.ImpostorConcealing, RoleAlignment.ImpostorSupport ],
                RoleListOption.ImpSpecial => [ RoleAlignment.ImpostorKilling, RoleAlignment.ImpostorPower ],
                RoleListOption.ImpRandom => [ RoleAlignment.ImpostorConcealing, RoleAlignment.ImpostorKilling, RoleAlignment.ImpostorPower, RoleAlignment.ImpostorSupport ],
                RoleListOption.NonImp => [ RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateKilling, RoleAlignment.CrewmateProtective, RoleAlignment.CrewmatePower, RoleAlignment.CrewmateSupport, RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralKilling, RoleAlignment.NeutralOutlier ],
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

        public static bool IsUsableRole(RoleBehaviour role)
        {
            if (!role) return false;
            if (role.IsDead)
                return false;
            if (role.Role == RoleTypes.Crewmate)
                return false;
            if (role.Role == AmongUs.GameOptions.RoleTypes.Impostor)
                return false;
            if (role is ITownOfUsRole touRole && (!touRole.IsDraftable || touRole.RoleAlignment > RoleAlignment.GameOutlier))
                return false;

            return role.GetRoleName() is { Length: > 0 } && CustomRoleUtils.CanSpawnOnCurrentMode(role) && IsRoleEnabled(role);
        }

        public static bool IsRoleUsableAndEnabled(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return false;
            var role = FindRoleByName(roleName);
            return role != null && IsUsableRole(role);
        }

        public static bool IsRoleEnabled(RoleBehaviour role)
        {
            if (role == null) return false;

            if (role is ICustomRole customRole)
            {
                if (customRole.Configuration.MaxRoleCount == 0) return false;
                int count = customRole.GetCount() is { } c ? (int)c : 0;
                int chance = customRole.GetChance() is { } ch ? (int)ch : 0;
                return count > 0 && chance > 0;
            }

            var roleOptions = GameOptionsManager.Instance?.CurrentGameOptions?.RoleOptions;
            if (roleOptions == null) return false;
            return roleOptions.GetNumPerGame(role.Role) > 0 && roleOptions.GetChancePerGame(role.Role) > 0;
        }

        public static int GetRoleChance(RoleBehaviour role)
        {
            if (role == null) return 0;

            if (role is ICustomRole customRole && customRole.Configuration.MaxRoleCount != 0)
                return customRole.GetChance() is { } chance ? (int)chance : 0;

            return GameOptionsManager.Instance?.CurrentGameOptions?.RoleOptions?.GetChancePerGame(role.Role) ?? 0;
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

            if (role is ICustomRole customRole && customRole.Configuration.MaxRoleCount != 0)
                return customRole.GetCount() is { } count ? (int)count : 0;

            return GameOptionsManager.Instance?.CurrentGameOptions?.RoleOptions?.GetNumPerGame(role.Role) ?? 0;
        }
    }
}