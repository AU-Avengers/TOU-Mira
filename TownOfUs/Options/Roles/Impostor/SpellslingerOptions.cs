using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class SpellslingerOptions : AbstractOptionGroup<SpellslingerRole>
{
    public override string GroupName => TouLocale.Get($"TouRoleSpellslinger");

    [ModdedNumberOption("Hex Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HexCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Hexes", 3f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxHexes { get; set; } = 5f;

    [ModdedNumberOption("Hex Bomb Countdown Duration", 30f, 180f, 5f, MiraNumberSuffixes.Seconds)]
    public float HexBombDuration { get; set; } = 120f;
}