using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class SaboteurOptions : AbstractOptionGroup<SaboteurModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => TouLocale.Get("TouModifierSaboteur", "Saboteur");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 40;

    [ModdedNumberOption("Reduced Sabotage Bonus", 5f, 15f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float ReducedSaboCooldown { get; set; } = 10f;
}