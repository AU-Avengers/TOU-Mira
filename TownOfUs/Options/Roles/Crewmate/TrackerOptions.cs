using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class TrackerOptions : AbstractOptionGroup<TrackerTouRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Tracker, "Tracker");

    [ModdedNumberOption("Track Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float TrackCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Number Of Tracks", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxTracks { get; set; } = 5f;

    [ModdedNumberOption("Arrow Update Interval", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float UpdateInterval { get; set; } = 5f;

    [ModdedToggleOption("Tracker Arrows Make Sound On Death")]
    public bool SoundOnDeactivate { get; set; } = true;

    [ModdedToggleOption("Tracker Arrows Reset After Each Round")]
    public bool ResetOnNewRound { get; set; } = true;

    public ModdedToggleOption TaskUses { get; } = new("Get More Uses From Completing Tasks", false)
    {
        Visible = () => !OptionGroupSingleton<TrackerOptions>.Instance.ResetOnNewRound
    };
}