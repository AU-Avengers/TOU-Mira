using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class ShroudOptions : AbstractOptionGroup<ShroudRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Shroud, "Shroud");

    [ModdedNumberOption("Kill Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;
    [ModdedNumberOption("Enshroud Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float EnshroudCooldown { get; set; } = 35f;
    [ModdedNumberOption("Compel Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CompelCooldown { get; set; } = 10f;

    public bool CanVent { get; set; } = true;
}