using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MirrorcasterOptions : AbstractOptionGroup<MirrorcasterRole>
{
    public override string GroupName => TouLocale.Get("TouRoleMirrorcaster", "Mirrorcaster");

    [ModdedEnumOption("TouOptionMirrorcasterWhoGetsMurderAttemptIndicator", typeof(MirrorOption),
        ["TouOptionMirrorcasterNotifEnumMirrorcaster", "TouOptionMirrorcasterNotifEnumMirrorcasterAndKiller"])]
    public MirrorOption WhoGetsNotification { get; set; } = MirrorOption.MirrorcasterAndKiller;

    public ModdedNumberOption MirrorCooldown { get; } =
        new($"TouOptionMirrorcasterMagicMirrorCooldown", 0f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MirrorDuration { get; } =
        new($"TouOptionMirrorcasterMagicMirrorDuration", 30f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption UnleashCooldown { get; } =
        new($"TouOptionMirrorcasterUnleashCooldown", 15f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption AttackInformationGiven { get; set; } = new("TouOptionMirrorcasterAttackInformationGiven", (int)MirrorAttackInfo.Subalignment, typeof(MirrorAttackInfo),
        ["TouOptionMirrorcasterInfoEnumRole", "TouOptionMirrorcasterInfoEnumSubalignment", "TouOptionMirrorcasterInfoEnumFaction", "TouOptionMirrorcasterInfoEnumNothing"]);

    [ModdedToggleOption("TouOptionMirrorcasterAccumulateMultipleUnleashes")]
    public bool MultiUnleash { get; set; } = false;

    [ModdedNumberOption("TouOptionMirrorcasterMaxNumberOfMagicMirrors", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxMirrors { get; set; } = 5f;
}

public enum MirrorOption
{
    Mirrorcaster,
    MirrorcasterAndKiller
}

public enum MirrorAttackInfo
{
    Role,
    Subalignment,
    Faction,
    Nothing
}