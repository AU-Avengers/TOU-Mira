using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Alliance;

public sealed class CrewpostorOptions : AbstractTouModifierOptionGroup<CrewpostorModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => TouLocale.Get("TouModifierCrewpostor", "Crewpostor");
    public override uint GroupPriority => 10;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;

    public ModdedToggleOption CrewpostorReplacesImpostor { get; set; } =
        new("TouOptionCrewpostorReplacesImpostor", true);

    public ModdedToggleOption CanAlwaysSabotage { get; set; } =
        new("TouOptionCrewpostorCanAlwaysSabotage", false);

    public ModdedToggleOption CrewpostorVision { get; set; } =
        new("TouOptionCrewpostorVision", true);

    public ModdedToggleOption ShowsAsImpostor { get; set; } =
        new("TouOptionCrewpostorShowsAsImpostor", false);
}