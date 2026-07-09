using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
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

    [ModdedToggleOption("TouOptionPlaguebearerLegacyMode")]
    public bool LegacyPestilence { get; set; } = false;

    public ModdedToggleOption AnnouncePest { get; set; } = new("TouOptionPlaguebearerAnnounceTransformation", true)
    {
        Visible = () => OptionGroupSingleton<PlaguebearerOptions>.Instance.LegacyPestilence
    };

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
