using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class SaboteurOptions : AbstractOptionGroup<SaboteurModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Saboteur, "Saboteur");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 41;

    [ModdedNumberOption("Reduced Sabotage Bonus", 5f, 15f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float ReducedSaboCooldown { get; set; } = 10f;
}