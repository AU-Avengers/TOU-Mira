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
    public static List<string> GetOfferedRoles(List<string> currentPool, IRng rng = null, ICollection<string> avoid = null)
    {
        rng ??= new UnityRng();
        var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
        if (roleOpts == null) return new List<string>();

        if (currentPool == null || currentPool.Count == 0) return new List<string>();

        int offered = Math.Max(1, (int)roleOpts.OfferedRolesCount.Value);
        var poolCopy = new List<string>(currentPool);

        if (roleOpts.ShowRandomOption)
            poolCopy.Add("__RANDOM__");

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

        var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
        int maxImps   = impOpts != null ? Math.Max(0, (int)impOpts.MaxImpostors.Value) : int.MaxValue;
        int addedImps = 0;

        UnityRng rng       = new();
        var usedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        int limit = Math.Min(numPlayers, slots.Length);
        for (int i = 0; i < limit; i++)
        {
            var roleNames = DraftRolePool.ResolveBucketToRoleNames(RoleListOptionToString(slots[i]));
            if (roleNames == null || roleNames.Count == 0) continue;

            var candidates = roleNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Where(n => usedCounts.GetValueOrDefault(n) < DraftRolePool.GetMaxCountForRoleName(n))
                .Where(n => addedImps < maxImps || !DraftRolePool.IsImpostorRoleName(n))
                .ToList();

            if (candidates.Count == 0) continue;

            var chosen = candidates[rng.NextInt(candidates.Count)];
            pool.Add(chosen);
            usedCounts[chosen] = usedCounts.GetValueOrDefault(chosen) + 1;

            if (DraftRolePool.IsImpostorRoleName(chosen))
                addedImps++;
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
            int maxNeuts   = Math.Max(0, (int)neutOpts.MaxNeutrals.Value);
            int addedNeuts = 0;

            (RoleListOption bucket, int count)[] neutBuckets =
            [
                (RoleListOption.NeutBenign,  (int)neutOpts.MaxNeutBenign.Value),
                (RoleListOption.NeutEvil,    (int)neutOpts.MaxNeutEvil.Value),
                (RoleListOption.NeutKilling, (int)neutOpts.MaxNeutKilling.Value),
                (RoleListOption.NeutOutlier, (int)neutOpts.MaxNeutOutlier.Value),
            ];

            foreach (var (bucket, count) in neutBuckets)
            {
                if (addedNeuts >= maxNeuts) break;
                int allowed = Math.Min(count, maxNeuts - addedNeuts);
                addedNeuts += ExpandBucketCapped(pool, bucket, allowed);
            }
        }

        var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
        if (impOpts != null)
        {
            int maxImps   = Math.Max(0, (int)impOpts.MaxImpostors.Value);
            int addedImps = 0;

            (RoleListOption bucket, int count)[] impBuckets =
            [
                (RoleListOption.ImpConceal, (int)impOpts.MaxImpConcealing.Value),
                (RoleListOption.ImpKilling, (int)impOpts.MaxImpKilling.Value),
                (RoleListOption.ImpPower,   (int)impOpts.MaxImpPower.Value),
                (RoleListOption.ImpSupport, (int)impOpts.MaxImpSupport.Value),
            ];

            foreach (var (bucket, count) in impBuckets)
            {
                if (addedImps >= maxImps) break;
                int allowed = Math.Min(count, maxImps - addedImps);
                addedImps += ExpandBucketCapped(pool, bucket, allowed);
            }
        }

        return pool;
    }

    private static void ExpandBucket(List<string> pool, RoleListOption bucket, int maxSlots)
    {
        ExpandBucketCapped(pool, bucket, maxSlots);
    }

    private static int ExpandBucketCapped(List<string> pool, RoleListOption bucket, int maxSlots)
    {
        if (maxSlots <= 0) return 0;

        var names = DraftRolePool.ResolveBucketToRoleNames(RoleListOptionToString(bucket))
            ?.Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
        if (names == null || names.Count == 0) return 0;

        int take = Math.Min(maxSlots, names.Count);
        for (int i = 0; i < take; i++)
            pool.Add(names[i]);

        return take;
    }

    private static string RoleListOptionToString(RoleListOption opt)
    {
        var ary = RoleOptions.OptionStrings;
        int idx = (int)opt;
        if (ary == null || idx < 0 || idx >= ary.Length) return string.Empty;
        return ary[idx];
    }
}