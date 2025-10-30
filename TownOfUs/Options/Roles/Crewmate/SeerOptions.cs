using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SeerOptions : AbstractOptionGroup<SeerRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSeer", "Seer");

    public ModdedToggleOption SalemSeer { get; set; } = new("Compare Players Instead of Checking Alignments", true);
    
    [ModdedNumberOption("Reveal/Compare Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SeerCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Uses of Reveal/Compare", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxCompares { get; set; } = 5f;

    public ModdedToggleOption BenignShowFriendlyToAll { get; set; } = new("Neutral Benign Show Friends To All", false)
    {
        Visible = () => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption EvilShowFriendlyToAll { get; set; } = new("Neutral Evil Show Friends To All", false)
    {
        Visible = () => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption OutlierShowFriendlyToAll { get; set; } = new("Neutral Outlier Show Friends To All", false)
    {
        Visible = () => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowCrewmateKillingAsRed { get; set; } = new("Crewmate Killing Roles Are Red", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralBenignAsRed { get; set; } = new("Neutral Benign Roles Are Red", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralEvilAsRed { get; set; } = new("Neutral Evil Roles Are Red", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralKillingAsRed { get; set; } = new("Neutral Killing Roles Are Red", true)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralOutlierAsRed { get; set; } = new("Neutral Outlier Roles Are Red", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption SwapTraitorColors { get; set; } = new("Traitor Swaps Colors", true)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };
}