using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class TraitorOptions : AbstractOptionGroup<TraitorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleTraitor", "Traitor");

    [ModdedNumberOption("Minimum People Alive When Traitor Can Spawn", 3f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float LatestSpawn { get; set; } = 5f;

    [ModdedToggleOption("Traitor Won't Spawn If NK Is Alive")]
    public bool NeutralKillingStopsTraitor { get; set; } = false;

    [ModdedToggleOption("Disable Existing Impostor Roles")]
    public bool RemoveExistingRoles { get; set; } = false;

    public ModdedEnumOption TraitorGuess { get; set; } = new("Traitor Must Be Guessed As", (int)CacheRoleGuess.ActiveOrCachedRole, typeof(CacheRoleGuess), ["Traitor", "New Role", "Traitor or New Role"]);
}