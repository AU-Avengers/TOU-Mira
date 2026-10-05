using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class FlashOptions : AbstractTouModifierOptionGroup<FlashModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Flash", "Flash");
    public override uint GroupPriority => 31;
    public override Color GroupColor => TownOfUsColors.Flash;

    [ModdedNumberOption("TouOptionFlashSpeed", 1.05f, 2.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float FlashSpeed { get; set; } = 1.75f;
}