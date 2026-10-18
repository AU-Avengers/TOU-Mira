using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class TraitorOptions : AbstractRoleOptionGroup<TraitorRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Traitor", "Traitor");

    [ModdedNumberOption("TouOptionTraitorMinimumPeopleAlive", 3f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float LatestSpawn { get; set; } = 5f;

    [ModdedToggleOption("TouOptionTraitorTraitorWontSpawnIfNKIsAlive")]
    public bool NeutralKillingStopsTraitor { get; set; } = false;

    [ModdedToggleOption("TouOptionTraitorDisableExistingImpostorRoles")]
    public bool RemoveExistingRoles { get; set; } = true;

    public ModdedEnumOption TraitorGuess { get; set; } = new("TouOptionTraitorGuessAs", (int)CacheRoleGuess.ActiveOrCachedRole, typeof(CacheRoleGuess), ["TouOptionTraitorGuessEnumCached", "TouOptionTraitorGuessEnumActive", "TouOptionTraitorGuessEnumActiveOrCached"]);

    public ModdedToggleOption TraitorCanAssassin { get; } =
        new("TouOptionTraitorBecomesAssassin", true);
}