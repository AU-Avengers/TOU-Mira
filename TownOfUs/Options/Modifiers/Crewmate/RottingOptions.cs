using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class RottingOptions : AbstractTouModifierOptionGroup<RottingModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Rotting", "Rotting");
    public override uint GroupPriority => 25;
    public override Color GroupColor => TownOfUsColors.Rotting;

    [ModdedNumberOption("TouOptionRottingTimeBeforeBodyRots", 0f, 25f, 1f, MiraNumberSuffixes.Seconds)]
    public float RotDelay { get; set; } = 5f;
}