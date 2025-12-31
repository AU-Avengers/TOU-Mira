using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class ParasiteOptions : AbstractOptionGroup<ParasiteRole>
{
    public override string GroupName => TouLocale.Get("TouRoleParasite", "Parasite");
    public override Color GroupColor => Palette.ImpostorRoleRed;

    [ModdedNumberOption("TouOptionParasiteOvertakeCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float OvertakeCooldown { get; set; } = 37.5f;

    [ModdedNumberOption("TouOptionParasiteOvertakeKillCooldown", 0f, 5f, 0.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float OvertakeKillCooldown { get; set; } = 3f;

    [ModdedNumberOption("TouOptionParasiteControlDuration", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0", true)]
    public float ControlDuration { get; set; } = 30f;

    [ModdedToggleOption("TouOptionParasiteSaveVictimIfParasiteDies")]
    public bool SaveVictimIfParasiteDies { get; set; } = true;

    [ModdedToggleOption("TouOptionParasiteSaveVictimIfMeetingCalled")]
    public bool SaveVictimIfMeetingCalled { get; set; } = false;

    [ModdedToggleOption("TouOptionParasiteCanVent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("TouOptionParasiteCanMoveIndependently")]
    public bool CanMoveIndependently { get; set; } = true;

    [ModdedToggleOption("TouOptionParasiteOvertakenLooksLikeParasite")]
    public bool OvertakenLooksLikeParasite { get; set; } = false;
}