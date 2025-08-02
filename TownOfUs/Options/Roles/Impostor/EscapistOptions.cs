using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class EscapistOptions : AbstractOptionGroup<EscapistRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Escapist, "Escapist");
    public override Color GroupColor => Palette.ImpostorRoleRed;

    [ModdedNumberOption("Recall Uses Per Game", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxEscapes { get; set; } = 0f;

    [ModdedNumberOption("Recall Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RecallCooldown { get; set; } = 25f;

    [ModdedToggleOption("Escapist Can Vent")]
    public bool CanVent { get; set; } = true;
}