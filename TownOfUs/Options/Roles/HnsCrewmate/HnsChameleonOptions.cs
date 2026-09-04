using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.HideAndSeek.Hider;

namespace TownOfUs.Options.Roles.HnsCrewmate;

public sealed class HnsChameleonOptions : AbstractRoleOptionGroup<HnsChameleonRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.HideAndSeek.Role.Chameleon", "Chameleon");

    [ModdedNumberOption("TownOfUsMira.HideAndSeek.Role.Option.ChameleonSwoopUsesPerRound", 1f, 10f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxSwoops { get; set; } = 5f;

    [ModdedNumberOption("TownOfUsMira.HideAndSeek.Role.Option.ChameleonSwoopCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopCooldown { get; set; } = 25f;

    [ModdedNumberOption("TownOfUsMira.HideAndSeek.Role.Option.ChameleonSwoopDuration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopDuration { get; set; } = 10f;

    /*[ModdedToggleOption("Swooper Can Vent")]
    public bool CanVent { get; set; } = true;*/
}