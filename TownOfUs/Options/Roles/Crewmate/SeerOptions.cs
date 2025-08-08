using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SeerOptions : AbstractOptionGroup<SeerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Seer, "Seer");

    [ModdedNumberOption("Compare Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SeerCooldown { get; set; } = 25f;
    [ModdedNumberOption("Max Uses of Compare", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxCompares { get; set; } = 0f;
    [ModdedToggleOption("Neutral Benign Show Friends To All")]
    public bool BenignShowFriendlyToAll { get; set; } = false;
    [ModdedToggleOption("Neutral Evil Show Friends To All")]
    public bool EvilShowFriendlyToAll { get; set; } = false;
}