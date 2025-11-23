using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Alliance;

public sealed class CrewpostorOptions : AbstractOptionGroup<CrewpostorModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => TouLocale.Get("TouModifierCrewpostor", "Crewpostor");
    public override uint GroupPriority => 10;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;

    public ModdedToggleOption CrewpostorReplacesImpostor { get; set; } = new("Crewpostor Replaces A Real Impostor", true);

    public ModdedToggleOption CanAlwaysSabotage { get; set; } = new("Crewpostor Can Always Sabotage", false);

    public ModdedToggleOption CrewpostorVision { get; set; } = new("Crewpostor Gets Impostor Vision", true);

    public ModdedToggleOption ShowsAsImpostor { get; set; } = new("Crewpostor Appears Like A Traitor", false);
}