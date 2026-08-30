using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class InitialRoundOptions : AbstractOptionGroup
{
    public override string GroupName => TouLocale.Get("TouOptionTitleRoundStart");
    public override uint GroupPriority => 1;

    [ModdedEnumOption("TouOptionModifierReveal", typeof(ModReveal),
        [
            "TouOptionModifierRevealEnumAlliance",
            "TouOptionModifierRevealEnumUniversal",
            "TouOptionModifierRevealEnumNeither"
        ])]
    public ModReveal ModifierReveal { get; set; } = ModReveal.Universal;

    [ModdedToggleOption("TouOptionTeamModifierReveal")]
    public bool TeamModifierReveal { get; set; } = true;

    [ModdedNumberOption("TouOptionGameStartCd", 10f, 30f, 2.5f,
        MiraNumberSuffixes.Seconds, "0.#")]
    public float GameStartCd { get; set; } = 10f;

    [ModdedEnumOption("TouOptionStartCooldownMode", typeof(StartCooldownType),
        [
            "TouOptionStartCooldownModeEnumAllButtons",
            "TouOptionStartCooldownModeEnumSpecificCooldowns",
            "TouOptionStartCooldownModeEnumNoButtons"
        ])]
    public StartCooldownType StartCooldownMode { get; set; } = StartCooldownType.SpecificCooldowns;

    public ModdedNumberOption StartCooldownMin { get; set; } =
        new("TouOptionStartCooldownMin", 5f, 0f, 60f,
            2.5f, MiraNumberSuffixes.Seconds, "0.#")
        {
            Visible = () =>
                OptionGroupSingleton<InitialRoundOptions>.Instance.StartCooldownMode
                is StartCooldownType.SpecificCooldowns
        };

    public ModdedNumberOption StartCooldownMax { get; set; } =
        new("TouOptionStartCooldownMax", 60f, 0f, 60f,
            2.5f, MiraNumberSuffixes.Seconds, "0.#")
        {
            Visible = () =>
                OptionGroupSingleton<InitialRoundOptions>.Instance.StartCooldownMode
                is StartCooldownType.SpecificCooldowns
        };

    [ModdedToggleOption("TouOptionFirstDeathShield")]
    public bool FirstDeathShield { get; set; } = true;

    [ModdedToggleOption("TouOptionRoundOneVictims")]
    public bool RoundOneVictims { get; set; } = true;
}
public enum StartCooldownType
{
    AllButtons,
    SpecificCooldowns,
    NoButtons
}

public enum ModReveal
{
    Alliance,
    Universal,
    Neither
}