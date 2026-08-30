using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class GameMechanicOptions : AbstractOptionGroup
{
 public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.GameMechanics");
    public override uint GroupPriority => 1;

    /*[ModdedToggleOption("TouOptionHideNamesOutOfSight")]
    public bool HideNamesOutOfSight { get; set; } = true;*/

    [ModdedToggleOption("TouOptionCrewKillersContinue")]
    public bool CrewKillersContinue { get; set; } = true;

    [ModdedToggleOption("TouOptionAnonymousShields")]
    public bool AnonymousShields { get; set; } = false;

    public ModdedEnumOption CleanedBodiesAppearance { get; set; } =
        new("TouOptionCleanedBodiesAppearance", (int)BodyVitalsMode.Missing,
            typeof(BodyVitalsMode),
            [
                "TouOptionCleanedBodiesAppearanceEnumMissing",
                "TouOptionCleanedBodiesAppearanceEnumDead",
                "TouOptionCleanedBodiesAppearanceEnumDisconnected"
            ]);

    public ModdedEnumOption KillAnimationBackgroundColor { get; set; } =
        new("TouOptionKillAnimationBackgroundColor", (int)KillColor.Red,
            typeof(KillColor),
            [
                "TouOptionKillAnimationBackgroundColorEnumRed",
                "TouOptionKillAnimationBackgroundColorEnumFaction",
                "TouOptionKillAnimationBackgroundColorEnumRoleColor"
            ]);

    public ModdedNumberOption PlayerCountWhenVentsDisable { get; set; } =
        new("TouOptionPlayerCountWhenVentsDisable",
            2f, 1f, 15f, 1f, MiraNumberSuffixes.None, "0.#");

    public ModdedToggleOption GhostwalkerFixSabos { get; set; } =
        new("TouOptionGhostwalkerFixSabos", false);

    [ModdedNumberOption("TouOptionTempSaveCdReset", 0f, 15f, 0.5f,
        MiraNumberSuffixes.Seconds, "0.#")]
    public float TempSaveCdReset { get; set; } = 5f;

    public ModdedNumberOption FullSaveCdMultiplier { get; set; } =
        new("TouOptionFullSaveCdMultiplier",
            0.5f, 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.#");
}

public enum BodyVitalsMode
{
    Missing,
    Dead,
    Disconnected
}

public enum KillColor
{
    Red,
    Faction,
    RoleColor
}