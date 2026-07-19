using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings.Attributes;
using TownOfUs.Options;

namespace TownOfUs.Modules.DraftMode;

public sealed class TownOfUsLocalDraftSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.IconDraftMode
    };
    public override string TabName => "Draft";
    protected override bool ShouldCreateLabels => true;

    public override bool ShouldCreateButton =>
        OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.Draft;

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowDraftTooltips { get; private set; } =
        config.Bind(
            "UI",
            "Show Draft Tooltips",
            true);

}
