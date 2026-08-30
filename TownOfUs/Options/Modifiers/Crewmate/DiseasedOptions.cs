using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class DiseasedOptions : AbstractTouModifierOptionGroup<DiseasedModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TouModifierDiseased", "Diseased");
    public override uint GroupPriority => 21;
    public override Color GroupColor => TownOfUsColors.Diseased;

    [ModdedNumberOption("TouOptionDiseasedKillMultiplier", 1.5f, 5f, 0.5f, MiraNumberSuffixes.Multiplier)]
    public float CooldownMultiplier { get; set; } = 3f;
}