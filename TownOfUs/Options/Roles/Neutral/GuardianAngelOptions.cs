using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class GuardianAngelOptions : AbstractOptionGroup<GuardianAngelTouRole>
{
    public override string GroupName => TouLocale.Get(TouNames.GuardianAngel, "Guardian Angel");

    [ModdedNumberOption("Protect Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ProtectCooldown { get; set; } = 25f;

    [ModdedNumberOption("Protect Duration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float ProtectDuration { get; set; } = 10f;

    [ModdedNumberOption("Max Number Of Protects", 1, 15, 1, MiraNumberSuffixes.None, "0")]
    public float MaxProtects { get; set; } = 5;

    [ModdedEnumOption("Show Protected Player", typeof(ProtectOptions), ["Guardian Angel", "Target + GA", "Everyone"])]
    public ProtectOptions ShowProtect { get; set; } = ProtectOptions.SelfAndGA;

    [ModdedEnumOption("On Target Death, GA Becomes", typeof(BecomeOptions))]
    public BecomeOptions OnTargetDeath { get; set; } = BecomeOptions.Amnesiac;

    [ModdedToggleOption("Target Knows GA Exists")]
    public bool GATargetKnows { get; set; } = true;
    
    [ModdedToggleOption("GA Knows Targets Role")]
    public bool GAKnowsTargetRole { get; set; } = true;

    [ModdedNumberOption("Odds Of Target Being Evil", 0f, 100f, 10f, MiraNumberSuffixes.Percent, "0")]
    public float EvilTargetPercent { get; set; } = 20f;
}

public enum ProtectOptions
{
    GA,
    SelfAndGA,
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