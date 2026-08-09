using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class ImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Impostor Modifiers";
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 3;
    public AmountChanceOption CircumventAmount { get; } = new("Circumvent Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.Circumvent,
        assetName: "TouMira.Modifier.Impostor.Circumvent", assetScale: 1.45f)
    {
        ChangedEvent = _circumventNotif
    };

    public AmountChanceOption CircumventChance { get; } = new("Circumvent Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Circumvent,
        assetName: "TouMira.Modifier.Impostor.Circumvent", assetScale: 1.45f)
    {
        ChangedEvent = _circumventNotif,
        Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventAmount > 0
    };

    public AmountChanceOption DeadlyQuotaAmount { get; } = new("Deadly Quota Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.DeadlyQuota,
        assetName: "TouMira.Modifier.Impostor.DeadlyQuota", assetScale: 1.45f)
    {
        ChangedEvent = _dqNotif
    };

    public AmountChanceOption DeadlyQuotaChance { get; } = new("Deadly Quota Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.DeadlyQuota,
        assetName: "TouMira.Modifier.Impostor.DeadlyQuota", assetScale: 1.45f)
    {
        ChangedEvent = _dqNotif,
        Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaAmount > 0
    };

    public AmountChanceOption DisperserAmount { get; } = new("Disperser Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.Disperser,
        assetName: "TouMira.Modifier.Impostor.Disperser", assetScale: 1.45f)
    {
        ChangedEvent = _disperserNotif
    };

    public AmountChanceOption DisperserChance { get; } = new("Disperser Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Disperser,
        assetName: "TouMira.Modifier.Impostor.Disperser", assetScale: 1.45f)
    {
        ChangedEvent = _disperserNotif,
        Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.DisperserAmount > 0
    };

    public AmountChanceOption SaboteurAmount { get; } = new("Saboteur Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.Saboteur,
        assetName: "TouMira.Modifier.Impostor.Saboteur", assetScale: 1.45f)
    {
        ChangedEvent = _saboteurNotif
    };

    public AmountChanceOption SaboteurChance { get; } = new("Saboteur Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Saboteur,
        assetName: "TouMira.Modifier.Impostor.Saboteur", assetScale: 1.45f)
    {
        ChangedEvent = _saboteurNotif,
        Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurAmount > 0
    };

    public AmountChanceOption TelepathAmount { get; } = new("Telepath Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.Telepath,
        assetName: "TouMira.Modifier.Impostor.Telepath", assetScale: 1.45f)
    {
        ChangedEvent = _telepathNotif
    };

    public AmountChanceOption TelepathChance { get; } = new("Telepath Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Telepath,
        assetName: "TouMira.Modifier.Impostor.Telepath", assetScale: 1.45f)
    {
        ChangedEvent = _telepathNotif,
        Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.TelepathAmount > 0
    };

    public AmountChanceOption UnderdogAmount { get; } = new("Underdog Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.Underdog,
        assetName: "TouMira.Modifier.Impostor.Underdog", assetScale: 1.45f)
    {
        ChangedEvent = _underdogNotif
    };

    public AmountChanceOption UnderdogChance { get; } = new("Underdog Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Underdog,
        assetName: "TouMira.Modifier.Impostor.Underdog", assetScale: 1.45f)
    {
        ChangedEvent = _underdogNotif,
        Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.UnderdogAmount > 0
    };
    
    private static Action<float> _circumventNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventChance;
        RunNotif(opt, optAmount, "TouModifierCircumvent");
    };
    private static Action<float> _dqNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaChance;
        RunNotif(opt, optAmount, "TouModifierDeadlyQuota");
    };
    private static Action<float> _disperserNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.DisperserAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.DisperserChance;
        RunNotif(opt, optAmount, "TouModifierDisperser");
    };
    private static Action<float> _saboteurNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurChance;
        RunNotif(opt, optAmount, "TouModifierSaboteur");
    };
    private static Action<float> _telepathNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.TelepathAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.TelepathChance;
        RunNotif(opt, optAmount, "TouModifierTelepath");
    };
    private static Action<float> _underdogNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.UnderdogAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.UnderdogChance;
        RunNotif(opt, optAmount, "TouModifierUnderdog");
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