using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class FairyOptions : AbstractOptionGroup<FairyRole>
{
    public override string GroupName => TouLocale.Get("TouRoleFairy", "Fairy");

    [ModdedNumberOption("TouOptionFairyCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ProtectCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionFairyDuration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float ProtectDuration { get; set; } = 10f;

    [ModdedNumberOption("TouOptionFairyMaxProtects", 1, 15, 1, MiraNumberSuffixes.None, "0")]
    public float MaxProtects { get; set; } = 5;

    [ModdedEnumOption("TouOptionFairyShowProtected", typeof(ProtectOptions), ["TouOptionFairyProtectionEnumFairy", "TouOptionFairyProtectionEnumFairyAndTarget", "TouOptionFairyProtectionEnumEveryone"])]
    public ProtectOptions ShowProtect { get; set; } = ProtectOptions.SelfAndFairy;

    [ModdedEnumOption("TouOptionFairyOnDeathFairyBecomes", typeof(BecomeOptions), ["CrewmateKeyword", "TouRoleAmnesiac", "TouRoleSurvivor", "TouRoleMercenary", "TouRoleJester"])]
    public BecomeOptions OnTargetDeath { get; set; } = BecomeOptions.Amnesiac;

    [ModdedToggleOption("TouOptionFairyTargetKnowsFairyExists")]
    public bool FairyTargetKnows { get; set; } = true;

    [ModdedToggleOption("TouOptionFairyFairyKnowsRole")]
    public bool FairyKnowsTargetRole { get; set; } = true;

    [ModdedNumberOption("TouOptionFairyEvilOdds", 0f, 100f, 10f, MiraNumberSuffixes.Percent, "0")]
    public float EvilTargetPercent { get; set; } = 20f;
}

public enum ProtectOptions
{
    Fairy,
    SelfAndFairy,
    Everyone
}

public enum BecomeOptions
{
    Crew,
    Amnesiac,
    Survivor,
    Mercenary,
    Jester
}