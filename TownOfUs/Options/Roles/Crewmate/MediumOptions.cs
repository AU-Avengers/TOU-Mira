using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MediumOptions : AbstractOptionGroup<MediumRole>
{
    public override string GroupName => TouLocale.Get("TouRoleMedium", "Medium");

    [ModdedNumberOption("TouOptionMediumMediateCooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MediateCooldown { get; set; } = 10;

    [ModdedToggleOption("TouOptionMediumRevealAppearanceOfMediateTarget")]
    public bool RevealMediateAppearance { get; set; } = true;

    [ModdedEnumOption("TouOptionMediumArrowVisibility", typeof(MediumVisibility),
        ["TouOptionMediumArrowEnumShowMedium", "TouOptionMediumArrowEnumShowMediated", "TouOptionMediumArrowEnumBoth", "TouOptionMediumArrowEnumNone"])]
    public MediumVisibility ArrowVisibility { get; set; } = MediumVisibility.Both;

    [ModdedEnumOption("TouOptionMediumWhoisRevealed", typeof(MediateRevealedTargets),
        ["TouOptionMediumGhostEnumOldestDead", "TouOptionMediumGhostEnumNewestDead", "TouOptionMediumGhostEnumRandomDead", "TouOptionMediumGhostEnumAllDead"])]
    public MediateRevealedTargets WhoIsRevealed { get; set; } = MediateRevealedTargets.OldestDead;
}

public enum MediateRevealedTargets
{
    OldestDead,
    NewestDead,
    RandomDead,
    AllDead
}

public enum MediumVisibility
{
    ShowMedium,
    ShowMediate,
    Both,
    None
}