using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Utilities;

namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup
{
    // TODO: Once hide and seek is possibly implemented as a selectable mode, then this code should be removed.
    public override Func<bool> GroupVisible => () =>
        !(GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek
            or GameModes.SeekFools);
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

    public RoleDistribution CurrentRoleDistribution()
    {
        var gameMode = (TouGamemode)CustomGameMode.Value;
        var roleDist = (RoleSelectionMode)RoleAssignmentType.Value;
        if (/*gameMode is TouGamemode.HideAndSeek && */GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek or GameModes.SeekFools)
        {
            return RoleDistribution.HideAndSeek;
        }

        if (gameMode is TouGamemode.Cultist)
        {
            return RoleDistribution.Cultist;
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

    public bool IsClassicRoleAssignment
    {
        get
        {
            var gameMode = (TouGamemode)CustomGameMode.Value;
            return !(GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek
                or GameModes.SeekFools || gameMode is TouGamemode.Cultist);
        }
    }
    public ModdedEnumOption CustomGameMode { get; } =
        new("Current Game Mode", (int)TouGamemode.Normal, typeof(TouGamemode), ["Normal", "Hide And Seek (N/A)", "Cultist (N/A)"], false)
        {
            // Who could've possibly thought this code breaks the game?
            /*ChangedEvent = x =>
            {
                var newGm = (TouGamemode)x;
                var manager = GameOptionsManager.Instance;
                if (manager != null)
                {
                    if (newGm is TouGamemode.HideAndSeek && manager.currentGameMode is not GameModes.HideNSeek && manager.currentGameMode is not GameModes.SeekFools)
                    {
                        GameOptionsManager.Instance.SwitchGameMode(GameModes.HideNSeek);
                        GameManager.DestroyInstance();
                        GameManager netObjParent2 = GameManagerCreator.CreateGameManager(GameOptionsManager.Instance.CurrentGameOptions.GameMode);
                        AmongUsClient.Instance.Spawn(netObjParent2, -2, SpawnFlags.None);
                    }
                    else if (newGm is not TouGamemode.HideAndSeek && (manager.currentGameMode is GameModes.HideNSeek || manager.currentGameMode is GameModes.SeekFools))
                    {
                        GameOptionsManager.Instance.SwitchGameMode(GameModes.Normal);
                        GameManager.DestroyInstance();
                        GameManager netObjParent2 = GameManagerCreator.CreateGameManager(GameOptionsManager.Instance.CurrentGameOptions.GameMode);
                        AmongUsClient.Instance.Spawn(netObjParent2, -2, SpawnFlags.None);
                    }
                }

                Debug($"New gamemode is {newGm.ToString().ToLowerInvariant()}!");
            }*/
            Visible = () => true
        };
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

    public ModdedEnumOption Slot1 { get; } =
        new("Slot 1", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot2 { get; } =
        new("Slot 2", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot3 { get; } =
        new("Slot 3", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot4 { get; } =
        new("Slot 4", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot5 { get; } =
        new("Slot 5", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot6 { get; } =
        new("Slot 6", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot7 { get; } =
        new("Slot 7", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot8 { get; } =
        new("Slot 8", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot9 { get; } =
        new("Slot 9", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot10 { get; } =
        new("Slot 10", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot11 { get; } =
        new("Slot 11", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot12 { get; } =
        new("Slot 12", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot13 { get; } =
        new("Slot 13", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot14 { get; } =
        new("Slot 14", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.RoleList
        };

    public ModdedEnumOption Slot15 { get; } =
        new("Slot 15", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
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