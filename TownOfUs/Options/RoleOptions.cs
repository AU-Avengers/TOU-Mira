using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Utilities;

namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup
{
    internal static string[] OptionStrings =
    [
        MiscUtils.GetParsedRoleBucket("CommonCrew"),
        MiscUtils.GetParsedRoleBucket("RandomCrew"),
        MiscUtils.GetParsedRoleBucket("CrewInvestigative"),
        MiscUtils.GetParsedRoleBucket("CrewKilling"),
        MiscUtils.GetParsedRoleBucket("CrewProtective"),
        MiscUtils.GetParsedRoleBucket("CrewPower"),
        MiscUtils.GetParsedRoleBucket("CrewSupport"),
        MiscUtils.GetParsedRoleBucket("SpecialCrew"),
        MiscUtils.GetParsedRoleBucket("NonImp"),
        MiscUtils.GetParsedRoleBucket("CommonNeutral"),
        MiscUtils.GetParsedRoleBucket("SpecialNeutral"),
        MiscUtils.GetParsedRoleBucket("WildcardNeutral"),
        MiscUtils.GetParsedRoleBucket("RandomNeutral"),
        MiscUtils.GetParsedRoleBucket("NeutralBenign"),
        MiscUtils.GetParsedRoleBucket("NeutralEvil"),
        MiscUtils.GetParsedRoleBucket("NeutralKilling"),
        MiscUtils.GetParsedRoleBucket("NeutralOutlier"),
        MiscUtils.GetParsedRoleBucket("CommonImp"),
        MiscUtils.GetParsedRoleBucket("RandomImp"),
        MiscUtils.GetParsedRoleBucket("ImpConcealing"),
        MiscUtils.GetParsedRoleBucket("ImpKilling"),
        MiscUtils.GetParsedRoleBucket("ImpPower"),
        MiscUtils.GetParsedRoleBucket("ImpSupport"),
        MiscUtils.GetParsedRoleBucket("SpecialImp"),
        MiscUtils.GetParsedRoleBucket("Any")
    ];

    public override string GroupName => "Role Settings";
    public override uint GroupPriority => 2;

    [ModdedToggleOption("Reduce Impostor Streak")]
    public bool LastImpostorBias { get; set; } = false;

    public ModdedNumberOption ImpostorBiasPercent { get; } =
        new("Reduction Chance", 15f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.LastImpostorBias
        };

    [ModdedToggleOption("Role List Enabled")]
    public bool RoleListEnabled { get; set; } = true;

    public ModdedEnumOption Slot1 { get; } =
        new("Slot 1", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot2 { get; } =
        new("Slot 2", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot3 { get; } =
        new("Slot 3", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot4 { get; } =
        new("Slot 4", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot5 { get; } =
        new("Slot 5", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot6 { get; } =
        new("Slot 6", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot7 { get; } =
        new("Slot 7", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot8 { get; } =
        new("Slot 8", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot9 { get; } =
        new("Slot 9", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot10 { get; } =
        new("Slot 10", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot11 { get; } =
        new("Slot 11", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot12 { get; } =
        new("Slot 12", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot13 { get; } =
        new("Slot 13", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot14 { get; } =
        new("Slot 14", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedEnumOption Slot15 { get; } =
        new("Slot 15", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MinNeutralBenign { get; } =
        new("Min Neutral Benign", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MaxNeutralBenign { get; } =
        new("Max Neutral Benign", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MinNeutralEvil { get; } =
        new("Min Neutral Evil", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MaxNeutralEvil { get; } =
        new("Max Neutral Evil", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MinNeutralKiller { get; } =
        new("Min Neutral Killer", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MaxNeutralKiller { get; } =
        new("Max Neutral Killer", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MinNeutralOutlier { get; } =
        new("Min Neutral Outliers", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };

    public ModdedNumberOption MaxNeutralOutlier { get; } =
        new("Max Neutral Outliers", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
        };
}

public enum RoleListOption
{
    CrewCommon,
    CrewRandom,
    CrewInvest,
    CrewKilling,
    CrewProtective,
    CrewPower,
    CrewSupport,
    CrewSpecial,
    NonImp,
    NeutCommon,
    NeutSpecial,
    NeutWildcard,
    NeutRandom,
    NeutBenign,
    NeutEvil,
    NeutKilling,
    NeutOutlier,
    ImpCommon,
    ImpRandom,
    ImpConceal,
    ImpKilling,
    ImpPower,
    ImpSupport,
    ImpSpecial,
    Any
}