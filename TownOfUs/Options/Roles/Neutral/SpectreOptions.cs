using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SpectreOptions : AbstractRoleOptionGroup<SpectreRole>
{
    public override string GroupName => MiraLocaleManager.Get("TouRoleSpectre", "Spectre");

    [ModdedNumberOption("TouOptionSpectreTasksLeftClickable", 1, 15)]
    public float NumTasksLeftBeforeClickable { get; set; } = 3f;

    [ModdedEnumOption("TouOptionSpectreWin", typeof(SpectreWinOptions), ["TouOptionSpectreWinEnumEndsGame", "TouOptionSpectreWinEnumSpooks", "TouOptionSpectreWinEnumNothing"])]
    public SpectreWinOptions SpectreWin { get; set; } = SpectreWinOptions.Nothing;
}

public enum SpectreWinOptions
{
    EndsGame,
    Spooks,
    Nothing
}