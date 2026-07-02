using TownOfUs.Options;
using MiraAPI.GameOptions;

namespace TownOfUs.Modules.DraftMode;

public static class DraftPoolBuilder
{
    public static List<string> BuildPool()
    {
        var pool = new List<string>();
        var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
        if (roleOpts == null)
            return pool;
        if (roleOpts.UseRoleListForPool)
        {
            var rl = OptionGroupSingleton<RoleDraftRoleListOptions>.Instance;
            if (rl == null)
                return pool;

            var slots = new[]
            {
                rl.Slot1.Value, rl.Slot2.Value, rl.Slot3.Value, rl.Slot4.Value, rl.Slot5.Value,
                rl.Slot6.Value, rl.Slot7.Value, rl.Slot8.Value, rl.Slot9.Value, rl.Slot10.Value,
                rl.Slot11.Value, rl.Slot12.Value, rl.Slot13.Value, rl.Slot14.Value, rl.Slot15.Value,
            };

            foreach (var slot in slots)
            {
                var s = RoleListOptionToString(slot);
                if (!string.IsNullOrEmpty(s)) pool.Add(s);
            }

            return pool;
        }

        var crewOpts = OptionGroupSingleton<RoleDraftCrewOptions>.Instance;
        if (crewOpts != null)
        {
            AddRepeated(pool, RoleListOption.CrewInvest, (int)crewOpts.MaxCrewInvestigative.Value);
            AddRepeated(pool, RoleListOption.CrewKilling, (int)crewOpts.MaxCrewKilling.Value);
            AddRepeated(pool, RoleListOption.CrewPower, (int)crewOpts.MaxCrewPower.Value);
            AddRepeated(pool, RoleListOption.CrewProtective, (int)crewOpts.MaxCrewProtective.Value);
            AddRepeated(pool, RoleListOption.CrewSupport, (int)crewOpts.MaxCrewSupport.Value);
        }

        var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;
        if (neutOpts != null)
        {
            var maxNeuts = Math.Max(0, (int)neutOpts.MaxNeutrals.Value);
            var addedNeuts = 0;

            var neutBuckets = new (RoleListOption bucket, int count)[ ]
            {
                (RoleListOption.NeutBenign, (int)neutOpts.MaxNeutBenign.Value),
                (RoleListOption.NeutEvil, (int)neutOpts.MaxNeutEvil.Value),
                (RoleListOption.NeutKilling, (int)neutOpts.MaxNeutKilling.Value),
                (RoleListOption.NeutOutlier, (int)neutOpts.MaxNeutOutlier.Value),
            };

            foreach (var (bucket, count) in neutBuckets)
            {
                if (addedNeuts >= maxNeuts) break;
                var allowed = Math.Min(count, maxNeuts - addedNeuts);
                AddRepeated(pool, bucket, allowed);
                addedNeuts += allowed;
            }
        }

        var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
        if (impOpts != null)
        {
            var maxImps = Math.Max(0, (int)impOpts.MaxImpostors.Value);
            var addedImps = 0;

            var impBuckets = new (RoleListOption bucket, int count)[] 
            {
                (RoleListOption.ImpConceal, (int)impOpts.MaxImpConcealing.Value),
                (RoleListOption.ImpKilling, (int)impOpts.MaxImpKilling.Value),
                (RoleListOption.ImpPower, (int)impOpts.MaxImpPower.Value),
                (RoleListOption.ImpSupport, (int)impOpts.MaxImpSupport.Value),
            };

            foreach (var (bucket, count) in impBuckets)
            {
                if (addedImps >= maxImps) break;
                var allowed = Math.Min(count, maxImps - addedImps);
                AddRepeated(pool, bucket, allowed);
                addedImps += allowed;
            }
        }

        return pool;
    }

    public static List<string> GetOfferedRoles(System.Random rng = null)
    {
        rng ??= new System.Random();
        var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
        if (roleOpts == null)
            return new List<string>();

        var pool = BuildPool();
        if (pool.Count == 0)
            return new List<string>();

        var offered = Math.Max(1, (int)roleOpts.OfferedRolesCount.Value);
        var showRandom = roleOpts.ShowRandomOption;

        if (showRandom)
        {
            var any = RoleListOptionToString(RoleListOption.Any);
            if (!string.IsNullOrEmpty(any) && !pool.Contains(any))
                pool.Add(any);
        }

        // Shuffle pool and pick distinct values up to 'offered'
        var indices = Enumerable.Range(0, pool.Count).ToArray();
        Shuffle(indices, rng);

        var picked = new List<string>();
        var seen = new HashSet<string>();
        for (int i = 0; i < indices.Length && picked.Count < offered; i++)
        {
            var candidate = pool[indices[i]];
            if (string.IsNullOrEmpty(candidate)) continue;
            if (seen.Add(candidate)) picked.Add(candidate);
        }

        return picked;
    }

    private static string RoleListOptionToString(RoleListOption opt)
    {
        var ary = RoleOptions.OptionStrings;
        var idx = (int)opt;
        if (ary == null || idx < 0 || idx >= ary.Length) return string.Empty;
        return ary[idx];
    }

    private static void AddRepeated(List<string> pool, RoleListOption bucket, int count)
    {
        if (count <= 0) return;
        var s = RoleListOptionToString(bucket);
        if (string.IsNullOrEmpty(s)) return;
        for (int i = 0; i < count; i++) pool.Add(s);
    }

    private static void Shuffle(int[] arr, System.Random rng)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            var tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
        }
    }
}