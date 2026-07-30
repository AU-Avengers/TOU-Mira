using BepInEx.Configuration;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Modules;
using TownOfUs.Patches;

namespace TownOfUs;

public class TouLocalTabPlayers(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "Players";
    protected override bool ShouldCreateLabels => true;

    public override void Open()
    {
        base.Open();

        foreach (var entry in TouLocale.LocalizedToggles)
        {
            var toggleObject = entry.Key;
            LocalizedLocalToggleSetting.UpdateToggleText(toggleObject.Text, entry.Value, toggleObject.onState);
        }

        foreach (var entry in TouLocale.LocalizedSliders)
        {
            var sliderObject = entry.Key;
            sliderObject.SliderObject.Title.text =
                LocalizedLocalSliderSetting.GetLocalizedValueText(sliderObject, sliderObject.LocaleKey);
        }
    }

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

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ColorPlayerNameToggle { get; private set; } =
        config.Bind("UI / Visuals", "ColorPlayerName", false);

    [LocalizedLocalEnumSetting(names: ["NameStyleTop", "NameStyleTopSmall", "NameStyleBottom", "NameStyleBottomSmall"])]
    public ConfigEntry<NameStyle> RoleNameStyle { get; private set; } =
        config.Bind("UI / Visuals", "RoleNameStyle", NameStyle.TopSmall);

    [LocalizedLocalEnumSetting(names: ["ProgressTrackingNever", "ProgressTrackingOnSelf", "ProgressTrackingOnOthers", "ProgressTrackingAlways"])]
    public ConfigEntry<ProgressTracking> DisplayPlayerProgress { get; private set; } =
        config.Bind("UI / Visuals", "DisplayPlayerProgress", ProgressTracking.Always);

    [LocalizedLocalToggleSetting]
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