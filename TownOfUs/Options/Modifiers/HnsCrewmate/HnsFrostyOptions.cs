using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.HnsGame.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.HnsCrewmate;

public sealed class HnsFrostyOptions : AbstractOptionGroup<HnsFrostyModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.HideAndSeek;
    public override string GroupName => TouLocale.Get("HnsModifierFrosty", "Frosty");
    public override uint GroupPriority => 5;
    public override Color GroupColor => TownOfUsColors.Frosty;

    [ModdedNumberOption("Chill Duration", 0f, 15f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ChillDuration { get; set; } = 10f;

    [ModdedNumberOption("Chill Start Speed", 0.25f, 0.95f, 0.05f, MiraNumberSuffixes.Multiplier)]
    public float ChillStartSpeed { get; set; } = 0.75f;
}