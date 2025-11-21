using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class FairyOptions : AbstractOptionGroup<FairyRole>
{
    public override string GroupName => TouLocale.Get("TouRoleFairy", "Fairy");

    [ModdedNumberOption("Protect Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ProtectCooldown { get; set; } = 25f;

    [ModdedNumberOption("Protect Duration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float ProtectDuration { get; set; } = 10f;

    [ModdedNumberOption("Max Number Of Protects", 1, 15, 1, MiraNumberSuffixes.None, "0")]
    public float MaxProtects { get; set; } = 5;

    [ModdedEnumOption("Show Protected Player", typeof(ProtectOptions), ["Fairy", "Target + Fairy", "Everyone"])]
    public ProtectOptions ShowProtect { get; set; } = ProtectOptions.SelfAndFairy;

    [ModdedEnumOption("On Target Death, Fairy Becomes", typeof(BecomeOptions))]
    public BecomeOptions OnTargetDeath { get; set; } = BecomeOptions.Amnesiac;

    [ModdedToggleOption("Target Knows Fairy Exists")]
    public bool FairyTargetKnows { get; set; } = true;

    [ModdedToggleOption("Fairy Knows Targets Role")]
    public bool FairyKnowsTargetRole { get; set; } = true;

    [ModdedNumberOption("Odds Of Target Being Evil", 0f, 100f, 10f, MiraNumberSuffixes.Percent, "0")]
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