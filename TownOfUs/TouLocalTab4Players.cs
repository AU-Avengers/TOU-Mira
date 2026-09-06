using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.LocalSettings.SettingTypes;
using TownOfUs.Modules;
using TownOfUs.Patches;

namespace TownOfUs;

public class TouLocalTabPlayers(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "Players";
    protected override bool ShouldCreateLabels => true;

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        if (configEntry == RoleNameStyle)
        {
            HudManagerPatches.RoleNameStyle = RoleNameStyle.Value;
            FakePlayer.UpdateFakePlayerText();
            StonedPlayer.UpdateFakePlayerText();
        }
        else if (configEntry == DisplayPlayerProgress)
        {
            HudManagerPatches.PlayerNameProgress = DisplayPlayerProgress.Value;
        }
        else if (configEntry == ShowRoleIcons)
        {
            HudManagerPatches.IconOnRoleName = ShowRoleIcons.Value;
        }
        else if (configEntry == ColorPlayerNameToggle)
        {
            FakePlayer.UpdateFakePlayerText();
            StonedPlayer.UpdateFakePlayerText();
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.LocalPlayers,
        HideIconOnHover = false,
    };

    [LocalToggleSetting]
    public ConfigEntry<bool> ColorPlayerNameToggle { get; private set; } =
        config.Bind("UI / Visuals", "ColorPlayerName", false);

    [LocalEnumSetting(names: ["NameStyleTop", "NameStyleTopSmall", "NameStyleBottom", "NameStyleBottomSmall"])]
    public ConfigEntry<NameStyle> RoleNameStyle { get; private set; } =
        config.Bind("UI / Visuals", "RoleNameStyle", NameStyle.TopSmall);

    [LocalEnumSetting(names: ["ProgressTrackingNever", "ProgressTrackingOnSelf", "ProgressTrackingOnOthers", "ProgressTrackingAlways"])]
    public ConfigEntry<ProgressTracking> DisplayPlayerProgress { get; private set; } =
        config.Bind("UI / Visuals", "DisplayPlayerProgress", ProgressTracking.Always);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowRoleIcons { get; private set; } =
        config.Bind("UI / Visuals", "ShowRoleIcons", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> UseCrewmateTeamColorToggle { get; private set; } =
        config.Bind("Gameplay", "UseCrewmateTeamColor", false);
}

public enum ProgressTracking
{
    Never,
    OnSelf,
    OnOthers,
    Always
}

public enum NameStyle
{
    Top,
    TopSmall,
    Bottom,
    BottomSmall,
}