using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.HnsCrewmate;

namespace TownOfUs.Options.Roles.HnsCrewmate;

public sealed class HnsChameleonOptions : AbstractOptionGroup<HnsChameleonRole>
{
    public override string GroupName => TouLocale.Get("HnsRoleChameleon", "Chameleon");

    [ModdedNumberOption("HnsOptionChameleonSwoopUsesPerRound", 1f, 10f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxSwoops { get; set; } = 5f;

    [ModdedNumberOption("HnsOptionChameleonSwoopCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopCooldown { get; set; } = 25f;

    [ModdedNumberOption("HnsOptionChameleonSwoopDuration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopDuration { get; set; } = 10f;

    /*[ModdedToggleOption("Swooper Can Vent")]
    public bool CanVent { get; set; } = true;*/
}