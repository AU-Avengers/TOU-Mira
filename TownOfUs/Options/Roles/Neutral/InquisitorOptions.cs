using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class InquisitorOptions : AbstractOptionGroup<InquisitorRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Inquisitor, "Inquisitor");

    [ModdedNumberOption("Vanquish Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VanquishCooldown { get; set; } = 25f;

    [ModdedToggleOption("Inquisitor Continues Game In Final 3")]
    public bool StallGame { get; set; } = true;

    [ModdedToggleOption("Inquisitor Can't Inquire")]
    public bool CantInquire { get; set; } = false;

    public ModdedNumberOption InquireCooldown { get; set; } =
        new("Inquire Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => !OptionGroupSingleton<InquisitorOptions>.Instance.CantInquire
        };

    public ModdedNumberOption MaxUses { get; set; } =
        new("Max Number Of Inquires", 5f, 1f, 15f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<InquisitorOptions>.Instance.CantInquire
        };

    public ModdedNumberOption AmountOfHeretics { get; set; } =
        new("Amount of Heretics Needed", 3f, 3f, 5f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<InquisitorOptions>.Instance.CantInquire
        };
}