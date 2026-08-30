using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class SatelliteOptions : AbstractTouModifierOptionGroup<SatelliteModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => TouLocale.Get("TouModifierSatellite", "Satellite");
    public override uint GroupPriority => 34;
    public override Color GroupColor => TownOfUsColors.Satellite;

    [ModdedNumberOption("TouOptionSatelliteButtonCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 15f;

    [ModdedNumberOption("TouOptionSatelliteMaxUses", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumCast { get; set; } = 5f;

    [ModdedToggleOption("TouOptionSatelliteOneUsagePerRound")]
    public bool OneUsePerRound { get; set; } = true;

    [ModdedToggleOption("TouOptionSatelliteAllowFirstRound")]
    public bool FirstRoundUse { get; set; } = true;
}