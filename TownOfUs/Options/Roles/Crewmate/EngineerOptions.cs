using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class EngineerOptions : AbstractOptionGroup<EngineerTouRole>
{
    public override string GroupName => TouLocale.Get("TouRoleEngineer", "Engineer");
    public ModdedNumberOption MaxVents { get; } = new("TouOptionEngineerMaxVents", -1f, -1f, 30f, 1f, "0", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption VentPerTasks { get; } = new("TouOptionEngineerVentPerTasks", 1f, 0f, 15f, 1f, "</b>Off<b>", "#", MiraNumberSuffixes.None, "0")
    {
        Visible = () => OptionGroupSingleton<EngineerOptions>.Instance.MaxVents != -1
    };

    [ModdedNumberOption("TouOptionEngineerVentCooldown", 0f, 25f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VentCooldown { get; set; } = 15f;

    [ModdedNumberOption("TouOptionEngineerVentDuration", 0f, 25f, 5f, MiraNumberSuffixes.Seconds, zeroInfinity: true)]
    public float VentDuration { get; set; } = 10f;

    public ModdedNumberOption MaxFixes { get; } = new("TouOptionEngineerMaxFixes", 2f, -1f, 15f, 1f, "0", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption FixPerTasks { get; } = new("TouOptionEngineerFixPerTasks", 3f, 0f, 15f, 1f, "</b>Off<b>", "#", MiraNumberSuffixes.None, "0")
    {
        Visible = () => OptionGroupSingleton<EngineerOptions>.Instance.MaxFixes != -1
    };

    public ModdedNumberOption FixDelay { get; } = new("TouOptionEngineerFixDelay", 0.5f, 0f, 5f, 0.5f, MiraNumberSuffixes.Seconds);
}