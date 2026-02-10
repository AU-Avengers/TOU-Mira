using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class HerbalistOptions : AbstractOptionGroup<HerbalistRole>
{
    public override string GroupName => TouLocale.Get("TouRoleHerbalist", "Herbalist");

    [ModdedNumberOption("Herb Cooldown", 10f, 90f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HerbCooldown { get; set; } = 30f;

    [ModdedNumberOption("Confuse Delay", 0.5f, 5f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ConfuseDelay{ get; set; } = 3f;

    [ModdedNumberOption("Confuse Duration", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ConfuseDuration { get; set; } = 15f;

    /*
    [ModdedNumberOption("Glamour Duration", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GlamourDuration { get; set; } = 15f;*/

    [ModdedNumberOption("Protect Duration", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ProtectDuration { get; set; } = 15f;
}