using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ImitatorOptions : AbstractOptionGroup<ImitatorRole>
{
    public override string GroupName => TouLocale.Get("TouRoleImitator", "Imitator");

    public ModdedToggleOption ImitateNeutrals { get; set; } = new("TouOptionImitatorImitateNeutrals", true);

    public ModdedToggleOption ImitateImpostors { get; set; } = new("TouOptionImitatorImitateImpostors", true);

    public ModdedToggleOption ImitateBasicCrewmate { get; set; } = new("TouOptionImitatorImitateBasicCrewmate", true);

    public ModdedEnumOption ImitatorGuess { get; set; } = new("TouOptionImitatorGuessAs",
        (int)CacheRoleGuess.ActiveOrCachedRole, typeof(CacheRoleGuess),
        [
            "TouOptionImitatorGuessEnumCached", "TouOptionImitatorGuessEnumActive",
            "TouOptionImitatorGuessEnumActiveOrCached"
        ]);
}