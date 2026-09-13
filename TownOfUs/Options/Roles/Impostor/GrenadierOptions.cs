using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class GrenadierOptions : AbstractRoleOptionGroup<GrenadierRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Grenadier", "Grenadier");

    [ModdedNumberOption("TouOptionGrenadierFlashUsesPerGame", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxFlashes { get; set; } = 0f;

    [ModdedNumberOption("TouOptionGrenadierFlashGrenadeCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GrenadeCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionGrenadierFlashGrenadeDuration", 5f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float GrenadeDuration { get; set; } = 10f;

    [ModdedNumberOption("TouOptionGrenadierFlashRadius", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float FlashRadius { get; set; } = 1f;

    [ModdedToggleOption("TouOptionGrenadierAllowFlashingDuringSabotage")]
    public bool SabotageFlashing { get; set; } = false;

    [ModdedToggleOption("TouOptionGrenadierGrenadierCanVent")]
    public bool CanVent { get; set; } = true;
}