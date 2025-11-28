using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ProsecutorOptions : AbstractOptionGroup<ProsecutorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleProsecutor", "Prosecutor");

    [ModdedToggleOption("TouOptionProsecutorDiesWhenCrewmateExiled")]
    public bool ExileOnCrewmate { get; set; } = true;

    [ModdedNumberOption("TouOptionProsecutorMaxProsecutions", 1, 5)]
    public float MaxProsecutions { get; set; } = 2f;
}