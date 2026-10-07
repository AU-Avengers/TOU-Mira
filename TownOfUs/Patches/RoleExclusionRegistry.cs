using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Patches;

/// <summary>
///     Registry for role exclusion buckets. Extension mods can register their own buckets
///     or add roles to the built-in ones via <see cref="TryGet"/>.
/// </summary>
public static class RoleExclusionRegistry
{
    public const string Cleaning = "Cleaning";
    public const string Framing = "Framing";
    public const string Reviving = "Reviving";
    public const string Blinding = "Blinding";
    public const string Municipal = "Municipal";

    private static readonly Dictionary<string, RoleExclusionBucket> Buckets = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<RoleExclusionBucket> RegisteredBuckets => Buckets.Values;

    /// <summary>Registers a bucket. If a bucket with the same id already exists, the existing one is returned unchanged.</summary>
    public static RoleExclusionBucket Register(RoleExclusionBucket bucket)
    {
        if (Buckets.TryGetValue(bucket.Id, out var existing))
        {
            Warning($"Role exclusion bucket '{bucket.Id}' is already registered.");
            return existing;
        }

        Buckets.Add(bucket.Id, bucket);
        return bucket;
    }

    public static bool TryGet(string id, out RoleExclusionBucket bucket) => Buckets.TryGetValue(id, out bucket!);

    /// <summary>Whether two roles share any enabled bucket.</summary>
    public static bool AreExclusive(RoleBehaviour a, RoleBehaviour b)
    {
        return Buckets.Values.Any(x => x.IsEnabled() && x.Contains(a) && x.Contains(b));
    }

    internal static void RegisterBuiltInBuckets()
    {
        Register(new RoleExclusionBucket(Cleaning, () => OptionGroupSingleton<RoleExclusionOptions>.Instance.CleaningRoles)
            .Add(RoleTypes.Viper).Add(typeof(JanitorRole), typeof(ChefRole)));
        Register(new RoleExclusionBucket(Framing, () => OptionGroupSingleton<RoleExclusionOptions>.Instance.FramingRoles)
            .Add(RoleTypes.Shapeshifter).Add(typeof(MorphlingRole)));
        Register(new RoleExclusionBucket(Reviving, () => OptionGroupSingleton<RoleExclusionOptions>.Instance.RevivingRoles)
            .Add(typeof(AltruistRole), typeof(TimeLordRole)));
        Register(new RoleExclusionBucket(Blinding, () => OptionGroupSingleton<RoleExclusionOptions>.Instance.BlindingRoles)
            .Add(typeof(EclipsalRole), typeof(GrenadierRole)));
        Register(new RoleExclusionBucket(Municipal, () => OptionGroupSingleton<RoleExclusionOptions>.Instance.MunicipalRoles)
            .Add(typeof(OfficerRole), typeof(SheriffRole)));
    }
}
