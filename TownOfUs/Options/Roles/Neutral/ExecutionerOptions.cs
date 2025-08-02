using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class ExecutionerOptions : AbstractOptionGroup<ExecutionerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Executioner, "Executioner");

    [ModdedEnumOption("On Target Death, Executioner Becomes", typeof(BecomeOptions))]
    public BecomeOptions OnTargetDeath { get; set; } = BecomeOptions.Jester;

    [ModdedToggleOption("Executioner Can Button")]
    public bool CanButton { get; set; } = true;

    [ModdedEnumOption("Executioner Win", typeof(ExeWinOptions), ["Ends Game", "Leaves & Torments", "Nothing"])]
    public ExeWinOptions ExeWin { get; set; } = ExeWinOptions.Torments;
}

public enum ExeWinOptions
{
    EndsGame,
    Torments,
    Nothing
}