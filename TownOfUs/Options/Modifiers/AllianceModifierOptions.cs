using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class AllianceModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Alliance Modifiers";
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
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
            RunNotif(opt, x > 0f ? "1" : "0", "TouModifierCrewpostor");
        }
    };

    public AmountChanceOption EgotistChance { get; } = new("Egotist Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Egotist, asset: TouModifierIcons.Egotist,
        assetName: "TouMira.Modifier.Alliance.Egotist", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance;
            RunNotif(opt, x > 0f ? "1" : "0", "TouModifierEgotist");
        }
    };

    public AmountChanceOption LoversChance { get; } = new("Lovers Chance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Lover, asset: TouModifierIcons.Lover,
        assetName: "TouMira.Modifier.Alliance.Lover", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance;
            RunNotif(opt, x > 0f ? "2" : "0", "TouModifierLover");
        }
    };
    private static void RunNotif(AmountChanceOption opt, string count, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            MiraLocaleManager.Get(title),
            count,
            opt.Data.GetValueString(opt.Value));
    }
};
