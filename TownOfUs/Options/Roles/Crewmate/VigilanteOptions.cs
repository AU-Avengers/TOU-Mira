using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class VigilanteOptions : AbstractOptionGroup<VigilanteRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Vigilante, "Vigilante");

    [ModdedNumberOption("Number Of Guesses", 1f, 15f)]
    public float VigilanteKills { get; set; } = 5f;

    [ModdedToggleOption("Can Guess More Than Once Per Meeting")]
    public bool VigilanteMultiKill { get; set; } = true;

    [ModdedToggleOption("Can Guess Neutral Benign Roles")]
    public bool VigilanteGuessNeutralBenign { get; set; } = true;

    [ModdedToggleOption("Can Guess Neutral Evil Roles")]
    public bool VigilanteGuessNeutralEvil { get; set; } = true;

    [ModdedToggleOption("Can Guess Neutral Killing Roles")]
    public bool VigilanteGuessNeutralKilling { get; set; } = true;

    [ModdedToggleOption("Can Guess Killer Modifiers")]
    public bool VigilanteGuessKillerMods { get; set; } = true;

    [ModdedToggleOption("Can Guess Alliances")]
    public bool VigilanteGuessAlliances { get; set; } = true;

    [ModdedNumberOption("Safe Shots Available", 0f, 3f, 1f, MiraNumberSuffixes.None, "0")]
    public float MultiShots { get; set; } = 3;
}