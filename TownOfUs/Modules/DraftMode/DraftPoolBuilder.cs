using TownOfUs.Options;
using MiraAPI.GameOptions;

namespace TownOfUs.Modules.DraftMode
{
    public static class DraftPoolBuilder
    {
        public static List<string> BuildPool(int numPlayers, IRng rng = null!)
        {
            DraftRolePool.ClearNameCache();
            rng ??= DeterministicRng.CreateRandomlySeeded();
            var pool    = new List<string>();
            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts == null) return pool;

            if (roleOpts.UseRoleListForPool)
                return BuildPoolFromRoleList(numPlayers, rng);

            var manualPool = BuildPoolFromManualAmounts(rng);
            
            int rolesPerSlot = Math.Max(1, (int)roleOpts.OfferedRolesCount.Value);
            int targetSize = numPlayers + rolesPerSlot;
            if (manualPool.Count < targetSize)
            {
                var fallbackNames = GetAllowedManualFallbackNames();
                if (fallbackNames.Count == 0)
                {
                    fallbackNames = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                        ?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
                }

                if (fallbackNames.Count > 0)
                {
                    while (manualPool.Count < targetSize)
                    {
                        manualPool.Add(PickWeightedByChance(fallbackNames, rng));
                    }
                }
            }
            
            return manualPool;
        }

        public static List<string> GetOfferedRoles(List<string> currentPool, IRng rng = null!, ICollection<string> avoid = null!, bool allowGuaranteed = true)
        {
            rng ??= new UnityRng();
            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts == null) return new List<string>();

            if (currentPool == null || currentPool.Count == 0) return new List<string>();

            int offered = Math.Max(1, (int)roleOpts.OfferedRolesCount.Value);
            var poolCopy = new List<string>(currentPool);

            for (int i = poolCopy.Count - 1; i > 0; i--)
            {
                int j = rng.NextInt(i + 1);
                (poolCopy[i], poolCopy[j]) = (poolCopy[j], poolCopy[i]);
            }

            var eligible = poolCopy
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Where(c =>
                {
                    var baseName = BaseRoleName(c);
                    return avoid == null || (!avoid.Contains(c) && !avoid.Contains(baseName));
                })
                .ToList();

            if (eligible.Count == 0 && currentPool != null && currentPool.Count > 0)
            {
                eligible = currentPool
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();
            }

            if (eligible.Count == 0) return new List<string>();

            var picked = SelectWeightedDistinctCandidates(eligible, Math.Min(offered, eligible.Count), rng, allowGuaranteed);

            if (picked.Count < offered)
            {
                foreach (var candidate in poolCopy)
                {
                    if (picked.Count >= offered) break;
                    if (string.IsNullOrWhiteSpace(candidate)) continue;
                    if (!allowGuaranteed && DraftRolePool.GetChanceForRoleName(candidate) >= 100) continue;
                    var baseName = BaseRoleName(candidate);
                    if (avoid != null && (avoid.Contains(candidate) || avoid.Contains(baseName))) continue;
                    if (picked.Any(existing => string.Equals(BaseRoleName(existing), baseName, StringComparison.OrdinalIgnoreCase))) continue;
                    picked.Add(candidate);
                }
            }

            int cap = Math.Max(1, (int)(roleOpts?.OfferedRolesCount.Value ?? 3));
            if (picked.Count > cap) picked = picked.Take(cap).ToList();

            return picked;
        }

        private static string BaseRoleName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            int pipeIdx = name.IndexOf('|');
            return pipeIdx >= 0 ? name.Substring(0, pipeIdx) : name;
        }

        private static string PickWeightedByChance(List<string> candidates, IRng rng)
        {
            if (candidates.Count == 1) return candidates[0];

            var weights = new int[candidates.Count];
            int totalWeight = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                weights[i] = Math.Max(1, DraftRolePool.GetChanceForRoleName(candidates[i]));
                totalWeight += weights[i];
            }

            int roll = rng.NextInt(totalWeight);
            int cumulative = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative) return candidates[i];
            }

            return candidates[^1];
        }

        private static List<string> SelectWeightedDistinctCandidates(List<string> candidates, int take, IRng rng, bool allowGuaranteed = true)
        {
            if (candidates == null || candidates.Count == 0 || take <= 0) return new List<string>();

            var result = new List<string>();
            var remaining = new List<string>(candidates);
            var seenBaseNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (result.Count < take && remaining.Count > 0)
            {
                var guaranteed = allowGuaranteed
                    ? remaining
                        .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                        .Where(candidate => !seenBaseNames.Contains(BaseRoleName(candidate)))
                        .Where(candidate => DraftRolePool.GetChanceForRoleName(candidate) >= 100)
                        .ToList()
                    : new List<string>();

                var available = guaranteed.Count > 0
                    ? guaranteed
                    : remaining
                        .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                        .Where(candidate => !seenBaseNames.Contains(BaseRoleName(candidate)))
                        .Where(candidate => allowGuaranteed || DraftRolePool.GetChanceForRoleName(candidate) < 100)
                        .ToList();

                if (available.Count == 0) break;

                var chosen = PickWeightedByChance(available, rng);
                result.Add(chosen);
                seenBaseNames.Add(BaseRoleName(chosen));

                remaining.RemoveAll(candidate =>
                    string.Equals(BaseRoleName(candidate), BaseRoleName(chosen), StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        private static List<string> TakeWeightedByChance(List<string> names, int take, IRng rng)
        {
            var remaining = new List<string>(names);
            var distinctCount = remaining.Distinct().Count();
            var result = new List<string>();
            take = Math.Min(take, distinctCount);

            for (int n = 0; n < take; n++)
            {
                var chosen = PickWeightedByChance(remaining, rng);
                result.Add(chosen);
                remaining.RemoveAll(x => x == chosen);
            }

            return result;
        }

        private static List<string> BuildPoolFromRoleList(int numPlayers, IRng rng)
        {
            var pool = new List<string>();
            var rl   = OptionGroupSingleton<RoleDraftRoleListOptions>.Instance;
            if (rl == null) return pool;

            RoleListOption[] slots =
            [
                rl.Slot1.Value,  rl.Slot2.Value,  rl.Slot3.Value,
                rl.Slot4.Value,  rl.Slot5.Value,  rl.Slot6.Value,
                rl.Slot7.Value,  rl.Slot8.Value,  rl.Slot9.Value,
                rl.Slot10.Value, rl.Slot11.Value, rl.Slot12.Value,
                rl.Slot13.Value, rl.Slot14.Value, rl.Slot15.Value,
            ];

            int activeSlots = Math.Max(1, Math.Min(Math.Max(1, numPlayers), slots.Length));

            var bucketCounts = new Dictionary<RoleListOption, int>();
            for (int i = 0; i < activeSlots; i++)
            {
                bucketCounts[slots[i]] = bucketCounts.GetValueOrDefault(slots[i]) + 1;
            }

            foreach (var kvp in bucketCounts)
            {
                ExpandBucketCapped(pool, kvp.Key, kvp.Value, rng);
            }

            if (pool.Count < activeSlots)
            {
                var shortfall = activeSlots - pool.Count;
                ExpandBucketCapped(pool, RoleListOption.Any, shortfall, rng);
            }

            if (pool.Count == 0)
            {
                var fallbackId = DraftRolePool.GetAnyUsableRoleId();
                var fallbackName = fallbackId != 0 ? DraftRolePool.GetRoleNameFromId(fallbackId) : null;
                if (!string.IsNullOrEmpty(fallbackName))
                {
                    pool.Add(fallbackName);
                }
            }

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.NextInt(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            return pool;
        }

        private static List<string> SelectRoleListCandidates(List<string> candidates, int takeCount, IRng rng)
        {
            var result = new List<string>();
            if (candidates == null || candidates.Count == 0 || takeCount <= 0) return result;

            var remaining = new List<string>(candidates);
            while (result.Count < takeCount && remaining.Count > 0)
            {
                var available = remaining
                    .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                    .ToList();

                if (available.Count == 0) break;

                var chosen = PickWeightedByChance(available, rng);
                result.Add(chosen);
                remaining.RemoveAll(candidate => string.Equals(BaseRoleName(candidate), BaseRoleName(chosen), StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        private static List<string> BuildPoolFromManualAmounts(IRng rng)
        {
            var pool = new List<string>();

            var crewOpts = OptionGroupSingleton<RoleDraftCrewOptions>.Instance;
            if (crewOpts != null)
            {
                ExpandBucket(pool, RoleListOption.CrewInvest,     (int)crewOpts.MaxCrewInvestigative.Value, rng);
                ExpandBucket(pool, RoleListOption.CrewKilling,    (int)crewOpts.MaxCrewKilling.Value, rng);
                ExpandBucket(pool, RoleListOption.CrewPower,      (int)crewOpts.MaxCrewPower.Value, rng);
                ExpandBucket(pool, RoleListOption.CrewProtective, (int)crewOpts.MaxCrewProtective.Value, rng);
                ExpandBucket(pool, RoleListOption.CrewSupport,    (int)crewOpts.MaxCrewSupport.Value, rng);
            }

            var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;
            if (neutOpts != null && neutOpts.MaxNeutrals.Value > 0)
            {
                ExpandBucket(pool, RoleListOption.NeutBenign,  (int)neutOpts.MaxNeutBenign.Value, rng);
                ExpandBucket(pool, RoleListOption.NeutEvil,    (int)neutOpts.MaxNeutEvil.Value, rng);
                ExpandBucket(pool, RoleListOption.NeutKilling, (int)neutOpts.MaxNeutKilling.Value, rng);
                ExpandBucket(pool, RoleListOption.NeutOutlier, (int)neutOpts.MaxNeutOutlier.Value, rng);

                var neutralSubBucketCaps = new Dictionary<RoleListOption, int>
                {
                    [RoleListOption.NeutBenign]  = (int)neutOpts.MaxNeutBenign.Value,
                    [RoleListOption.NeutEvil]    = (int)neutOpts.MaxNeutEvil.Value,
                    [RoleListOption.NeutKilling] = (int)neutOpts.MaxNeutKilling.Value,
                    [RoleListOption.NeutOutlier] = (int)neutOpts.MaxNeutOutlier.Value,
                };

                TopUpBucketToTarget(pool, RoleListOption.NeutRandom, (int)neutOpts.MaxNeutrals.Value, rng, neutralSubBucketCaps);
            }

            var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
            if (impOpts != null && impOpts.MaxImpostors.Value > 0)
            {
                ExpandBucket(pool, RoleListOption.ImpConceal, (int)impOpts.MaxImpConcealing.Value, rng);
                ExpandBucket(pool, RoleListOption.ImpKilling, (int)impOpts.MaxImpKilling.Value, rng);
                ExpandBucket(pool, RoleListOption.ImpPower,   (int)impOpts.MaxImpPower.Value, rng);
                ExpandBucket(pool, RoleListOption.ImpSupport, (int)impOpts.MaxImpSupport.Value, rng);
            }

            return pool;
        }

        private static void ExpandBucket(List<string> pool, RoleListOption bucket, int maxSlots, IRng rng)
        {
            ExpandBucketCapped(pool, bucket, maxSlots, rng);
        }

        private static void TopUpBucketToTarget(List<string> pool, RoleListOption bucket, int targetTotal, IRng rng, Dictionary<RoleListOption, int>? subBucketCaps = null)
        {
            if (targetTotal <= 0) return;

            var names = DraftRolePool.ResolveBucketToRoleNames(bucket.ToString())
                ?.Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (names == null || names.Count == 0) return;

            var countsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in pool)
                countsByName[entry] = countsByName.GetValueOrDefault(entry) + 1;

            int currentTotal = names.Sum(n => countsByName.GetValueOrDefault(n));
            int guard = Math.Max(0, targetTotal) * 10 + 20;
            Dictionary<string, RoleListOption>? nameToSubBucket = null;
            Dictionary<RoleListOption, int>? subBucketCounts = null;
            if (subBucketCaps is { Count: > 0 })
            {
                nameToSubBucket = new Dictionary<string, RoleListOption>(StringComparer.OrdinalIgnoreCase);
                foreach (var subBucket in subBucketCaps.Keys)
                {
                    var subNames = DraftRolePool.ResolveBucketToRoleNames(subBucket.ToString())
                        ?.Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct(StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<string>();
                    foreach (var n in subNames)
                    {
                        nameToSubBucket.TryAdd(n, subBucket);
                    }
                }

                subBucketCounts = new Dictionary<RoleListOption, int>();
                foreach (var kvp in nameToSubBucket)
                {
                    var count = countsByName.GetValueOrDefault(kvp.Key);
                    if (count <= 0) continue;
                    subBucketCounts[kvp.Value] = subBucketCounts.GetValueOrDefault(kvp.Value) + count;
                }
            }

            while (currentTotal < targetTotal && guard-- > 0)
            {
                var candidates = names.Where(n => countsByName.GetValueOrDefault(n) < Math.Max(1, DraftRolePool.GetMaxCountForRoleName(n))).ToList();

                if (nameToSubBucket != null && subBucketCounts != null)
                {
                    candidates = candidates.Where(n =>
                    {
                        if (!nameToSubBucket.TryGetValue(n, out var subBucket)) return true;
                        if (!subBucketCaps!.TryGetValue(subBucket, out var cap)) return true;
                        return subBucketCounts.GetValueOrDefault(subBucket) < cap;
                    }).ToList();
                }

                if (candidates.Count == 0) break;

                var chosen = PickWeightedByChance(candidates, rng);
                pool.Add(chosen);
                countsByName[chosen] = countsByName.GetValueOrDefault(chosen) + 1;
                currentTotal++;

                if (nameToSubBucket != null && subBucketCounts != null && nameToSubBucket.TryGetValue(chosen, out var chosenSubBucket))
                {
                    subBucketCounts[chosenSubBucket] = subBucketCounts.GetValueOrDefault(chosenSubBucket) + 1;
                }
            }

            if (currentTotal < targetTotal && subBucketCaps is { Count: > 0 })
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftPoolBuilder] Could not fill neutrals up to Max Neutrals Total ({targetTotal}) without exceeding a per-type cap; stopped at {currentTotal}. Raise a per-type max (Benign/Evil/Killing/Outlier) or lower Max Neutrals Total.");
            }
        }

        private static void ExpandBucketCapped(List<string> pool, RoleListOption bucket, int maxSlots, IRng rng)
        {
            if (maxSlots <= 0) return;

            var names = DraftRolePool.ResolveBucketToRoleNames(bucket.ToString())
                ?.Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            if (names == null || names.Count == 0) return;

            pool.AddRange(TakeWeightedByChance(names, maxSlots, rng));
        }

        private static List<string> GetAllowedManualFallbackNames()
        {
            var fallbackNames = new List<string>();

            var crewOpts = OptionGroupSingleton<RoleDraftCrewOptions>.Instance;
            if (crewOpts != null)
            {
                if (crewOpts.MaxCrewInvestigative.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.CrewInvest)));
                if (crewOpts.MaxCrewKilling.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.CrewKilling)));
                if (crewOpts.MaxCrewPower.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.CrewPower)));
                if (crewOpts.MaxCrewProtective.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.CrewProtective)));
                if (crewOpts.MaxCrewSupport.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.CrewSupport)));
            }

            var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;
            if (neutOpts != null && neutOpts.MaxNeutrals.Value > 0)
            {
                if (neutOpts.MaxNeutBenign.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.NeutBenign)));
                if (neutOpts.MaxNeutEvil.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.NeutEvil)));
                if (neutOpts.MaxNeutKilling.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.NeutKilling)));
                if (neutOpts.MaxNeutOutlier.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.NeutOutlier)));
            }

            var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
            if (impOpts != null && impOpts.MaxImpostors.Value > 0)
            {
                if (impOpts.MaxImpConcealing.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.ImpConceal)));
                if (impOpts.MaxImpKilling.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.ImpKilling)));
                if (impOpts.MaxImpPower.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.ImpPower)));
                if (impOpts.MaxImpSupport.Value > 0)
                    fallbackNames.AddRange(DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.ImpSupport)));
            }

            return fallbackNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        internal static string RoleListOptionToString(RoleListOption opt)
        {
            var ary = RoleOptions.OptionStrings;
            int idx = (int)opt;
            if (ary == null || idx < 0 || idx >= ary.Length) return string.Empty;
            return ary[idx];
        }
    }
}