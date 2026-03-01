using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MediumOptions : AbstractOptionGroup<MediumRole>
{
    public override string GroupName => TouLocale.Get("TouRoleMedium", "Medium");

    public ModdedNumberOption MediateCooldown { get; set; } =
        new("TouOptionMediumMediateCooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MediateDuration { get; set; } =
        new("TouOptionMediumMediateDuration", 15f, 5f, 20f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MediatingSpeed { get; set; } =
        new("TouOptionMediumMediatingSpeed", 3.5f, 1.5f, 10f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption LivingSeeSpiritTimer { get; set; } =
        new("TouOptionMediumLivingSeeSpiritTimer", 2.5f, 0.5f, 20f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption PlayerVisibility { get; set; } = new("TouOptionMediumAppearanceVisibility",
        (int)AppearanceVisibility.Both, typeof(AppearanceVisibility),
        [
            "TouOptionMediumAppearanceEnumLiving", "TouOptionMediumAppearanceEnumDead",
            "TouOptionMediumAppearanceEnumBoth", "TouOptionMediumAppearanceEnumNone"
        ]);

    public ModdedEnumOption ArrowVisibility { get; set; } = new("TouOptionMediumArrowVisibility",
        (int)MediumVisibility.Both, typeof(MediumVisibility),
        [
            "TouOptionMediumArrowEnumShowMedium", "TouOptionMediumArrowEnumShowMediated",
            "TouOptionMediumArrowEnumBoth", "TouOptionMediumArrowEnumNone"
        ]);

    public ModdedEnumOption WhoIsRevealed { get; set; } = new("TouOptionMediumWhoisRevealed",
        (int)MediateRevealedTargets.OldestDead, typeof(MediateRevealedTargets),
        [
            "TouOptionMediumGhostEnumOldestDead", "TouOptionMediumGhostEnumNewestDead",
            "TouOptionMediumGhostEnumRandomDead", "TouOptionMediumGhostEnumAllDead"
        ]);
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

public enum AppearanceVisibility
{
    Living,
    Ghosts,
    Both,
    None
}