using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class ChefOptions : AbstractOptionGroup<ChefRole>
{
    public override string GroupName => TouLocale.Get("TouRoleChef", "Chef");

    [ModdedNumberOption("Cook Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CookCooldown { get; set; } = 25f;

    [ModdedNumberOption("Serve Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ServeCooldown { get; set; } = 25f;

    [ModdedToggleOption("Reset Cook & Serve Cooldowns Together")]
    public bool ResetCooldowns { get; set; } = true;

    [ModdedNumberOption("Amount Of Servings Needed", 2f, 5f)]
    public float ServingsNeeded { get; set; } = 3f;

    public ModdedNumberOption SideEffectDuration { get; set; } =
        new("Duration of Side Effects", 60f, 0f, 120f, 10f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Show Arrows Pointing To Dead Bodies")]
    public bool ChefArrows { get; set; } = true;

    public ModdedNumberOption ChefArrowDelay { get; set; } =
        new("Dead Body Arrow Delay", 0.5f, 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds, "0.0")
        {
            Visible = () => OptionGroupSingleton<ChefOptions>.Instance.ChefArrows
        };

    public ModdedNumberOption ChefArrowDuration { get; set; } =
        new("Dead Body Arrow Duration", 10f, 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds, "0.0", zeroInfinity: true)
        {
            Visible = () => OptionGroupSingleton<ChefOptions>.Instance.ChefArrows
        };
}