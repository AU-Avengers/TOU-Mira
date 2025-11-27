using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.HnsCrewmate;

namespace TownOfUs.Options.Roles.HnsCrewmate;

public sealed class HnsSnitchOptions : AbstractOptionGroup<HnsSnitchRole>
{
    public override string GroupName => TouLocale.Get("HnsRoleSnitch", "Snitch");

    public ModdedNumberOption CommonTaskMultiplier { get; set; } = new("HnsOptionSnitchCommonTaskMultiplier", 1.75f, 1f, 3f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption ShortTaskMultiplier { get; set; } = new("HnsOptionSnitchShortTaskMultiplier", 1.6f, 1f, 3f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption LongTaskMultiplier { get; set; } = new("HnsOptionSnitchLongTaskMultiplier", 1.9f, 1f, 3f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption SnitchNotifyDuration { get; set; } = new("HnsOptionSnitchNotifyDuration", 1.5f, 0.1f, 5f, 0.1f,
        MiraNumberSuffixes.Seconds, "0.00");
}