using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Alliance;

public sealed class LoversOptions : AbstractTouModifierOptionGroup<LoverModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Lovers", "Lovers");
    public override uint GroupPriority => 12;
    public override Color GroupColor => TownOfUsColors.Lover;

    [ModdedToggleOption("TouOptionLoversDieAndReviveTogether")]
    public bool BothLoversDie { get; set; } = true;

    [ModdedNumberOption("TouOptionLoversKillerProbability", 0, 100, 10f, MiraNumberSuffixes.Percent)]
    public float LovingImpPercent { get; set; } = 20;

    [ModdedToggleOption("TouOptionLoversNeutralRoles")]
    public bool NeutralLovers { get; set; } = true;

    [ModdedToggleOption("TouOptionLoversKillFactionTeammates")]
    public bool LoverKillTeammates { get; set; } = false;

    [ModdedToggleOption("TouOptionLoversKillEachOther")]
    public bool LoversKillEachOther { get; set; } = true;
}