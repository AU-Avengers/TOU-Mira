using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class RoleSeekerOptions : AbstractOptionGroup<RoleSeekerModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Telepath, "Role Seeker");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 42;

    [ModdedNumberOption("Reveal In-Game Role Chance", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float InGameReveal { get; set; } = 40f;

    [ModdedNumberOption("Reveal Not In-Game Role Chance", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float NotInGameReveal { get; set; } = 60f;
}
