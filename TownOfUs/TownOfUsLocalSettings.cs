using BepInEx.Configuration;
using MiraAPI.Utilities;
using MiraAPI.LocalSettings.Attributes;
using TownOfUs.Patches;

namespace TownOfUs;

public class TownOfUsLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU: Mira";
    protected override bool ShouldCreateLabels => true;
    private static float _oldButtonSliderValue;
    
    public override void Open()
    {
        base.Open();

        _oldButtonSliderValue = ButtonUIFactorSlider.Value;
    }
    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        if (configEntry == ButtonUIFactorSlider)
        {
            if (HudManager.InstanceExists)
            {
                HudManagerPatches.ResizeUI(1f / _oldButtonSliderValue);
                HudManagerPatches.ResizeUI(ButtonUIFactorSlider.Value);
            }
            _oldButtonSliderValue = ButtonUIFactorSlider.Value;
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.TouMiraIcon
    };

    [LocalToggleSetting]
    public ConfigEntry<bool> DeadSeeGhostsToggle { get; private set; } = config.Bind("Gameplay", "DeadSeeGhosts", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowVentsToggle { get; private set; } = config.Bind("Gameplay", "ShowVents", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> SortGuessingByAlignmentToggle { get; private set; } =
        config.Bind("Gameplay", "SortGuessingByAlignment", false);

    [LocalToggleSetting]
    public ConfigEntry<bool> PreciseCooldownsToggle { get; private set; } =
        config.Bind("Gameplay", "PreciseCooldowns", false);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowShieldHudToggle { get; private set; } =
        config.Bind("UI/Visuals", "ShowShieldHud", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> OffsetButtonsToggle { get; private set; } =
        config.Bind("UI/Visuals", "OffsetButtons", false);

    [LocalSliderSetting(min: 0.5f, max: 1.5f, suffixType: MiraNumberSuffixes.Multiplier, formatString: "0.00",
        displayValue: true)]
    public ConfigEntry<float> ButtonUIFactorSlider { get; private set; } =
        config.Bind("UI/Visuals", "ButtonUIFactor", 0.75f);

    [LocalToggleSetting]
    public ConfigEntry<bool> ColorPlayerNameToggle { get; private set; } =
        config.Bind("UI/Visuals", "ColorPlayerName", false);

    [LocalToggleSetting]
    public ConfigEntry<bool> UseCrewmateTeamColorToggle { get; private set; } =
        config.Bind("UI/Visuals", "UseCrewmateTeamColor", false);

    [LocalEnumSetting(names: ["ArrowDefault", "ArrowDarkGlow", "ArrowColorGlow", "ArrowLegacy"])]
    public ConfigEntry<ArrowStyleType> ArrowStyleEnum { get; private set; } =
        config.Bind("UI/Visuals", "ArrowStyle", ArrowStyleType.Default);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowWelcomeMessageToggle { get; private set; } =
        config.Bind("Miscellaneous", "ShowWelcomeMessage", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowSummaryMessageToggle { get; private set; } =
        config.Bind("Miscellaneous", "ShowSummaryMessage", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> VanillaWikiEntriesToggle { get; private set; } =
        config.Bind("Miscellaneous", "ShowVanillaWikiEntries", false);
}

public enum ArrowStyleType
{
    Default,
    DarkGlow,
    ColorGlow,
    Legacy
}