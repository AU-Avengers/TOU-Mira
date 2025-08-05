using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class BomberOptions : AbstractOptionGroup<BomberRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Bomber, "Bomber");

    [ModdedNumberOption("Bomb Uses Per Game", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxBombs { get; set; } = 3f;

    [ModdedNumberOption("Detonate Delay", 1f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float DetonateDelay { get; set; } = 5f;

    [ModdedNumberOption("Detonate Radius", 0.05f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float DetonateRadius { get; set; } = 0.25f;

    [ModdedNumberOption("Max Kills In Detonation", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxKillsInDetonation { get; set; } = 5f;

    [ModdedToggleOption("All Impostors See Bomb")]
    public bool AllImpsSeeBomb { get; set; } = true;
    
    [ModdedToggleOption("Allow Bombing in First Round")]
    public bool CanBombFirstRound { get; set; } = true;

    [ModdedToggleOption("Bomber Can Vent")]
    public bool CanVent { get; set; } = true;
}