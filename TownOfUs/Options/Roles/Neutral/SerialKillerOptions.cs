using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SerialKillerOptions : AbstractOptionGroup<SerialKillerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.SerialKiller, "Serial Killer");

    [ModdedNumberOption("Kill Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;
    [ModdedNumberOption("Bloodlust Duration", 1f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float BloodlustDuration { get; set; } = 2.5f;

    [ModdedToggleOption("Serial Killer Can Vent")]
    public bool CanVent { get; set; } = true;
}