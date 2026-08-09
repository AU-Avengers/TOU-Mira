using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.GameModes;
using TownOfUs.Patches;

namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup, IWikiOptionsSummaryProvider
{
    public override Func<bool> GroupVisible => () => IsClassicRoleAssignment;
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
        var roleDist = (RoleSelectionMode)RoleAssignmentType.Value;
        if (CustomGameModeManager.IsHideNSeek() || GameOptionsManager.Instance.CurrentGameOptions.GameMode is AmongUs.GameOptions.GameModes.HideNSeek or AmongUs.GameOptions.GameModes.SeekFools)
        {
            return RoleDistribution.HideAndSeek;
        }

        if (CustomGameModeManager.IsActiveGameMode<CultistMode>())
        {
            return RoleDistribution.Cultist;
        }
        if (CustomGameModeManager.IsActiveGameMode<KillFrenzyMode>())
        {
            return RoleDistribution.KillFrenzy;
        }
        if (CustomGameModeManager.IsActiveGameMode<TownOfPolusMode>())
        {
            return RoleDistribution.TownOfPolus;
        }

        return roleDist switch
        {
            RoleSelectionMode.MinMaxList => RoleDistribution.MinMaxList,
            RoleSelectionMode.RoleList => RoleDistribution.RoleList,
            RoleSelectionMode.Draft => RoleDistribution.Draft,
            _ => RoleDistribution.Vanilla,
        };
    }

    public static bool IsClassicRoleAssignment
    {
        get
        {
            return CustomGameModeManager.IsClassic();
        }
    }

    public ModdedEnumOption RoleAssignmentType { get; } =
        new("Role Assignment Type", (int)RoleSelectionMode.RoleList, typeof(RoleSelectionMode), ["Vanilla", "Role List", "Min/Max List", "Draft"])
        {
            Visible = () => IsClassicRoleAssignment
        };

    public ModdedToggleOption LastImpostorBias { get; } =
        new("Reduce Impostor Streak", true)
        {
            Visible = () => IsClassicRoleAssignment && OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is not RoleDistribution.Vanilla and not RoleDistribution.Draft
        };

    public ModdedNumberOption ImpostorBiasPercent { get; } =
        new("Reduction Chance", 15f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.LastImpostorBias && IsClassicRoleAssignment && OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is not RoleDistribution.Vanilla and not RoleDistribution.Draft
        };

    // --- Draft Settings (Declared BEFORE Slots to fix wiki option ordering) ---
    private static bool IsDraft =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft;

    public ModdedEnumOption<DraftRecapMode> DraftRecap { get; } =
        new("Draft Recap Displays", DraftRecapMode.Faction)
        {
            Visible = () => IsDraft
        };

    public ModdedEnumOption<DraftRecapMode> DraftSidebarDisplay { get; } =
        new("Draft Sidebar Displays", DraftRecapMode.Faction)
        {
            Visible = () => IsDraft
        };

    public ModdedToggleOption UseRoleListForPool { get; set; } = new("Use Role List For Pool", false)
    {
        Visible = () => IsDraft
    };

    public ModdedNumberOption OfferedRolesCount { get; set; } = new("Offered Role Picks Per Turn", 3f, 1f, 9f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => IsDraft
    };

    public ModdedToggleOption ShowRandomOption { get; set; } = new("Show Random Role Pick", true)
    {
        Visible = () => IsDraft
    };

    public ModdedNumberOption TurnDurationSeconds { get; set; } = new("Turn Duration", 10f, 5f, 60f, 1f, MiraNumberSuffixes.Seconds, "0")
    {
        Visible = () => IsDraft
    };

    public ModdedNumberOption ConcurrentPicks { get; set; } = new("Concurrent Picks Per Turn", 1f, 1f, 2f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => IsDraft
    };

    public ModdedNumberOption ShufflesPerPlayer { get; set; } = new("Shuffles Per Player", 1f, 0f, 3f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => IsDraft
    };

    public bool RoleListEnabled => RoleAssignmentType.Value is (int)RoleSelectionMode.RoleList;

    // --- Min/Max Neutral Options ---
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

    // --- Slot Definitions (Declared LAST to keep summary output cleanly at the end) ---
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
    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            // These are hidden because rolelist text already handles this
            MaxNeutralBenign.StringName,
            MinNeutralBenign.StringName,
            MaxNeutralEvil.StringName,
            MinNeutralEvil.StringName,
            MaxNeutralKiller.StringName,
            MinNeutralKiller.StringName,
            MaxNeutralOutlier.StringName,
            MinNeutralOutlier.StringName,
            Slot1.StringName,
            Slot2.StringName,
            Slot3.StringName,
            Slot4.StringName,
            Slot5.StringName,
            Slot6.StringName,
            Slot7.StringName,
            Slot8.StringName,
            Slot9.StringName,
            Slot10.StringName,
            Slot11.StringName,
            Slot12.StringName,
            Slot13.StringName,
            Slot14.StringName,
            Slot15.StringName
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var currentDist = CurrentRoleDistribution();

        if (currentDist == RoleDistribution.Vanilla) { return Enumerable.Empty<string>(); }

        if (HudManagerPatches.RoleListTextComp == null || string.IsNullOrWhiteSpace(HudManagerPatches.RoleListTextComp.text))
        {
            return Enumerable.Empty<string>();
        }

        string roleListText = HudManagerPatches.RoleListTextComp.text;

        float sizePercent = 100f;
        const float minSizePercent = 35f; 
        const float sizeStep = 2.5f;

        int lineCount = roleListText.Split([ '\n', '\r' ], StringSplitOptions.RemoveEmptyEntries).Length;

        if (lineCount > 5)
        {
            sizePercent = Math.Max(minSizePercent, 100f - ((lineCount - 5) * sizeStep * 2.0f));
        }

        string formattedText = $"<page><size={sizePercent:0}%>{roleListText}</size>";

        return [ formattedText ];
    }
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
    Draft,
}

public enum RoleDistribution
{
    Vanilla,
    RoleList,
    MinMaxList,
    Draft,
    HideAndSeek,
    Cultist,
    KillFrenzy,
    TownOfPolus,
    // Legacy
}

public enum DraftRecapMode
{
    Nothing,
    Faction,
    Alignment,
    Role,
}

public enum RoleListOption
{
    CrewInvest,
    CrewKilling,
    CrewProtective,
    CrewPower,
    CrewSupport,

    CrewCommon,
    CrewSpecial,
    CrewRandom,

    NeutBenign,
    NeutEvil,
    NeutKilling,
    NeutOutlier,

    NeutCommon,
    NeutSpecial,
    NeutWildcard,
    NeutRandom,

    ImpConceal,
    ImpKilling,
    ImpPower,
    ImpSupport,

    ImpCommon,
    ImpSpecial,
    ImpRandom,

    NonImp,
    Any
}