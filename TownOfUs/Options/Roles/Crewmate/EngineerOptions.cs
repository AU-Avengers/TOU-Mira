using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class EngineerOptions : AbstractOptionGroup<EngineerTouRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Engineer, "Engineer");

    [ModdedNumberOption("Vent Uses Per Game", 0f, 30f, 5f, MiraNumberSuffixes.None, "0", true)]
    public float MaxVents { get; set; } = 0f;

    public ModdedToggleOption TaskUses { get; } = new("Get More Vent Uses From Completing Tasks", true)
    {
        Visible = () => OptionGroupSingleton<EngineerOptions>.Instance.MaxVents != 0
    };

    [ModdedNumberOption("Vent Cooldown", 0f, 25f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VentCooldown { get; set; } = 15f;

    [ModdedNumberOption("Vent Duration", 0f, 25f, 5f, MiraNumberSuffixes.Seconds, zeroInfinity: true)]
    public float VentDuration { get; set; } = 10f;

    [ModdedNumberOption("Fix Uses Per Game", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxFixes { get; set; } = 5f;

    [ModdedNumberOption("Fix Delay", 0f, 5f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float FixDelay { get; set; } = 0.5f;
}