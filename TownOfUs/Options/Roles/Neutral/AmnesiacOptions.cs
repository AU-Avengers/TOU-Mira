using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class AmnesiacOptions : AbstractOptionGroup<AmnesiacRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Amnesiac, "Amnesiac");

    [ModdedToggleOption("Show Arrows Pointing To Dead Bodies")]
    public bool RememberArrows { get; set; } = true;

    public ModdedNumberOption RememberArrowDelay { get; } = new("Time After Death Arrow Appears", 5f, 0f, 15f, 1f,
        MiraNumberSuffixes.Seconds, "0")
    {
        Visible = () => OptionGroupSingleton<AmnesiacOptions>.Instance.RememberArrows
    };
}