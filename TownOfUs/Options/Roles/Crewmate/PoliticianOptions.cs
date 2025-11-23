using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PoliticianOptions : AbstractOptionGroup<PoliticianRole>
{
    public override string GroupName => TouLocale.Get("TouRolePolitician", "Politician");

    [ModdedNumberOption("TouOptionPoliticianCampaignCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CampaignCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionPoliticianPreventCampaignOnFailedReveal")]
    public bool PreventCampaign { get; set; } = true;
}