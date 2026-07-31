using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PoliticianOptions : AbstractRoleOptionGroup<PoliticianRole>
{
    public override string GroupName => TouLocale.Get("TouRolePolitician", "Politician");

    [ModdedNumberOption("TouOptionPoliticianCampaignCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CampaignCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionPoliticianMaxCampaignsPerRound", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxCampaigns { get; set; } = 0f;

    [ModdedToggleOption("TouOptionPoliticianPreventCampaignOnFailedReveal")]
    public bool PreventCampaign { get; set; } = true;

    [ModdedToggleOption("TouOptionPoliticianRequireCampaignedCrewmate")]
    public bool RequireCampaignedCrewmate { get; set; } = true;
}