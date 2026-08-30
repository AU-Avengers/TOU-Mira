using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class ButtonBarryOptions : AbstractTouModifierOptionGroup<ButtonBarryModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TouModifierButtonBarry", "Button Barry");
    public override uint GroupPriority => 30;
    public override Color GroupColor => TownOfUsColors.ButtonBarry;

    [ModdedNumberOption("TouOptionButtonBarryButtonCooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 30f;

    [ModdedNumberOption("TouOptionButtonBarryMaxUses", 1f, 3f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumButtons { get; set; } = 1f;

    [ModdedToggleOption("TouOptionButtonBarryIgnoreSabotage")]
    public bool IgnoreSabo { get; set; } = true;

    [ModdedToggleOption("TouOptionButtonBarryAllowFirstRound")]
    public bool FirstRoundUse { get; set; } = false;
}