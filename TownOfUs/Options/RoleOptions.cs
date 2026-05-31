using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup
{
    public override Func<bool> GroupVisible => () =>
        MiscUtils.CurrentGamemode() is TouGamemode.Classic;
    internal static string[] OptionStrings =
    [
        MiscUtils.GetParsedRoleBucket("CrewInvestigative"),
        MiscUtils.GetParsedRoleBucket("CrewKilling"),
        MiscUtils.GetParsedRoleBucket("CrewProtective"),
        MiscUtils.GetParsedRoleBucket("CrewPower"),
        MiscUtils.GetParsedRoleBucket("CrewSupport"),

        MiscUtils.GetParsedRoleBucket("CommonCrew"),
        MiscUtils.GetParsedRoleBucket("SpecialCrew"),
        MiscUtils.GetParsedRoleBucket("RandomCrew"),

        MiscUtils.GetParsedRoleBucket("NeutralBenign"),
        MiscUtils.GetParsedRoleBucket("NeutralEvil"),
        MiscUtils.GetParsedRoleBucket("NeutralKilling"),
        MiscUtils.GetParsedRoleBucket("NeutralOutlier"),

        MiscUtils.GetParsedRoleBucket("CommonNeutral"),
        MiscUtils.GetParsedRoleBucket("SpecialNeutral"),
        MiscUtils.GetParsedRoleBucket("WildcardNeutral"),
        MiscUtils.GetParsedRoleBucket("RandomNeutral"),

        MiscUtils.GetParsedRoleBucket("ImpConcealing"),
        MiscUtils.GetParsedRoleBucket("ImpKilling"),
        MiscUtils.GetParsedRoleBucket("ImpPower"),
        MiscUtils.GetParsedRoleBucket("ImpSupport"),

        MiscUtils.GetParsedRoleBucket("CommonImp"),
        MiscUtils.GetParsedRoleBucket("SpecialImp"),
        MiscUtils.GetParsedRoleBucket("RandomImp"),

        MiscUtils.GetParsedRoleBucket("NonImp"),
        MiscUtils.GetParsedRoleBucket("Any")
    ];

    public override string GroupName => "Role Settings";
    public override uint GroupPriority => 2;

    public RoleDistribution CurrentRoleDistribution()
    {
        var gameMode = MiscUtils.CurrentGamemode();
        var roleDist = (RoleSelectionMode)RoleAssignmentType.Value;

        switch (gameMode)
        {
            case TouGamemode.HideAndSeek:
                return RoleDistribution.HideAndSeek;
            case TouGamemode.Cultist:
                return RoleDistribution.Cultist;
            case TouGamemode.KillFrenzy:
                return RoleDistribution.KillFrenzy;
        }

        switch (roleDist)
        {
            case RoleSelectionMode.MinMaxList:
                return RoleDistribution.MinMaxList;
            case RoleSelectionMode.RoleList:
                return RoleDistribution.RoleList;
        }

        return RoleDistribution.Vanilla;
    }

    public bool IsClassicRoleAssignment => MiscUtils.CurrentGamemode() is TouGamemode.Classic;
    public ModdedEnumOption RoleAssignmentType { get; } =
        new("Role Assignment Type", (int)RoleSelectionMode.RoleList, typeof(RoleSelectionMode), ["Vanilla", "Role List", "Min/Max List"])
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment
        };

    public ModdedToggleOption LastImpostorBias { get; } =
        new("Reduce Impostor Streak", true)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment && OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is not RoleDistribution.Vanilla
        };

    public ModdedNumberOption ImpostorBiasPercent { get; } =
        new("Reduction Chance", 15f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.LastImpostorBias && OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment && OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is not RoleDistribution.Vanilla
        };

    public bool RoleListEnabled => RoleAssignmentType.Value is (int)RoleSelectionMode.RoleList;
    /*public ModdedEnumOption GuaranteedKiller { get; } =
        new("Guaranteed Killer", (int)RequiredKiller.ImpostorOrNeutralKiller, typeof(RequiredKiller), ["Impostor", "Neutral Killer", "Impostor or Neutral Killer"])
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };*/

    /*public ModdedStringOption SlotCustom { get; } =
        new("Custom Slot", HudManagerPatches.StoredRoleBuckets[0], HudManagerPatches.StoredRoleBuckets.ToArray())
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };*/

    public ModdedEnumOption<RoleListOption> Slot1 { get; } =
        new("Slot 1", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot2 { get; } =
        new("Slot 2", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot3 { get; } =
        new("Slot 3", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot4 { get; } =
        new("Slot 4", RoleListOption.ImpCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot5 { get; } =
        new("Slot 5", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot6 { get; } =
        new("Slot 6", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot7 { get; } =
        new("Slot 7", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot8 { get; } =
        new("Slot 8", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot9 { get; } =
        new("Slot 9", RoleListOption.ImpCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot10 { get; } =
        new("Slot 10", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot11 { get; } =
        new("Slot 11", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot12 { get; } =
        new("Slot 12", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot13 { get; } =
        new("Slot 13", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot14 { get; } =
        new("Slot 14", RoleListOption.ImpCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption<RoleListOption> Slot15 { get; } =
        new("Slot 15", RoleListOption.CrewCommon, OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedNumberOption MinNeutralBenign { get; } =
        new("Min Neutral Benign", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };

    public ModdedNumberOption MaxNeutralBenign { get; } =
        new("Max Neutral Benign", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };

    public ModdedNumberOption MinNeutralEvil { get; } =
        new("Min Neutral Evil", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };

    public ModdedNumberOption MaxNeutralEvil { get; } =
        new("Max Neutral Evil", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };

    public ModdedNumberOption MinNeutralKiller { get; } =
        new("Min Neutral Killer", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };

    public ModdedNumberOption MaxNeutralKiller { get; } =
        new("Max Neutral Killer", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };

    public ModdedNumberOption MinNeutralOutlier { get; } =
        new("Min Neutral Outliers", 0f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };

    public ModdedNumberOption MaxNeutralOutlier { get; } =
        new("Max Neutral Outliers", 0f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.MinMaxList
        };
}

public enum RequiredKiller
{
    Impostor,
    NeutralKiller,
    ImpostorOrNeutralKiller,
}

public enum RoleSelectionMode
{
    Vanilla,
    RoleList,
    MinMaxList,
}

public enum RoleDistribution
{
    Vanilla,
    RoleList,
    MinMaxList,
    HideAndSeek,
    Cultist,
    KillFrenzy,
    // Legacy
}

public enum RoleListOption
{
    CrewInvest,
    CrewKilling,
    CrewProtective,
    CrewPower,
    CrewSupport,

    CrewCommon, // Investigative / Protective / Support
    CrewSpecial, // Killing / Power
    // CrewUtility, // Investigative / Support
    // CrewBasic, // Vanilla Crewmate
    CrewRandom, // Any Crewmate role

    NeutBenign,
    NeutEvil,
    NeutKilling,
    NeutOutlier,

    NeutCommon, // Benign / Evil
    NeutSpecial, // Killing / Outlier
    NeutWildcard, // Benign / Evil / Outlier
    // NeutChaos, // Evil / Outlier
    // NeutPassive, // Benign / Outlier, this name sucks btw - Atony
    NeutRandom, // Any Neutral role

    ImpConceal,
    ImpKilling,
    ImpPower,
    ImpSupport,

    ImpCommon, // Concealing / Support
    ImpSpecial, // Killing / Power
    // ImpUtility, // Concealing / Killing / Support
    // ImpBasic, // Vanilla Impostor
    ImpRandom, // Any Impostor role

    NonImp, // Crewmate / Neutral
    // NonKilling, // Everything but Impostors, NKs, and CKs
    // AnyKilling, // Impostors, NKs, and CKs
    Any
}