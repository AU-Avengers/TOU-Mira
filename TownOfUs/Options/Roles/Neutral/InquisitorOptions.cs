using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class InquisitorOptions : AbstractOptionGroup<InquisitorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleInquisitor", "Inquisitor");

    [ModdedNumberOption("TouOptionInquisitorVanquishCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VanquishCooldown { get; set; } = 25f;

    [ModdedToggleOption("TouOptionInquisitorRoundOneVanquish")]
    public bool FirstRoundUse { get; set; } = false;

    [ModdedToggleOption("TouOptionInquisitorContinuesGame")]
    public bool StallGame { get; set; } = true;

    [ModdedToggleOption("TouOptionInquisitorCantInquire")]
    public bool CantInquire { get; set; } = false;

    public ModdedNumberOption InquireCooldown { get; set; } =
        new("TouOptionInquisitorInquireCooldown", 25f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => !OptionGroupSingleton<InquisitorOptions>.Instance.CantInquire
        };

    public ModdedNumberOption MaxUses { get; set; } =
        new("TouOptionInquisitorMaxInquiries", 5f, 1f, 15f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<InquisitorOptions>.Instance.CantInquire
        };

    public ModdedNumberOption AmountOfHeretics { get; set; } =
        new("TouOptionInquisitorHereticAmount", 3f, 3f, 5f, 1f, MiraNumberSuffixes.None, "0");
}