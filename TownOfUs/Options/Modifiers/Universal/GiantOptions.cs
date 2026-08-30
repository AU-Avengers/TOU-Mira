using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class GiantOptions : AbstractTouModifierOptionGroup<GiantModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => TouLocale.Get("TouModifierGiant", "Giant");
    public override uint GroupPriority => 32;
    public override Color GroupColor => TownOfUsColors.Giant;

    [ModdedNumberOption("TouOptionGiantSpeed", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float GiantSpeed { get; set; } = 0.75f;
}