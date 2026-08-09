using MiraAPI.GameOptions;
using MiraAPI.Utilities;

namespace TownOfUs.Options.Modifiers;

public sealed class UniversalModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Universal Modifiers";
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 1;

    public AmountChanceOption ButtonBarryChance { get; } = new("Button Barry Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.ButtonBarry, asset: TouModifierIcons.ButtonBarry,
        assetName: "TouMira.Modifier.Universal.ButtonBarry", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.ButtonBarryChance;
            opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
                opt.StringName,
                TouLocale.Get("TouModifierButtonBarry"),
                opt.Value > 0 ? "1" : "0",
                opt.Data.GetValueString(opt.Value));
        }
    };

    public AmountChanceOption TiebreakerChance { get; } = new("Tiebreaker Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Tiebreaker, asset: TouModifierIcons.Tiebreaker,
        assetName: "TouMira.Modifier.Universal.Tiebreaker", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerChance;
            opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
                opt.StringName,
                TouLocale.Get("TouModifierTiebreaker"),
                opt.Value > 0 ? "1" : "0",
                opt.Data.GetValueString(opt.Value));
        }
    };

    public AmountChanceOption DrunkAmount { get; } = new("Drunk Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Drunk, asset: TouModifierIcons.Drunk,
        assetName: "TouMira.Modifier.Universal.Drunk", assetScale: 1.45f)
    {
        ChangedEvent = _drunkNotif,
    };

    public AmountChanceOption DrunkChance { get; } = new("Drunk Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Drunk, asset: TouModifierIcons.Drunk,
        assetName: "TouMira.Modifier.Universal.Drunk", assetScale: 1.45f)
    {
        ChangedEvent = _drunkNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkAmount > 0
    };

    public AmountChanceOption FlashAmount { get; } = new("Flash Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Flash, asset: TouModifierIcons.Flash,
        assetName: "TouMira.Modifier.Universal.Flash", assetScale: 1.45f)
    {
        ChangedEvent = _flashNotif,
    };

    public AmountChanceOption FlashChance { get; } = new("Flash Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Flash, asset: TouModifierIcons.Flash,
        assetName: "TouMira.Modifier.Universal.Flash", assetScale: 1.45f)
    {
        ChangedEvent = _flashNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashAmount > 0
    };

    public AmountChanceOption GiantAmount { get; } = new("Giant Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Giant, asset: TouModifierIcons.Giant,
        assetName: "TouMira.Modifier.Universal.Giant", assetScale: 1.45f)
    {
        ChangedEvent = _giantNotif,
    };

    public AmountChanceOption GiantChance { get; } = new("Giant Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Giant, asset: TouModifierIcons.Giant,
        assetName: "TouMira.Modifier.Universal.Giant", assetScale: 1.45f)
    {
        ChangedEvent = _giantNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantAmount > 0
    };

    public AmountChanceOption ImmovableAmount { get; } = new("Immovable Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Immovable, asset: TouModifierIcons.Immovable,
        assetName: "TouMira.Modifier.Universal.Immovable", assetScale: 1.45f)
    {
        ChangedEvent = _immovableNotif,
    };

    public AmountChanceOption ImmovableChance { get; } = new("Immovable Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Immovable, asset: TouModifierIcons.Immovable,
        assetName: "TouMira.Modifier.Universal.Immovable", assetScale: 1.45f)
    {
        ChangedEvent = _immovableNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ImmovableAmount > 0
    };

    public AmountChanceOption MiniAmount { get; } = new("Mini Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Mini, asset: TouModifierIcons.Mini,
        assetName: "TouMira.Modifier.Universal.Mini", assetScale: 1.45f)
    {
        ChangedEvent = _miniNotif,
    };

    public AmountChanceOption MiniChance { get; } = new("Mini Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Mini, asset: TouModifierIcons.Mini,
        assetName: "TouMira.Modifier.Universal.Mini", assetScale: 1.45f)
    {
        ChangedEvent = _miniNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniAmount > 0
    };

    public AmountChanceOption RadarAmount { get; } = new("Radar Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Radar, asset: TouModifierIcons.Radar,
        assetName: "TouMira.Modifier.Universal.Radar", assetScale: 1.45f)
    {
        ChangedEvent = _radarNotif,
    };

    public AmountChanceOption RadarChance { get; } = new("Radar Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Radar, asset: TouModifierIcons.Radar,
        assetName: "TouMira.Modifier.Universal.Radar", assetScale: 1.45f)
    {
        ChangedEvent = _radarNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.RadarAmount > 0
    };

    public AmountChanceOption SatelliteAmount { get; } = new("Satellite Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Satellite, asset: TouModifierIcons.Satellite,
        assetName: "TouMira.Modifier.Universal.Satellite", assetScale: 1.45f)
    {
        ChangedEvent = _satelliteNotif,
    };

    public AmountChanceOption SatelliteChance { get; } = new("Satellite Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Satellite, asset: TouModifierIcons.Satellite,
        assetName: "TouMira.Modifier.Universal.Satellite", assetScale: 1.45f)
    {
        ChangedEvent = _satelliteNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteAmount > 0
    };

    public AmountChanceOption ShyAmount { get; } = new("Shy Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Shy, asset: TouModifierIcons.Shy,
        assetName: "TouMira.Modifier.Universal.Shy", assetScale: 1.45f)
    {
        ChangedEvent = _shyNotif,
    };

    public AmountChanceOption ShyChance { get; } = new("Shy Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Shy, asset: TouModifierIcons.Shy,
        assetName: "TouMira.Modifier.Universal.Shy", assetScale: 1.45f)
    {
        ChangedEvent = _shyNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ShyAmount > 0
    };

    public AmountChanceOption SixthSenseAmount { get; } = new("Sixth Sense Amount", 0, 0, 5, 1,
        color: TownOfUsColors.SixthSense, asset: TouModifierIcons.SixthSense,
        assetName: "TouMira.Modifier.Universal.SixthSense", assetScale: 1.45f)
    {
        ChangedEvent = _sixthSenseNotif,
    };

    public AmountChanceOption SixthSenseChance { get; } = new("Sixth Sense Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.SixthSense, asset: TouModifierIcons.SixthSense,
        assetName: "TouMira.Modifier.Universal.SixthSense", assetScale: 1.45f)
    {
        ChangedEvent = _sixthSenseNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.SixthSenseAmount > 0
    };

    public AmountChanceOption SleuthAmount { get; } = new("Sleuth Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Sleuth, asset: TouModifierIcons.Sleuth,
        assetName: "TouMira.Modifier.Universal.Sleuth", assetScale: 1.45f)
    {
        ChangedEvent = _sleuthNotif,
    };

    public AmountChanceOption SleuthChance { get; } = new("Sleuth Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Sleuth, asset: TouModifierIcons.Sleuth,
        assetName: "TouMira.Modifier.Universal.Sleuth", assetScale: 1.45f)
    {
        ChangedEvent = _sleuthNotif,
        Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthAmount > 0
    };

    private static Action<float> _drunkNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkChance;
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            TouLocale.Get("TouModifierDrunk"),
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    };

    private static Action<float> _flashNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashChance;
        RunNotif(opt, optAmount, "TouModifierFlash");
    };

    private static Action<float> _giantNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantChance;
        RunNotif(opt, optAmount, "TouModifierGiant");
    };

    private static Action<float> _immovableNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.ImmovableAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.ImmovableChance;
        RunNotif(opt, optAmount, "TouModifierImmovable");
    };

    private static Action<float> _miniNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniChance;
        RunNotif(opt, optAmount, "TouModifierMini");
    };

    private static Action<float> _radarNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.RadarAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.RadarChance;
        RunNotif(opt, optAmount, "TouModifierRadar");
    };

    private static Action<float> _satelliteNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteChance;
        RunNotif(opt, optAmount, "TouModifierSatellite");
    };

    private static Action<float> _shyNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.ShyAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.ShyChance;
        RunNotif(opt, optAmount, "TouModifierShy");
    };

    private static Action<float> _sixthSenseNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.SixthSenseAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.SixthSenseChance;
        RunNotif(opt, optAmount, "TouModifierSixthSense");
    };

    private static Action<float> _sleuthNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthChance;
        RunNotif(opt, optAmount, "TouModifierSleuth");
    };

    private static void RunNotif(AmountChanceOption opt, AmountChanceOption optAmount, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            TouLocale.Get(title),
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    }
}