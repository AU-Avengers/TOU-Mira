using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SeerOptions : AbstractOptionGroup<SeerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Seer, "Seer");

    [ModdedNumberOption("Seer Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SeerCooldown { get; set; } = 25f;

    [ModdedToggleOption("Crewmate Killing Roles Are Red")]
    public bool ShowCrewmateKillingAsRed { get; set; } = false;

    [ModdedToggleOption("Neutral Benign Roles Are Red")]
    public bool ShowNeutralBenignAsRed { get; set; } = false;

    [ModdedToggleOption("Neutral Evil Roles Are Red")]
    public bool ShowNeutralEvilAsRed { get; set; } = false;

    [ModdedToggleOption("Neutral Killing Roles Are Red")]
    public bool ShowNeutralKillingAsRed { get; set; } = true;

    [ModdedToggleOption("Traitor Swaps Colors")]
    public bool SwapTraitorColors { get; set; } = true;
}