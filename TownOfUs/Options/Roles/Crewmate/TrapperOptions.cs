using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class TrapperOptions : AbstractOptionGroup<TrapperRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Trapper, "Trapper");

    [ModdedNumberOption("Trap Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float TrapCooldown { get; set; } = 20f;

    [ModdedNumberOption("Min Amount Of Time In Trap To Register", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float MinAmountOfTimeInTrap { get; set; } = 5f;

    [ModdedNumberOption("Max Number Of Traps", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxTraps { get; set; } = 5f;

    [ModdedNumberOption("Trap Size", 0.05f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float TrapSize { get; set; } = 0.25f;

    [ModdedToggleOption("Traps Removed After Each Round")]
    public bool TrapsRemoveOnNewRound { get; set; } = true;

    public ModdedToggleOption TaskUses { get; } = new("Get More Uses From Completing Tasks", false)
    {
        Visible = () => !OptionGroupSingleton<TrapperOptions>.Instance.TrapsRemoveOnNewRound
    };

    [ModdedNumberOption("Minimum Number Of Roles Required To Trigger Trap", 1f, 15f)]
    public float MinAmountOfPlayersInTrap { get; set; } = 3f;
}