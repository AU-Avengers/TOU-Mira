using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PoliticianOptions : AbstractRoleOptionGroup<PoliticianRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Politician", "Politician");

    [ModdedNumberOption("TouOptionPoliticianCampaignCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CampaignCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionPoliticianPreventCampaignOnFailedReveal")]
    public bool PreventCampaign { get; set; } = true;
}