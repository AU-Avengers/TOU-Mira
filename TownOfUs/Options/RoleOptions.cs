using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
        "CrewInvestigative.Colored",
        "CrewKilling.Colored",
        "CrewProtective.Colored",
        "CrewPower.Colored",
        "CrewSupport.Colored",

        "CommonCrew.Colored",
        "SpecialCrew.Colored",
        "RandomCrew.Colored",

        "NeutralBenign.Colored",
        "NeutralEvil.Colored",
        "NeutralKilling.Colored",
        "NeutralOutlier.Colored",

        "CommonNeutral.Colored",
        "SpecialNeutral.Colored",
        "WildcardNeutral.Colored",
        "RandomNeutral.Colored",

        "ImpConcealing.Colored",
        "ImpKilling.Colored",
        "ImpPower.Colored",
        "ImpSupport.Colored",

        "CommonImp.Colored",
        "SpecialImp.Colored",
        "RandomImp.Colored",

        "NonImp.Colored",
        "Any"
    ];

    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.RoleSettings");
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
        new("TouOptionRoleAssignmentType", (int)RoleSelectionMode.RoleList, typeof(RoleSelectionMode),
            [
                "TouOptionRoleAssignmentTypeEnumVanilla",
                "TouOptionRoleAssignmentTypeEnumRoleList",
                "TouOptionRoleAssignmentTypeEnumMinMaxList",
                "TouOptionRoleAssignmentTypeEnumDraft"
            ])
        {
            Visible = () => IsClassicRoleAssignment
        };

    public ModdedToggleOption LastImpostorBias { get; } =
        new("TouOptionReduceImpostorStreak", true)
        {
            Visible = () => IsClassicRoleAssignment && OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is not RoleDistribution.Vanilla and not RoleDistribution.Draft
        };

    public ModdedNumberOption ImpostorBiasPercent { get; } =
        new("TouOptionImpostorStreakReductionChance", 15f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.LastImpostorBias && IsClassicRoleAssignment && OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is not RoleDistribution.Vanilla and not RoleDistribution.Draft
        };

    // --- Draft Settings (Declared BEFORE Slots to fix wiki option ordering) ---
    private static bool IsDraft =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft;

    public bool RoleListEnabled => RoleAssignmentType.Value is (int)RoleSelectionMode.RoleList;

    /*public ModdedEnumOption GuaranteedKiller { get; } =
        new("TouOptionGuaranteedKiller", (int)RequiredKiller.ImpostorOrNeutralKiller,
            typeof(RequiredKiller),
            [
                "TouOptionGuaranteedKillerEnumImpostor",
                "TouOptionGuaranteedKillerEnumNeutralKiller",
                "TouOptionGuaranteedKillerEnumImpostorOrNeutralKiller"
            ])
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution()
                is RoleDistribution.RoleList
        };*/

    /*public ModdedStringOption SlotCustom { get; } =
        new("TouOptionCustomSlot", HudManagerPatches.StoredRoleBuckets[0],
            HudManagerPatches.StoredRoleBuckets.ToArray())
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution()
                is RoleDistribution.RoleList
        };*/

    public ModdedEnumOption<DraftRecapMode> DraftRecap { get; } =
        new("TouOptionDraftRecapDisplays", DraftRecapMode.Faction,
            [
                "TouOptionDraftDisplayEnumNothing",
                "TouOptionDraftDisplayEnumFaction",
                "TouOptionDraftDisplayEnumAlignment",
                "TouOptionDraftDisplayEnumRole"
            ])
        {
            Visible = () => IsDraft
        };

    public ModdedEnumOption<DraftRecapMode> DraftSidebarDisplay { get; } =
        new("TouOptionDraftSidebarDisplays", DraftRecapMode.Faction,
            [
                "TouOptionDraftDisplayEnumNothing",
                "TouOptionDraftDisplayEnumFaction",
                "TouOptionDraftDisplayEnumAlignment",
                "TouOptionDraftDisplayEnumRole"
            ])
        {
            Visible = () => IsDraft
        };

    public ModdedToggleOption UseRoleListForPool { get; set; } =
        new("TouOptionDraftUseRoleListForPool", false)
        {
            Visible = () => IsDraft
        };

    public ModdedNumberOption OfferedRolesCount { get; set; } =
        new("TouOptionDraftOfferedRolesCount", 3f, 1f, 9f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => IsDraft
        };

    public ModdedToggleOption ShowRandomOption { get; set; } =
        new("TouOptionDraftShowRandomOption", true)
        {
            Visible = () => IsDraft
        };

    public ModdedNumberOption TurnDurationSeconds { get; set; } =
        new("TouOptionDraftTurnDuration", 10f, 5f, 60f, 1f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => IsDraft
        };

    public ModdedNumberOption ConcurrentPicks { get; set; } =
        new("TouOptionDraftConcurrentPicks", 1f, 1f, 2f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => IsDraft
        };

    public ModdedNumberOption ShufflesPerPlayer { get; set; } =
        new("TouOptionDraftShufflesPerPlayer", 1f, 0f, 3f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => IsDraft
        };

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

    public ModdedOptionList<ModdedEnumOption<RoleListOption>> Slot { get; } =
        new(15, i => new($"TouOptionRoleListSlot{i + 1}",
                         i + 2 % 5 == 0 ? RoleListOption.ImpCommon : RoleListOption.CrewCommon,
                         OptionStrings)
            {
                Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
            }
        );

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>(Slot.Select(slot => slot.StringName))
        {
            // These are hidden because rolelist text already handles this
            MaxNeutralBenign.StringName,
            MinNeutralBenign.StringName,
            MaxNeutralEvil.StringName,
            MinNeutralEvil.StringName,
            MaxNeutralKiller.StringName,
            MinNeutralKiller.StringName,
            MaxNeutralOutlier.StringName,
            MinNeutralOutlier.StringName
        };

    private const string WikiHeaderOpenTag = "<color=#FFD700>";
    private const float WikiCharWidthPercent = 3.2f;
    private const float WikiRowsAtFullSize = 7.5f;
    private const float WikiSecondColumnStartPercent = 52f;
    private const int WikiTwoColumnThreshold = 6;

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        if (CurrentRoleDistribution() == RoleDistribution.Vanilla) { return Enumerable.Empty<string>(); }

        if (HudManagerPatches.RoleListTextComp == null || string.IsNullOrWhiteSpace(HudManagerPatches.RoleListTextComp.text))
        {
            return Enumerable.Empty<string>();
        }

        var lines = HudManagerPatches.RoleListTextComp.text
            .Replace("<i>", string.Empty, StringComparison.Ordinal)
            .Replace("</i>", string.Empty, StringComparison.Ordinal)
            .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        return [BuildWikiPage(lines)];
    }

    private static string BuildWikiPage(string[] pageLines)
    {
        var body = pageLines[1..];
        var columns = body.Length > WikiTwoColumnThreshold ? 2 : 1;
        var rows = (int)Math.Ceiling(body.Length / (double)columns);
        var lineSpacing = columns == 2 ? 100f : 125f;
        var bullet = columns == 2 ? string.Empty : "<color=#696969>•</color> ";

        var extraWidth = columns == 2 ? 0 : 2;
        var widest = body.Length == 0
            ? 1
            : body.Max(x => Regex.Replace(x, "<[^>]+>", string.Empty).Length) + extraWidth;
        var widthLimit = 100f * (columns == 2 ? 46f : 94f) / (widest * WikiCharWidthPercent);
        var heightLimit = 100f * WikiRowsAtFullSize / ((rows * lineSpacing / 100f) + 1.5f);
        var size = Math.Min(Math.Min(widthLimit, heightLimit), 100f);

        var builder = new StringBuilder("<page>");
        builder.Append(CultureInfo.InvariantCulture, $"<size={size * 1.15f:0}%><b>{pageLines[0]}</b></size>\n");
        builder.Append(CultureInfo.InvariantCulture, $"<size={size:0}%><line-height={lineSpacing:0}%>");
        for (var i = 0; i < rows; i++)
        {
            builder.Append(bullet).Append(body[i]);
            var right = i + rows;
            if (columns == 2 && right < body.Length)
            {
                builder.Append(CultureInfo.InvariantCulture, $"<pos={WikiSecondColumnStartPercent:0}%>").Append(body[right]);
            }

            builder.Append('\n');
        }

        return builder.Append("</size>").ToString();
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