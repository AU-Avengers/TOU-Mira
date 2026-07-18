using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class AllianceModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Alliance Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => Color.white;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 0;

    public AmountChanceOption CrewpostorChance { get; } = new("Crewpostor Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Crewpostor,
        assetName: "TouMira.Modifier.Alliance.Crewpostor", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.CrewpostorChance;
            opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
                opt.StringName,
                TouLocale.Get("TouModifierCrewpostor"),
                x > 0f ? "1" : "0",
                opt.Data.GetValueString(x));
        }
    };

    public AmountChanceOption EgotistChance { get; } = new("Egotist Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Egotist, asset: TouModifierIcons.Egotist,
        assetName: "TouMira.Modifier.Alliance.Egotist", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance;
            opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
                opt.StringName,
                TouLocale.Get("TouModifierEgotist"),
                x > 0f ? "1" : "0",
                opt.Data.GetValueString(x));
        }
    };

    public AmountChanceOption LoversChance { get; } = new("Lovers Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Lover, asset: TouModifierIcons.Lover,
        assetName: "TouMira.Modifier.Alliance.Lover", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance;
            opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
                opt.StringName,
                TouLocale.Get("TouModifierLover"),
                x > 0f ? "2" : "0",
                opt.Data.GetValueString(x));
        }
    };
}