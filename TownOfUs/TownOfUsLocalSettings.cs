using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;

namespace TownOfUs;

public class TownOfUsLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU:M";
    protected override bool ShouldCreateLabels => false;

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.TouMiraIcon
    };

    [LocalToggleSetting]
    public ConfigEntry<bool> VanillaWikiEntriesToggle { get; private set; } = config.Bind("Wiki", "Show Vanilla Wiki Entries", false);
}