using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class AmnesiacOptions : AbstractRoleOptionGroup<AmnesiacRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Amnesiac", "Amnesiac");

    [ModdedToggleOption("TouOptionAmnesiacInheritFactionModifier")]
    public bool InheritFactionModifier { get; set; } = true;

    [ModdedToggleOption("TouOptionAmnesiacShowArrows")]
    public bool RememberArrows { get; set; } = true;

    public ModdedNumberOption RememberArrowDelay { get; } = new("TouOptionAmnesiacArrowDelay", 5f, 0f, 15f, 1f,
        MiraNumberSuffixes.Seconds, "0")
    {
        Visible = () => OptionGroupSingleton<AmnesiacOptions>.Instance.RememberArrows
    };

    public ModdedEnumOption<AssassinRemember> AmneTurnImpAssassin { get; } = 
        new($"TouOptionAmnesiacAssassinImpostor", AssassinRemember.IfAssassin,
            ["TouOptionAmnesiacAssassinEnumNever", "TouOptionAmnesiacAssassinEnumDependentImp", "TouOptionAmnesiacAssassinEnumAlways"]);

    public ModdedEnumOption<AssassinRemember> AmneTurnNeutAssassin { get; } =
        new($"TouOptionAmnesiacAssassinNeutral", AssassinRemember.Always,
            ["TouOptionAmnesiacAssassinEnumNever", "TouOptionAmnesiacAssassinEnumDependentNeut", "TouOptionAmnesiacAssassinEnumAlways"]);
}

public enum AssassinRemember
{
    Never,
    IfAssassin,
    Always
}