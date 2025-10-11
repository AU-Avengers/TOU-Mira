using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Roles.Other;

namespace TownOfUs.Options;

public sealed class GeneralOptions : AbstractOptionGroup
{
    public override string GroupName => "General";
    public override uint GroupPriority => 1;

    [ModdedEnumOption("Modifier Type To Show In Role Intro", typeof(ModReveal))]
    public ModReveal ModifierReveal { get; set; } = ModReveal.Universal;

    [ModdedToggleOption("Show Faction Modifier On Role Reveal")]
    public bool TeamModifierReveal { get; set; } = true;

    [ModdedToggleOption("Camouflage Comms")]
    public bool CamouflageComms { get; set; } = true;

    [ModdedToggleOption("Kill Anyone During Camouflage")]
    public bool KillDuringCamoComms { get; set; } = true;

    [ModdedToggleOption("Impostors Don't Know Each Other")]
    public bool FFAImpostorMode { get; set; } = false;

    public ModdedToggleOption ImpsKnowRoles { get; set; } = new("Impostors Know Each Other's Roles", true)
    {
        Visible = () => !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode
    };

    public ModdedToggleOption ImpostorChat { get; set; } = new("Impostors Get A Private Meeting Chat", true)
    {
        Visible = () => !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode
    };

    [ModdedToggleOption("Vampires Get A Private Meeting Chat")]
    public bool VampireChat { get; set; } = true;

    [ModdedToggleOption("The Dead Know Everything")]
    public bool TheDeadKnow { get; set; } = true;

    public ModdedToggleOption EnableSpectators { get; set; } = new("Allow More Spectators", true)
    {
        ChangedEvent = x =>
        {
            var list = SpectatorRole.TrackedSpectators;
            foreach (var name in list)
            {
                SpectatorRole.TrackedSpectators.Remove(name);
            }
            Logger<TownOfUsPlugin>.Debug("Removed all spectators.");
        },
    };

    [ModdedNumberOption("Game Start Cooldowns", 10f, 30f, 2.5f, MiraNumberSuffixes.Seconds, "0.#")]
    public float GameStartCd { get; set; } = 10f;

    [ModdedEnumOption("Start Cooldowns Apply For", typeof(StartCooldownType),
        ["All Buttons", "Specific Cooldowns", "No Buttons"])]
    public StartCooldownType StartCooldownMode { get; set; } = StartCooldownType.SpecificCooldowns;

    public ModdedNumberOption StartCooldownMin { get; set; } = new("Minimum Cooldown To Be Applicable", 5f, 0f, 60f,
        2.5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<GeneralOptions>.Instance.StartCooldownMode is StartCooldownType.SpecificCooldowns
    };

    public ModdedNumberOption StartCooldownMax { get; set; } = new("Maximum Cooldown To Be Applicable", 60f, 0f, 60f,
        2.5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<GeneralOptions>.Instance.StartCooldownMode is StartCooldownType.SpecificCooldowns
    };

    [ModdedNumberOption("Temp Save Cooldown Reset", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds, "0.#")]
    public float TempSaveCdReset { get; set; } = 2.5f;

    [ModdedToggleOption("Parallel Medbay Scans")]
    public bool ParallelMedbay { get; set; } = true;

    [ModdedEnumOption("Disable Meeting Skip Button", typeof(SkipState))]
    public SkipState SkipButtonDisable { get; set; } = SkipState.No;

    [ModdedNumberOption("Voting Time Added After Meeting Death", 0f, 15f, 1f, MiraNumberSuffixes.Seconds, "0.#")]
    public float AddedMeetingDeathTimer { get; set; } = 5f;

    [ModdedToggleOption("First Death Shield Next Game")]
    public bool FirstDeathShield { get; set; } = true;

    [ModdedToggleOption("Powerful Crew Continue The Game")]
    public bool CrewKillersContinue { get; set; } = true;

    [ModdedToggleOption("Hide Vent Animations Not In Vision")]
    public bool HideVentAnimationNotInVision { get; set; } = true;
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

public enum SkipState
{
    No,
    Emergency,
    Always
}