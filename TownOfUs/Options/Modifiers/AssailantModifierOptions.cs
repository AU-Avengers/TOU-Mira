using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class AssailantModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Assailant Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => TownOfUsColors.Overclocker;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 3;

    public AmountChanceOption ImpDoubleShotAmount { get; } = new("<sprite name=\"AmongUs.Role.Impostor\"> Double Shot Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.DoubleShot,
        assetName: "TouMira.Modifier.Assailant.DoubleShot", assetScale: 1.45f)
    {
        ChangedEvent = _dsImpNotif
    };

    public AmountChanceOption ImpDoubleShotChance { get; } = new("<sprite name=\"AmongUs.Role.Impostor\"> Double Shot Chance", 50f, 0, 100f, 10f, "#",
        "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.DoubleShot,
        assetName: "TouMira.Modifier.Assailant.DoubleShot", assetScale: 1.45f)
    {
        ChangedEvent = _dsImpNotif,
        Visible = () => OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpDoubleShotAmount > 0
    };

    public AmountChanceOption NeutDoubleShotAmount { get; } = new("<sprite name=\"AmongUs.Role.Neutral\"> Double Shot Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Neutral, asset: TouModifierIcons.DoubleShot,
        assetName: "TouMira.Modifier.Assailant.DoubleShot", assetScale: 1.45f)
    {
        ChangedEvent = _dsNeutNotif
    };

    public AmountChanceOption NeutDoubleShotChance { get; } = new("<sprite name=\"AmongUs.Role.Neutral\"> Double Shot Chance", 50f, 0, 100f, 10f, "#",
        "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Neutral, asset: TouModifierIcons.DoubleShot,
        assetName: "TouMira.Modifier.Assailant.DoubleShot", assetScale: 1.45f)
    {
        ChangedEvent = _dsNeutNotif,
        Visible = () => OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutDoubleShotAmount > 0
    };

    private static Action<float> _dsImpNotif = x =>
    {
        var optAmount = OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpDoubleShotAmount;
        var opt = OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpDoubleShotChance;
        RunNotif(opt, optAmount, "TouModifierDoubleShot");
    };

    private static Action<float> _dsNeutNotif = x =>
    {
        var optAmount = OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutDoubleShotAmount;
        var opt = OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutDoubleShotChance;
        RunNotif(opt, optAmount, "TouModifierDoubleShot");
    };

    public AmountChanceOption ImpOverclockerAmount { get; } = new("<sprite name=\"AmongUs.Role.Impostor\"> Overclocker Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Impostor, asset: TouModifierIcons.Overclocker,
        assetName: "TouMira.Modifier.Assailant.Overclocker", assetScale: 1.45f)
    {
        ChangedEvent = _ocImpNotif
    };

    public AmountChanceOption ImpOverclockerChance { get; } = new("<sprite name=\"AmongUs.Role.Impostor\"> Overclocker Chance", 50f, 0, 100f, 10f, "#",
        "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Overclocker,
        assetName: "TouMira.Modifier.Assailant.Overclocker", assetScale: 1.45f)
    {
        ChangedEvent = _ocImpNotif,
        Visible = () => OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpOverclockerAmount > 0
    };

    public AmountChanceOption NeutOverclockerAmount { get; } = new("<sprite name=\"AmongUs.Role.Neutral\"> Overclocker Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Neutral, asset: TouModifierIcons.Overclocker,
        assetName: "TouMira.Modifier.Assailant.Overclocker", assetScale: 1.45f)
    {
        ChangedEvent = _ocNeutNotif
    };

    public AmountChanceOption NeutOverclockerChance { get; } = new("<sprite name=\"AmongUs.Role.Neutral\"> Overclocker Chance", 50f, 0, 100f, 10f, "#",
        "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Neutral, asset: TouModifierIcons.Overclocker,
        assetName: "TouMira.Modifier.Assailant.Overclocker", assetScale: 1.45f)
    {
        ChangedEvent = _ocNeutNotif,
        Visible = () => OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutOverclockerAmount > 0
    };

    private static Action<float> _ocImpNotif = x =>
    {
        var optAmount = OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpOverclockerAmount;
        var opt = OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpOverclockerChance;
        RunNotif(opt, optAmount, "TouModifierOverclocker");
    };

    private static Action<float> _ocNeutNotif = x =>
    {
        var optAmount = OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutOverclockerAmount;
        var opt = OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutOverclockerChance;
        RunNotif(opt, optAmount, "TouModifierOverclocker");
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