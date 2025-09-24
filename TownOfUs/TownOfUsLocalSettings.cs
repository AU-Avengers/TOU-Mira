using BepInEx.Configuration;
using MiraAPI.Utilities;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Patches;

namespace TownOfUs;

public class TownOfUsLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU: Mira";
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
            sliderObject.SliderObject.Title.text = LocalizedLocalSliderSetting.GetLocalizedValueText(sliderObject, sliderObject.LocaleKey);
        }
    }

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        if (configEntry == ButtonUIFactorSlider)
        {
            var slider = TouLocale.LocalizedSliders.FirstOrDefault(x => x.Key.ConfigEntry == ButtonUIFactorSlider).Key;
            if (HudManager.InstanceExists)
            {
                HudManagerPatches.ResizeUI(1f / slider.OldValue);
                HudManagerPatches.ResizeUI(slider.GetValue());
            }
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.TouMiraIcon
    };

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> DeadSeeGhostsToggle { get; private set; } = config.Bind("Gameplay", "DeadSeeGhosts", true);
    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowVentsToggle { get; private set; } = config.Bind("Gameplay", "ShowVents", true);
    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> SortGuessingByAlignmentToggle { get; private set; } = config.Bind("Gameplay", "SortGuessingByAlignment", false);
    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> PreciseCooldownsToggle { get; private set; } = config.Bind("Gameplay", "PreciseCooldowns", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowShieldHudToggle { get; private set; } = config.Bind("UI/Visuals", "ShowShieldHud", true);
    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> OffsetButtonsToggle { get; private set; } = config.Bind("UI/Visuals", "OffsetButtons", false);
    [LocalizedLocalSliderSetting(min: 0.5f, max: 1.5f, suffixType: MiraNumberSuffixes.Multiplier, formatString: "0.00", displayValue:true)]
    public ConfigEntry<float> ButtonUIFactorSlider { get; private set; } = config.Bind("UI/Visuals", "ButtonUIFactor", 0.75f);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ColorPlayerNameToggle { get; private set; } = config.Bind("UI/Visuals", "ColorPlayerName", false);
    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> UseCrewmateTeamColorToggle { get; private set; } = config.Bind("UI/Visuals", "UseCrewmateTeamColor", false);
    [LocalizedLocalEnumSetting(names:["ArrowDefault", "ArrowDarkGlow", "ArrowColorGlow", "ArrowLegacy"])]
    public ConfigEntry<ArrowStyleType> ArrowStyleEnum { get; private set; } = config.Bind("UI/Visuals", "ArrowStyle", ArrowStyleType.Default);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowWelcomeMessageToggle { get; private set; } = config.Bind("Miscellaneous", "ShowWelcomeMessage", true);
    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowSummaryMessageToggle { get; private set; } = config.Bind("Miscellaneous", "ShowSummaryMessage", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> VanillaWikiEntriesToggle { get; private set; } = config.Bind("Miscellaneous", "ShowVanillaWikiEntries", false);
}

public enum ArrowStyleType
{
    Default,
    DarkGlow,
    ColorGlow,
    Legacy
}