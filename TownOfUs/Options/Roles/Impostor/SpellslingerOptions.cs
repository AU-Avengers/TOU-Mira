using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class SpellslingerOptions : AbstractOptionGroup<SpellslingerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Spellslinger, "Spellslinger");

    [ModdedNumberOption("Hex Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HexCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Hexes", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxHexes { get; set; } = 0f;
}