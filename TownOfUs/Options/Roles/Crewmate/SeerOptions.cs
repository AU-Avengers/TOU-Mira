using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SeerOptions : AbstractOptionGroup<SeerRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSeer", "Seer");

    public ModdedToggleOption SalemSeer { get; set; } = new("TouOptionSeerSalemMode", true);
    
    [ModdedNumberOption("TouOptionSeerCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SeerCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionSeerUses", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxCompares { get; set; } = 5f;

    public ModdedToggleOption BenignShowFriendlyToAll { get; set; } = new("TouOptionSeerNeutralBenignFriendly", false)
    {
        Visible = () => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption EvilShowFriendlyToAll { get; set; } = new("TouOptionSeerNeutralEvilFriendly", false)
    {
        Visible = () => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption OutlierShowFriendlyToAll { get; set; } = new("TouOptionSeerNeutralOutlierFriendly", false)
    {
        Visible = () => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowCrewmateKillingAsRed { get; set; } = new("TouOptionSeerCrewmateKillingRolesAreRed", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralBenignAsRed { get; set; } = new("TouOptionSeerNeutralBenignRolesAreRed", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralEvilAsRed { get; set; } = new("TouOptionSeerNeutralEvilRolesAreRed", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralKillingAsRed { get; set; } = new("TouOptionSeerNeutralKillingRolesAreRed", true)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption ShowNeutralOutlierAsRed { get; set; } = new("TouOptionSeerNeutralOutlierRolesAreRed", false)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };

    public ModdedToggleOption SwapTraitorColors { get; set; } = new("TouOptionSeerTraitorSwapsColors", true)
    {
        Visible = () => !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer
    };
}