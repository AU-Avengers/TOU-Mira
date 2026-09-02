using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ProsecutorOptions : AbstractRoleOptionGroup<ProsecutorRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Prosecutor", "Prosecutor");

    [ModdedToggleOption("TouOptionProsecutorDiesWhenCrewmateExiled")]
    public bool ExileOnCrewmate { get; set; } = true;

    [ModdedNumberOption("TouOptionProsecutorMaxProsecutions", 1, 5)]
    public float MaxProsecutions { get; set; } = 2f;
}

/*public enum BadProsecuteResult
{
    EjectPros,
    LoseUsesAndEjectTarget,
    EjectProsAndEjectTarget
}*/
