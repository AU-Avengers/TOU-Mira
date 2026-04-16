using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class DictatorOptions : AbstractOptionGroup<DictatorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleDictator", "Dictator");

    [ModdedNumberOption("TouOptionDictatorInfluenceCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InfluenceCooldown { get; set; } = 20f;

    [ModdedToggleOption("TouOptionDictatorCanStealMayorVotes")]
    public bool CanStealMayorVotes { get; set; } = false;

    [ModdedNumberOption("TouOptionDictatorMinNumberOfPlayersToInfluence", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MinNumberOfPlayersToInfluenceBeforeCoercing { get; set; } = 2f;

    [ModdedNumberOption("TouOptionDictatorMaxNumberOfPlayersToInfluence", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxNumberOfPlayersToInfluence { get; set; } = 3f;
}
