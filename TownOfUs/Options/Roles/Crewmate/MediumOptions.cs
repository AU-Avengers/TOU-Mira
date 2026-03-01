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

    public ModdedToggleOption HidePlayersWhileMediating { get; set; } =
        new("TouOptionMediumHidePlayersWhileMediating", true);

    public ModdedNumberOption MediatingSpeed { get; set; } =
        new("TouOptionMediumMediatingSpeed", 3.5f, 1.5f, 10f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption LivingSeeSpiritTimer { get; set; } =
        new("TouOptionMediumLivingSeeSpiritTimer", 2.5f, 0.5f, 20f, 0.5f, MiraNumberSuffixes.Seconds);
    public ModdedToggleOption RevealMediateAppearance { get; set; } =
        new("TouOptionMediumRevealAppearanceOfMediateTarget", true);

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