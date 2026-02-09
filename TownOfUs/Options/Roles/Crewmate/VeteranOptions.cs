using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class VeteranOptions : AbstractOptionGroup<VeteranRole>
{
    public override string GroupName => TouLocale.Get("TouRoleVeteran", "Veteran");

    [ModdedNumberOption("TouOptionVeteranAlertCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AlertCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionVeteranAlertDuration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AlertDuration { get; set; } = 10f;

    [ModdedNumberOption("TouOptionVeteranMaxNumberofAlerts", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumAlerts { get; set; } = 5f;

    [ModdedToggleOption("TouOptionVeteranCanBeKilledOnAlert")]
    public bool KilledOnAlert { get; set; } = false;

    public ModdedToggleOption KnowWhenAttackedInMeeting { get; } = new("TouOptionVeteranKnowWhenAttackedInMeeting", true)
    {
        Visible = () =>
            !OptionGroupSingleton<VeteranOptions>.Instance.KilledOnAlert
    };

    [ModdedToggleOption("TouOptionVeteranGetMoreUsesFromCompletingTasks")]
    public bool TaskUses { get; set; } = true;
}