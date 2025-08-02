using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PoliticianOptions : AbstractOptionGroup<PoliticianRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Politician, "Politician");

    [ModdedNumberOption("Campaign Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CampaignCooldown { get; set; } = 25f;

    [ModdedToggleOption("Prevent Campaigning on Failed Reveal")]
    public bool PreventCampaign { get; set; } = true;
}