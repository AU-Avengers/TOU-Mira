using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class SwooperOptions : AbstractOptionGroup<SwooperRole>
{
    public override string GroupName => TouLocale.Get("TouRoleSwooper", "Swooper");

    [ModdedNumberOption("Swoop Uses Per Round", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxSwoops { get; set; } = 0f;

    [ModdedNumberOption("Swoop Cooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopCooldown { get; set; } = 25f;

    [ModdedNumberOption("Swoop Duration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopDuration { get; set; } = 10f;

    public ModdedEnumOption CanVent { get; set; } = new("Swooper Can Vent", (int)SwooperVent.Visible, typeof(SwooperVent),
        ["Never", "While Visible", "Always"]);
}

public enum SwooperVent
{
    Never,
    Visible,
    Always,
}