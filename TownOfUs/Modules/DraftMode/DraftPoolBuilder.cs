using System;
using System.Collections.Generic;
using System.Linq;
using TownOfUs.Options;
using MiraAPI.GameOptions;

namespace TownOfUs.Modules.DraftMode
{
    public static class DraftPoolBuilder
    {
        public static List<string> BuildPool(int numPlayers)
        {
            DraftRolePool.ClearNameCache();
            var pool    = new List<string>();
            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts == null) return pool;

            if (roleOpts.UseRoleListForPool)
                return BuildPoolFromRoleList(numPlayers);

            var manualPool = BuildPoolFromManualAmounts();
            
            int rolesPerSlot = Math.Max(1, (int)roleOpts.OfferedRolesCount.Value);
            int targetSize = numPlayers + rolesPerSlot;
            if (manualPool.Count < targetSize)
            {
                var anyNames = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                    ?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
                if (anyNames != null && anyNames.Count > 0)
                {
                    var rng = new UnityRng();
                    while (manualPool.Count < targetSize)
                    {
                        manualPool.Add(PickWeightedByChance(anyNames, rng));
                    }
                }
            }
            
            return manualPool;
        }

        public static List<string> GetOfferedRoles(List<string> currentPool, IRng rng = null!, ICollection<string> avoid = null!)
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

            var picked = new List<string>();
            var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var candidate in poolCopy)
            {
                if (string.IsNullOrEmpty(candidate)) continue;
                var baseName = BaseRoleName(candidate);
                if (avoid != null && (avoid.Contains(candidate) || avoid.Contains(baseName))) continue;

                if (seen.Add(baseName)) picked.Add(candidate);
                if (picked.Count >= offered) break;
            }

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

        private static List<string> BuildPoolFromRoleList(int numPlayers)
        {
            var pool = new List<string>();
            var rl   = OptionGroupSingleton<RoleDraftRoleListOptions>.Instance;
            if (rl == null) return pool;

            RoleListOption[] slots =
            [
                rl.Slot1.Value,  rl.Slot2.Value,  rl.Slot3.Value,
                rl.Slot4.Value,  rl.Slot5.Value,  rl.Slot6.Value,
                rl.Slot7.Value,  rl.Slot8.Value,  rl.Slot9.Value,
                rl.Slot10.Value, rl.Slot11.Value, rl.Slot15.Value,
                rl.Slot13.Value, rl.Slot14.Value, rl.Slot15.Value,
            ];

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            int rolesPerSlot = roleOpts != null ? Math.Max(1, (int)roleOpts.OfferedRolesCount.Value) : 3;

            UnityRng rng       = new();

            int limit = Math.Min(numPlayers, slots.Length);
            for (int i = 0; i < limit; i++)
            {
                var roleNames = DraftRolePool.ResolveBucketToRoleNames(RoleListOptionToString(slots[i]));
                if (roleNames == null || roleNames.Count == 0)
                {
                    roleNames = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any));
                }

                bool addedAny = false;
                if (roleNames != null && roleNames.Count > 0)
                {
                    var slotUsed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    for (int k = 0; k < rolesPerSlot; k++)
                    {
                        var candidates = roleNames
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .Where(n => !slotUsed.Contains(n))
                            .ToList();

                        if (candidates.Count == 0) break;

                        var chosen = PickWeightedByChance(candidates, rng);
                        pool.Add($"{chosen}|{i}");
                        slotUsed.Add(chosen);
                    }
                    addedAny = true;
                }

                if (!addedAny)
                {
                    var anyNames = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                        ?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
                    if (anyNames != null && anyNames.Count > 0)
                    {
                        var chosen = PickWeightedByChance(anyNames, rng);
                        pool.Add($"{chosen}|{i}");
                    }
                    else
                    {
                        pool.Add($"Crewmate|{i}");
                    }
                }
            }

            return pool;
        }

        private static List<string> BuildPoolFromManualAmounts()
        {
            var pool = new List<string>();

            var crewOpts = OptionGroupSingleton<RoleDraftCrewOptions>.Instance;
            if (crewOpts != null)
            {
                ExpandBucket(pool, RoleListOption.CrewInvest,     (int)crewOpts.MaxCrewInvestigative.Value);
                ExpandBucket(pool, RoleListOption.CrewKilling,    (int)crewOpts.MaxCrewKilling.Value);
                ExpandBucket(pool, RoleListOption.CrewPower,      (int)crewOpts.MaxCrewPower.Value);
                ExpandBucket(pool, RoleListOption.CrewProtective, (int)crewOpts.MaxCrewProtective.Value);
                ExpandBucket(pool, RoleListOption.CrewSupport,    (int)crewOpts.MaxCrewSupport.Value);
            }

            var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;
            if (neutOpts != null && neutOpts.MaxNeutrals.Value > 0)
            {
                ExpandBucket(pool, RoleListOption.NeutBenign,  (int)neutOpts.MaxNeutBenign.Value);
                ExpandBucket(pool, RoleListOption.NeutEvil,    (int)neutOpts.MaxNeutEvil.Value);
                ExpandBucket(pool, RoleListOption.NeutKilling, (int)neutOpts.MaxNeutKilling.Value);
                ExpandBucket(pool, RoleListOption.NeutOutlier, (int)neutOpts.MaxNeutOutlier.Value);
            }

            var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
            if (impOpts != null && impOpts.MaxImpostors.Value > 0)
            {
                ExpandBucket(pool, RoleListOption.ImpConceal, (int)impOpts.MaxImpConcealing.Value);
                ExpandBucket(pool, RoleListOption.ImpKilling, (int)impOpts.MaxImpKilling.Value);
                ExpandBucket(pool, RoleListOption.ImpPower,   (int)impOpts.MaxImpPower.Value);
                ExpandBucket(pool, RoleListOption.ImpSupport, (int)impOpts.MaxImpSupport.Value);
            }

            return pool;
        }

        private static void ExpandBucket(List<string> pool, RoleListOption bucket, int maxSlots)
        {
            ExpandBucketCapped(pool, bucket, maxSlots);
        }

        private static readonly UnityRng ManualPoolRng = new();

        private static void ExpandBucketCapped(List<string> pool, RoleListOption bucket, int maxSlots)
        {
            if (maxSlots <= 0) return;

            var names = DraftRolePool.ResolveBucketToRoleNames(RoleListOptionToString(bucket))
                ?.Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            if (names == null || names.Count == 0) return;

            pool.AddRange(TakeWeightedByChance(names, maxSlots, ManualPoolRng));
        }

        private static string RoleListOptionToString(RoleListOption opt)
        {
            var ary = RoleOptions.OptionStrings;
            int idx = (int)opt;
            if (ary == null || idx < 0 || idx >= ary.Length) return string.Empty;
            return ary[idx];
        }
    }
}
