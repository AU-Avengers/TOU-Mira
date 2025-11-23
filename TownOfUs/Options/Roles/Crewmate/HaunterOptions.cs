using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class HaunterOptions : AbstractOptionGroup<HaunterRole>
{
    public override string GroupName => TouLocale.Get("TouRoleHaunter", "Haunter");

    [ModdedNumberOption("TouOptionHaunterNumTasksLeftBeforeClickable", 0f, 5)]
    public float NumTasksLeftBeforeClickable { get; set; } = 3f;

    [ModdedNumberOption("TouOptionHaunterNumTasksLeftBeforeAlerted", 0f, 15)]
    public float NumTasksLeftBeforeAlerted { get; set; } = 1f;

    [ModdedToggleOption("TouOptionHaunterRevealNeutralRoles")]
    public bool RevealNeutralRoles { get; set; } = false;

    [ModdedEnumOption("TouOptionHaunterCanBeClickedBy", typeof(HaunterRoleClickableType),
        ["TouOptionHaunterClickEnumEveryone", "TouOptionHaunterClickEnumNonCrew", "TouOptionHaunterClickEnumImpsOnly"])]
    public HaunterRoleClickableType HaunterCanBeClickedBy { get; set; } = HaunterRoleClickableType.NonCrew;
}

public enum HaunterRoleClickableType
{
    Everyone,
    NonCrew,
    ImpsOnly
}