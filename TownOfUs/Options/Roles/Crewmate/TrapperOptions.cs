using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class RevealerOptions : AbstractOptionGroup<RevealerRole>
{
    public override string GroupName => TouLocale.Get("TouRoleRevealer", "Revealer");

    [ModdedNumberOption("TouOptionRevealerTrapCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float TrapCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionRevealerMinAmountOfTimeInTrap", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float MinAmountOfTimeInTrap { get; set; } = 5f;

    [ModdedNumberOption("TouOptionRevealerMaxNumberOfTraps", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxTraps { get; set; } = 5f;

    [ModdedNumberOption("TouOptionRevealerTrapSize", 0.05f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float TrapSize { get; set; } = 0.25f;

    [ModdedToggleOption("TouOptionRevealerTrapsRemovedAfterRound")]
    public bool TrapsRemoveOnNewRound { get; set; } = true;

    public ModdedToggleOption TaskUses { get; } = new("TouOptionRevealerGetUsesFromTasks", false)
    {
        Visible = () => !OptionGroupSingleton<RevealerOptions>.Instance.TrapsRemoveOnNewRound
    };

    [ModdedNumberOption("TouOptionRevealerMinimumNumberOfRoles", 1f, 15f)]
    public float MinAmountOfPlayersInTrap { get; set; } = 3f;
}