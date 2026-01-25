using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class PlaguebearerOptions : AbstractOptionGroup<PlaguebearerRole>
{
    public override string GroupName => TouLocale.Get("TouRolePlaguebearer", "Plaguebearer");

    [ModdedNumberOption("TouOptionPlaguebearerInstantPesti", 0, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float PestChance { get; set; } = 0f;

    [ModdedNumberOption("TouOptionPlaguebearerInfectCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InfectCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionPlaguebearerAnnounceTransformation")]
    public bool AnnouncePest { get; set; } = true;

    [ModdedNumberOption("TouOptionPlaguebearerPestilenceKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PestKillCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionPlaguebearerPestilenceCanVent")]
    public bool CanVent { get; set; } = false;
}

public enum PestRevealMode
{
    NoReveal,
    RevealAfterMeeting,
    RevealInMeeting
}
