using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class InitialRoundOptions : AbstractOptionGroup
{
    public override string GroupName => "Round Start Options";
    public override uint GroupPriority => 1;

    [ModdedEnumOption("Modifier Type To Show In Role Intro", typeof(ModReveal))]
    public ModReveal ModifierReveal { get; set; } = ModReveal.Universal;

    [ModdedToggleOption("Show Faction Modifier On Role Reveal")]
    public bool TeamModifierReveal { get; set; } = true;

    [ModdedNumberOption("Initial Button Cooldowns", 10f, 30f, 2.5f, MiraNumberSuffixes.Seconds, "0.#")]
    public float GameStartCd { get; set; } = 10f;

    [ModdedEnumOption("Initial Cooldowns Apply For", typeof(StartCooldownType),
        ["All Buttons", "Specific Cooldowns", "No Buttons"])]
    public StartCooldownType StartCooldownMode { get; set; } = StartCooldownType.SpecificCooldowns;

    public ModdedNumberOption StartCooldownMin { get; set; } = new("Minimum Cooldown To Be Applicable", 5f, 0f, 60f,
        2.5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<InitialRoundOptions>.Instance.StartCooldownMode is StartCooldownType.SpecificCooldowns
    };

    public ModdedNumberOption StartCooldownMax { get; set; } = new("Maximum Cooldown To Be Applicable", 60f, 0f, 60f,
        2.5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<InitialRoundOptions>.Instance.StartCooldownMode is StartCooldownType.SpecificCooldowns
    };

    [ModdedToggleOption("First Death Shield Next Game")]
    public bool FirstDeathShield { get; set; } = true;

    [ModdedToggleOption("Indicate Round One Victims")]
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