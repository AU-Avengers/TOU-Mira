using TownOfUs.Options;
using MiraAPI.GameOptions;

namespace TownOfUs.Modules.DraftMode;

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

        return BuildPoolFromManualAmounts();
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

        if (avoid != null && avoid.Count > 0)
        {
            foreach (var candidate in poolCopy)
            {
                if (string.IsNullOrEmpty(candidate)) continue;
                if (avoid.Contains(candidate)) continue;
                if (seen.Add(candidate)) picked.Add(candidate);
                if (picked.Count >= offered) break;
            }
        }

        if (picked.Count < offered)
        {
            foreach (var candidate in poolCopy)
            {
                if (string.IsNullOrEmpty(candidate)) continue;
                if (seen.Add(candidate)) picked.Add(candidate);
                if (picked.Count >= offered) break;
            }
        }

        return picked;
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
            rl.Slot10.Value, rl.Slot11.Value, rl.Slot12.Value,
            rl.Slot13.Value, rl.Slot14.Value, rl.Slot15.Value,
        ];

        var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
        int rolesPerSlot = roleOpts != null ? Math.Max(1, (int)roleOpts.OfferedRolesCount.Value) : 3;

        UnityRng rng       = new();
        var usedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        int limit = Math.Min(numPlayers, slots.Length);
        for (int i = 0; i < limit; i++)
        {
            var roleNames = DraftRolePool.ResolveBucketToRoleNames(RoleListOptionToString(slots[i]));
            if (roleNames == null || roleNames.Count == 0) continue;

            for (int k = 0; k < rolesPerSlot; k++)
            {
                var candidates = roleNames
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Where(n => usedCounts.GetValueOrDefault(n) < DraftRolePool.GetMaxCountForRoleName(n))
                    .ToList();

                if (candidates.Count == 0) break;

                var chosen = candidates[rng.NextInt(candidates.Count)];
                pool.Add($"{chosen}|{i}");
                usedCounts[chosen] = usedCounts.GetValueOrDefault(chosen) + 1;
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
        if (neutOpts != null)
        {
            if (neutOpts.MaxNeutrals <= 0)
            {
                ExpandBucket(pool, RoleListOption.NeutBenign, 0);
                ExpandBucket(pool, RoleListOption.NeutEvil, 0);
                ExpandBucket(pool, RoleListOption.NeutKilling, 0);
                ExpandBucket(pool, RoleListOption.NeutOutlier, 0);
            } else {
                ExpandBucket(pool, RoleListOption.NeutBenign,  (int)neutOpts.MaxNeutBenign.Value);
                ExpandBucket(pool, RoleListOption.NeutEvil,    (int)neutOpts.MaxNeutEvil.Value);
                ExpandBucket(pool, RoleListOption.NeutKilling, (int)neutOpts.MaxNeutKilling.Value);
                ExpandBucket(pool, RoleListOption.NeutOutlier, (int)neutOpts.MaxNeutOutlier.Value);
            }
        }

        var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
        if (impOpts != null)
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

    private static void ExpandBucketCapped(List<string> pool, RoleListOption bucket, int maxSlots)
    {
        if (maxSlots <= 0) return;

        var names = DraftRolePool.ResolveBucketToRoleNames(RoleListOptionToString(bucket))
            ?.Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
        if (names == null || names.Count == 0) return;

        int take = Math.Min(maxSlots, names.Count);
        for (int i = 0; i < take; i++)
            pool.Add(names[i]);
    }

    private static string RoleListOptionToString(RoleListOption opt)
    {
        var ary = RoleOptions.OptionStrings;
        int idx = (int)opt;
        if (ary == null || idx < 0 || idx >= ary.Length) return string.Empty;
        return ary[idx];
    }
}