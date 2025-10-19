using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Maps;

public sealed class BetterFungleOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Fungle";
    public override uint GroupPriority => 6;

    [ModdedToggleOption("Change Sabotage Timers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("Reactor Sabotage Countdown", 60f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterFungleOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownMixUp { get; set; } = new("Mix-Up Sabotage Duration", 10f, 5f, 60f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterFungleOptions>.Instance.ChangeSaboTimers
    };
}