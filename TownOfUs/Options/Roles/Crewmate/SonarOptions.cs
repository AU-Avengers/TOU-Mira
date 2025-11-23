using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SonarOptions : AbstractOptionGroup<SonarRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSonar", "Sonar");

    [ModdedNumberOption("TouOptionSonarTrackCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float TrackCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionSonarMaxNumberOfTracks", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxTracks { get; set; } = 5f;

    [ModdedNumberOption("TouOptionSonarArrowUpdateInterval", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float UpdateInterval { get; set; } = 5f;

    [ModdedToggleOption("TouOptionSonarArrowsMakeSoundOnDeath")]
    public bool SoundOnDeactivate { get; set; } = true;

    [ModdedToggleOption("TouOptionSonarArrowsResetAfterEachRound")]
    public bool ResetOnNewRound { get; set; } = true;

    public ModdedToggleOption TaskUses { get; } = new("TouOptionSonarTaskUses", false)
    {
        Visible = () => !OptionGroupSingleton<SonarOptions>.Instance.ResetOnNewRound
    };
}