using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class MercenaryOptions : AbstractOptionGroup<MercenaryRole>
{
    public override string GroupName => TouLocale.Get("TouRoleMercenary", "Mercenary");

    [ModdedNumberOption("Guard Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GuardCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Number Of Guards", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxUses { get; set; } = 6f;

    [ModdedNumberOption("Bribe Cost", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float BribeCost { get; set; } = 2f;

    public ModdedToggleOption GuardProtection { get; set; } = new("Guarding Stops Attacks", true);

    public ModdedNumberOption GoldGivenFromAttack { get; set; } = new("Gold Given From An Attack", 2f, 0f, 3f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => OptionGroupSingleton<MercenaryOptions>.Instance.GuardProtection.Value
    };
}