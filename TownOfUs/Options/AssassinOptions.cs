using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class AssassinOptions : AbstractOptionGroup
{
    public override string GroupName => "Assassin Options";
    public override uint GroupPriority => 7;

    [ModdedNumberOption("Number Of Impostor Assassins", 0, 4, 1, MiraNumberSuffixes.None, "0")]
    public float NumberOfImpostorAssassins { get; set; } = 1;

    public ModdedNumberOption ImpAssassinChance { get; } =
        new("Impostor Assassin Chance", 100f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins > 0
        };

    [ModdedNumberOption("Number Of Neutral Assassins", 0, 5, 1, MiraNumberSuffixes.None, "0")]
    public float NumberOfNeutralAssassins { get; set; } = 1;

    public ModdedNumberOption NeutAssassinChance { get; } =
        new("Neutral Assassin Chance", 100f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins > 0
        };

    public ModdedToggleOption AmneTurnImpAssassin { get; } = new($"{TouLocale.Get(TouNames.Amnesiac, "Amnesiac")} Turned Impostor Gets Ability", true);

    public ModdedToggleOption AmneTurnNeutAssassin { get; } = new($"{TouLocale.Get(TouNames.Amnesiac, "Amnesiac")} Turned Neutral Killing Gets Ability", true);

    [ModdedToggleOption("Traitor Gets Ability")]
    public bool TraitorCanAssassin { get; set; } = true;

    [ModdedNumberOption("Number Of Assassin Kills", 1, 15, 1, MiraNumberSuffixes.None, "0")]
    public float AssassinKills { get; set; } = 5;

    [ModdedToggleOption("Assassin Can Kill More Than Once Per Meeting")]
    public bool AssassinMultiKill { get; set; } = true;

    [ModdedToggleOption("Assassin Can Guess \"Crewmate\"")]
    public bool AssassinCrewmateGuess { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess Crew Investigative Roles")]
    public bool AssassinGuessInvest { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess Neutral Benign Roles")]
    public bool AssassinGuessNeutralBenign { get; set; } = true;

    [ModdedToggleOption("Assassin Can Guess Neutral Evil Roles")]
    public bool AssassinGuessNeutralEvil { get; set; } = true;

    [ModdedToggleOption("Assassin Can Guess Neutral Killing Roles")]
    public bool AssassinGuessNeutralKilling { get; set; } = true;

    [ModdedToggleOption("Assassin Can Guess Impostor Roles")]
    public bool AssassinGuessImpostors { get; set; } = true;

    [ModdedToggleOption("Assassin Can Guess Crewmate Modifiers")]
    public bool AssassinGuessCrewModifiers { get; set; } = true;

    public ModdedToggleOption AssassinGuessUtilityModifiers { get; } =
        new("Assassin Can Guess Crew Utility Modifiers", false)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessCrewModifiers
        };

    [ModdedToggleOption("Assassin Can Guess Alliances")]
    public bool AssassinGuessAlliances { get; set; } = true;
}