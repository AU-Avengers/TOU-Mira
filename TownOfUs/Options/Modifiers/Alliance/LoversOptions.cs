using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Alliance;

public sealed class LoversOptions : AbstractOptionGroup<LoverModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Lovers, "Lovers");
    public override uint GroupPriority => 11;
    public override Color GroupColor => TownOfUsColors.Lover;

    [ModdedToggleOption("Both Lovers Die And Revive Together")]
    public bool BothLoversDie { get; set; } = true;

    [ModdedNumberOption("Loving Another Killer Probability", 0, 100, 10f, MiraNumberSuffixes.Percent)]
    public float LovingImpPercent { get; set; } = 20;

    [ModdedToggleOption("Neutral Roles Can Be Lovers")]
    public bool NeutralLovers { get; set; } = true;

    [ModdedToggleOption("Lover Can Kill Faction Teammates")]
    public bool LoverKillTeammates { get; set; } = false;

    [ModdedToggleOption("Lovers Can Kill One Another")]
    public bool LoversKillEachOther { get; set; } = true;
}