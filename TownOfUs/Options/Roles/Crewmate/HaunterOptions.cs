using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class HaunterOptions : AbstractOptionGroup<HaunterRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Haunter, "Haunter");

    [ModdedNumberOption("Tasks Left Before Clickable", 0f, 5)]
    public float NumTasksLeftBeforeClickable { get; set; } = 3f;

    [ModdedNumberOption("Tasks Left Before Alerted", 0f, 15)]
    public float NumTasksLeftBeforeAlerted { get; set; } = 1f;

    [ModdedToggleOption("Reveal Neutral Roles")]
    public bool RevealNeutralRoles { get; set; } = false;

    [ModdedEnumOption("Can Be Clicked By", typeof(HaunterRoleClickableType),
        ["Everyone", "Non-Crew", "Impostors Only"])]
    public HaunterRoleClickableType HaunterCanBeClickedBy { get; set; } = HaunterRoleClickableType.NonCrew;
}

public enum HaunterRoleClickableType
{
    Everyone,
    NonCrew,
    ImpsOnly
}