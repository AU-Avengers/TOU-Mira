using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MediumOptions : AbstractRoleOptionGroup<MediumRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Medium", "Medium");

    public ModdedNumberOption MediateCooldown { get; set; } =
        new("TouOptionMediumMediateCooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MediateDuration { get; set; } =
        new("TouOptionMediumMediateDuration", 15f, 5f, 20f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MediatingSpeed { get; set; } =
        new("TouOptionMediumMediatingSpeed", 3.5f, 1.5f, 10f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption LivingSeeSpiritTimer { get; set; } =
        new("TouOptionMediumLivingSeeSpiritTimer", 2.5f, 0.5f, 20f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption<AppearanceVisibility> PlayerVisibility { get; set; } = new("TouOptionMediumAppearanceVisibility",
        AppearanceVisibility.None,
        [
            "TouOptionMediumAppearanceEnumLiving", "TouOptionMediumAppearanceEnumDead",
            "TouOptionMediumAppearanceEnumBoth", "TouOptionMediumAppearanceEnumNone"
        ]);

    public ModdedEnumOption<MediumVisibility> ArrowVisibility { get; set; } = new("TouOptionMediumArrowVisibility",
        MediumVisibility.Both,
        [
            "TouOptionMediumArrowEnumShowMedium", "TouOptionMediumArrowEnumShowMediated",
            "TouOptionMediumArrowEnumBoth", "TouOptionMediumArrowEnumNone"
        ]);

    public ModdedEnumOption<MediateRevealedTargets> WhoIsRevealed { get; set; } = new("TouOptionMediumWhoisRevealed",
        MediateRevealedTargets.OldestDead,
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