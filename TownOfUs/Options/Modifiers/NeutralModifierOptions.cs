using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Neutral Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => TownOfUsColors.Neutral;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 4;

    public AmountChanceOption DoubleShotAmount { get; } = new("Double Shot Amount", 0, 0, 5, 1,
        color: TownOfUsColors.Neutral, asset: TouModifierIcons.DoubleShot,
        assetName: "TouMira.Modifier.Assailant.DoubleShot", assetScale: 1.45f)
    {
        ChangedEvent = _dsNotif
    };

    public AmountChanceOption DoubleShotChance { get; } = new("Double Shot Chance", 50f, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Neutral, asset: TouModifierIcons.DoubleShot,
        assetName: "TouMira.Modifier.Assailant.DoubleShot", assetScale: 1.45f)
    {
        ChangedEvent = _dsNotif,
        Visible = () => OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotAmount > 0
    };
    private static Action<float> _dsNotif = x =>
    {
        var optAmount = OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotAmount;
        var opt = OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotChance;
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            TouLocale.Get("TouModifierDoubleShot"),
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    };
}