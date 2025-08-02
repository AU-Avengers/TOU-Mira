using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MediumOptions : AbstractOptionGroup<MediumRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Medium, "Medium");

    [ModdedNumberOption("Mediate Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MediateCooldown { get; set; } = 10;

    [ModdedToggleOption("Reveal Appearance of Mediate Target")]
    public bool RevealMediateAppearance { get; set; } = true;

    [ModdedEnumOption("Arrow Visibility", typeof(MediumVisibility),
        ["Medium", "Mediated", "Medium + Mediated", "Neither"])]
    public MediumVisibility ArrowVisibility { get; set; } = MediumVisibility.Both;

    [ModdedEnumOption("Who is Revealed", typeof(MediateRevealedTargets),
        ["Oldest Dead", "Newest Dead", "Random Dead", "All Dead"])]
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