using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class CrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Crewmate Modifiers";
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 2;

    public AmountChanceOption BaitChance { get; } = new("Bait Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Bait, asset: TouModifierIcons.Bait,
        assetName: "TouMira.Modifier.Crewmate.Bait", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.BaitChance;
            RunNotif(opt, x > 0f ? "1" : "0", "TouModifierBait");
        }
    };

    public AmountChanceOption CelebrityChance { get; } = new("Celebrity Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Celebrity, asset: TouModifierIcons.Celebrity,
        assetName: "TouMira.Modifier.Crewmate.Celebrity", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.CelebrityChance;
            RunNotif(opt, x > 0f ? "1" : "0", "TouModifierCelebrity");
        }
    };

    public AmountChanceOption AftermathAmount { get; } = new("Aftermath Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Aftermath, asset: TouModifierIcons.Aftermath,
        assetName: "TouMira.Modifier.Crewmate.Aftermath", assetScale: 1.45f)
    {
        ChangedEvent = _aftermathNotif
    };

    public AmountChanceOption AftermathChance { get; } = new("Aftermath Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Aftermath, asset: TouModifierIcons.Aftermath,
        assetName: "TouMira.Modifier.Crewmate.Aftermath", assetScale: 1.45f)
    {
        ChangedEvent = _aftermathNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathAmount > 0
    };

    public AmountChanceOption DiseasedAmount { get; } = new("Diseased Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Diseased, asset: TouModifierIcons.Diseased,
        assetName: "TouMira.Modifier.Crewmate.Diseased", assetScale: 1.45f)
    {
        ChangedEvent = _diseasedNotif
    };

    public AmountChanceOption DiseasedChance { get; } = new("Diseased Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Diseased, asset: TouModifierIcons.Diseased,
        assetName: "TouMira.Modifier.Crewmate.Diseased", assetScale: 1.45f)
    {
        ChangedEvent = _diseasedNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedAmount > 0
    };

    public AmountChanceOption FrostyAmount { get; } = new("Frosty Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Frosty, asset: TouModifierIcons.Frosty,
        assetName: "TouMira.Modifier.Crewmate.Frosty", assetScale: 1.45f)
    {
        ChangedEvent = _frostyNotif
    };

    public AmountChanceOption FrostyChance { get; } = new("Frosty Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Frosty, asset: TouModifierIcons.Frosty,
        assetName: "TouMira.Modifier.Crewmate.Frosty", assetScale: 1.45f)
    {
        ChangedEvent = _frostyNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.FrostyAmount > 0
    };

    public AmountChanceOption InvestigatorAmount { get; } = new("Investigator Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Investigator, asset: TouRoleIcons.Investigator,
        assetName: "TouMira.Role.Crewmate.Investigator", assetScale: 1.45f)
    {
        ChangedEvent = _investigatorNotif
    };

    public AmountChanceOption InvestigatorChance { get; } = new("Investigator Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Investigator, asset: TouRoleIcons.Investigator,
        assetName: "TouMira.Role.Crewmate.Investigator", assetScale: 1.45f)
    {
        ChangedEvent = _investigatorNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorAmount > 0
    };

    public AmountChanceOption MultitaskerAmount { get; } = new("Multitasker Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Multitasker, asset: TouModifierIcons.Multitasker,
        assetName: "TouMira.Modifier.Crewmate.Multitasker", assetScale: 1.45f)
    {
        ChangedEvent = _multitaskerNotif
    };

    public AmountChanceOption MultitaskerChance { get; } = new("Multitasker Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Multitasker, asset: TouModifierIcons.Multitasker,
        assetName: "TouMira.Modifier.Crewmate.Multitasker", assetScale: 1.45f)
    {
        ChangedEvent = _multitaskerNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerAmount > 0
    };

    public AmountChanceOption NoisemakerAmount { get; } = new("Noisemaker Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Noisemaker, asset: TouRoleIcons.Noisemaker,
        assetName: "AmongUs.Role.Noisemaker", assetScale: 1.45f)
    {
        ChangedEvent = _noisemakerNotif
    };

    public AmountChanceOption NoisemakerChance { get; } = new("Noisemaker Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Noisemaker, asset: TouRoleIcons.Noisemaker,
        assetName: "AmongUs.Role.Noisemaker", assetScale: 1.45f)
    {
        ChangedEvent = _noisemakerNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerAmount > 0
    };

    public AmountChanceOption OperativeAmount { get; } = new("Operative Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Operative, asset: TouModifierIcons.Operative,
        assetName: "TouMira.Modifier.Crewmate.Operative", assetScale: 1.45f)
    {
        ChangedEvent = _operativeNotif
    };

    public AmountChanceOption OperativeChance { get; } = new("Operative Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Operative, asset: TouModifierIcons.Operative,
        assetName: "TouMira.Modifier.Crewmate.Operative", assetScale: 1.45f)
    {
        ChangedEvent = _operativeNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.OperativeAmount > 0
    };

    public AmountChanceOption RottingAmount { get; } = new("Rotting Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Rotting, asset: TouModifierIcons.Rotting,
        assetName: "TouMira.Modifier.Crewmate.Rotting", assetScale: 1.45f)
    {
        ChangedEvent = _rottingNotif
    };

    public AmountChanceOption RottingChance { get; } = new("Rotting Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Rotting, asset: TouModifierIcons.Rotting,
        assetName: "TouMira.Modifier.Crewmate.Rotting", assetScale: 1.45f)
    {
        ChangedEvent = _rottingNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.RottingAmount > 0
    };

    public AmountChanceOption ScientistAmount { get; } = new("Scientist Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Scientist, asset: TouRoleIcons.Scientist,
        assetName: "AmongUs.Role.Scientist", assetScale: 1.45f)
    {
        ChangedEvent = _scientistNotif
    };

    public AmountChanceOption ScientistChance { get; } = new("Scientist Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Scientist, asset: TouRoleIcons.Scientist,
        assetName: "AmongUs.Role.Scientist", assetScale: 1.45f)
    {
        ChangedEvent = _scientistNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScientistAmount > 0
    };

    public AmountChanceOption ScoutAmount { get; } = new("Scout Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Scout, asset: TouModifierIcons.Scout,
        assetName: "TouMira.Modifier.Crewmate.Scout", assetScale: 1.45f)
    {
        ChangedEvent = _scoutNotif
    };

    public AmountChanceOption ScoutChance { get; } = new("Scout Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Scout, asset: TouModifierIcons.Scout,
        assetName: "TouMira.Modifier.Crewmate.Scout", assetScale: 1.45f)
    {
        ChangedEvent = _scoutNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScoutAmount > 0
    };

    public AmountChanceOption SpyAmount { get; } = new("Spy Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Spy, asset: TouRoleIcons.Spy,
        assetName: "TouMira.Role.Crewmate.Spy", assetScale: 1.45f)
    {
        ChangedEvent = _spyNotif
    };

    public AmountChanceOption SpyChance { get; } = new("Spy Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Spy, asset: TouRoleIcons.Spy,
        assetName: "TouMira.Role.Crewmate.Spy", assetScale: 1.45f)
    {
        ChangedEvent = _spyNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyAmount > 0
    };

    public AmountChanceOption TaskmasterAmount { get; } = new("Taskmaster Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Taskmaster, asset: TouModifierIcons.Taskmaster,
        assetName: "TouMira.Modifier.Crewmate.Taskmaster", assetScale: 1.45f)
    {
        ChangedEvent = _taskmasterNotif
    };

    public AmountChanceOption TaskmasterChance { get; } = new("Taskmaster Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Taskmaster, asset: TouModifierIcons.Taskmaster,
        assetName: "TouMira.Modifier.Crewmate.Taskmaster", assetScale: 1.45f)
    {
        ChangedEvent = _taskmasterNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.TaskmasterAmount > 0
    };

    public AmountChanceOption TorchAmount { get; } = new("Torch Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Torch, asset: TouModifierIcons.Torch,
        assetName: "TouMira.Modifier.Crewmate.Torch", assetScale: 1.45f)
    {
        ChangedEvent = _torchNotif
    };

    public AmountChanceOption TorchChance { get; } = new("Torch Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Torch, asset: TouModifierIcons.Torch,
        assetName: "TouMira.Modifier.Crewmate.Torch", assetScale: 1.45f)
    {
        ChangedEvent = _torchNotif,
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchAmount > 0
    };
    
    private static Action<float> _aftermathNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathChance;
        RunNotif(opt, optAmount, "TouModifierAftermath");
    };
    
    private static Action<float> _diseasedNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedChance;
        RunNotif(opt, optAmount, "TouModifierDiseased");
    };
    
    private static Action<float> _frostyNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.FrostyAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.FrostyChance;
        RunNotif(opt, optAmount, "TouModifierFrosty");
    };
    
    private static Action<float> _investigatorNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorChance;
        RunNotif(opt, optAmount, "TouRoleInvestigator");
    };
    
    private static Action<float> _multitaskerNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerChance;
        RunNotif(opt, optAmount, "TouModifierMultitasker");
    };
    
    private static Action<float> _noisemakerNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerChance;
        RunNotif(opt, optAmount, "TouModifierNoisemaker");
    };
    
    private static Action<float> _operativeNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.OperativeAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.OperativeChance;
        RunNotif(opt, optAmount, "TouModifierOperative");
    };
    
    private static Action<float> _rottingNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.RottingAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.RottingChance;
        RunNotif(opt, optAmount, "TouModifierRotting");
    };
    
    private static Action<float> _scientistNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScientistAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScientistChance;
        RunNotif(opt, optAmount, "TouModifierScientist");
    };
    
    private static Action<float> _scoutNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScoutAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScoutChance;
        RunNotif(opt, optAmount, "TouModifierScout");
    };
    
    private static Action<float> _spyNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyChance;
        RunNotif(opt, optAmount, "TouRoleSpy");
    };
    
    private static Action<float> _taskmasterNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.TaskmasterAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.TaskmasterChance;
        RunNotif(opt, optAmount, "TouModifierTaskmaster");
    };
    
    private static Action<float> _torchNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchChance;
        RunNotif(opt, optAmount, "TouModifierTorch");
    };

    private static void RunNotif(AmountChanceOption opt, string count, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            TouLocale.Get(title),
            count,
            opt.Data.GetValueString(opt.Value));
    }
    private static void RunNotif(AmountChanceOption opt, AmountChanceOption optAmount, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            TouLocale.Get(title),
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    }
}