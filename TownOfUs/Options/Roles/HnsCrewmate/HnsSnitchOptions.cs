using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.HideAndSeek.Hider;

namespace TownOfUs.Options.Roles.HnsCrewmate;

public sealed class HnsSnitchOptions : AbstractRoleOptionGroup<HnsSnitchRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.HideAndSeek.Role.Snitch", "Snitch");

    public ModdedNumberOption CommonTaskMultiplier { get; set; } = new("TownOfUsMira.HideAndSeek.Role.Option.SnitchCommonTaskMultiplier", 1.75f, 1f, 3f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption ShortTaskMultiplier { get; set; } = new("TownOfUsMira.HideAndSeek.Role.Option.SnitchShortTaskMultiplier", 1.6f, 1f, 3f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption LongTaskMultiplier { get; set; } = new("TownOfUsMira.HideAndSeek.Role.Option.SnitchLongTaskMultiplier", 1.9f, 1f, 3f, 0.1f,
        MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption SnitchNotifyDuration { get; set; } = new("TownOfUsMira.HideAndSeek.Role.Option.SnitchNotifyDuration", 1.5f, 0.1f, 5f, 0.1f,
        MiraNumberSuffixes.Seconds, "0.00");
}