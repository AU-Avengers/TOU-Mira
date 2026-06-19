using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class ExecutionerOptions : AbstractOptionGroup<ExecutionerRole>
{
    public override string GroupName => TouLocale.Get("TouRoleExecutioner", "Executioner");

    [ModdedEnumOption("TouOptionExecutionerBecomesTargetDeath", typeof(BecomeOptions), ["CrewmateKeyword", "TouRoleAmnesiac", "TouRoleSurvivor", "TouRoleMercenary", "TouRoleJester"])]
    public BecomeOptions OnTargetDeath { get; set; } = BecomeOptions.Jester;

    [ModdedToggleOption("Executioner Can Button")]
    public bool CanButton { get; set; } = true;

    [ModdedEnumOption("Executioner Win", typeof(ExeWinOptions), ["Ends Game", "Leaves & Torments", "Nothing"])]
    public ExeWinOptions ExeWin { get; set; } = ExeWinOptions.Torments;

    public ModdedToggleOption ExeAnonymizeWin { get; set; } =
        new("TouOptionExecutionerNotifyWin", true)
    {
        Visible = () => OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is not ExeWinOptions.EndsGame
    };
}

public enum ExeWinOptions
{
    EndsGame,
    Torments,
    Nothing
}