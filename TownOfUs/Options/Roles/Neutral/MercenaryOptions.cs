using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class MercenaryOptions : AbstractOptionGroup<MercenaryRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Mercenary, "Mercenary");

    [ModdedNumberOption("Guard Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GuardCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Number Of Guards", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxUses { get; set; } = 6f;

    [ModdedNumberOption("Bribe Cost", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float BribeCost { get; set; } = 2f;
}