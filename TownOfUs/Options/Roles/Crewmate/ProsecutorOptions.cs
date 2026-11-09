using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ProsecutorOptions : AbstractRoleOptionGroup<ProsecutorRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Prosecutor", "Prosecutor");

    public ModdedEnumOption<BadProsecuteResult> WrongfulProsecutionResult { get; } =
        new("TouOptionProsecutorWrongfulProsecutionResult", BadProsecuteResult.EjectPros,
            ["TouOptionProsecutorWrongProsEnumPros", "TouOptionProsecutorWrongProsEnumTarget", "TouOptionProsecutorWrongProsEnumProsTarget"]);

    [ModdedNumberOption("TouOptionProsecutorMaxProsecutions", 1, 5)]
    public float MaxProsecutions { get; set; } = 2f;
}

public enum BadProsecuteResult
{
    EjectPros,
    LoseUsesAndEjectTarget,
    EjectProsAndEjectTarget
}
