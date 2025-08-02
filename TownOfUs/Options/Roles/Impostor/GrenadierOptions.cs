using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class GrenadierOptions : AbstractOptionGroup<GrenadierRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Grenadier, "Grenadier");

    [ModdedNumberOption("Flash Uses Per Game", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxFlashes { get; set; } = 0f;

    [ModdedNumberOption("Flash Grenade Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GrenadeCooldown { get; set; } = 25f;

    [ModdedNumberOption("Flash Grenade Duration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float GrenadeDuration { get; set; } = 10f;

    [ModdedNumberOption("Flash Radius", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float FlashRadius { get; set; } = 1f;

    [ModdedToggleOption("Grenadier Can Vent")]
    public bool CanVent { get; set; } = true;
}