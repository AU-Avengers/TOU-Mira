using BepInEx.Configuration;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;

namespace TownOfUs;

public class TownOfUsLocalRoleSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU:M Roles";
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
        if ((configEntry == ParasitePiPLocation || configEntry == ParasitePiPSize) &&
                 PlayerControl.LocalPlayer != null &&
                 PlayerControl.LocalPlayer.Data?.Role is Roles.Impostor.ParasiteRole parasiteRole)
        {
            // Apply PiP changes to the Parasite (controller) side.
            parasiteRole.MarkPiPSettingsDirty(resetManualThisSession: true);
            parasiteRole.TickPiP();
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouRoleIcons.Engineer
    };

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> SortGuessingByAlignmentToggle { get; private set; } =
        config.Bind("Gameplay", "SortGuessingByAlignment", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> UseCrewmateTeamColorToggle { get; private set; } =
        config.Bind("Gameplay", "UseCrewmateTeamColor", false);

    [LocalizedLocalEnumSetting(names: ["ArrowDefault", "ArrowDarkGlow", "ArrowColorGlow", "ArrowLegacy"])]
    public ConfigEntry<ArrowStyleType> ArrowStyleEnum { get; private set; } =
        config.Bind("Gameplay", "ArrowStyle", ArrowStyleType.Default);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowShieldHudToggle { get; private set; } =
        config.Bind("Gameplay", "ShowShieldHud", true);

    [LocalizedLocalEnumSetting(names: ["PiPLocationTopLeft", "PiPLocationMiddleLeft", "PiPLocationBottomLeft", "PiPLocationTopRight", "PiPLocationMiddleRight", "PiPLocationBottomRight", "PiPLocationDynamic"])]
    public ConfigEntry<ParasitePiPLocation> ParasitePiPLocation { get; private set; } =
        config.Bind("Role Visuals", "ParasitePiPLocation", TownOfUs.ParasitePiPLocation.Dynamic);

    [LocalizedLocalEnumSetting(names: ["PiPSizeNormal", "PiPSizeSmall", "PiPSizeLarge"])]
    public ConfigEntry<ParasitePiPSize> ParasitePiPSize { get; private set; } =
        config.Bind("Role Visuals", "ParasitePiPSize", TownOfUs.ParasitePiPSize.Normal);

    [LocalizedLocalEnumSetting(names: ["FlashWhite", "FlashLightGray", "FlashGray", "FlashDarkGray"])]
    public ConfigEntry<GrenadeFlashColor> GrenadierFlashColor { get; private set; } =
        config.Bind("Role Visuals", "GrenadierFlashColor", GrenadeFlashColor.LightGray);
}

public enum ArrowStyleType
{
    Default,
    DarkGlow,
    ColorGlow,
    Legacy
}

public enum GrenadeFlashColor
{
    White,
    LightGray,
    Gray,
    DarkGray
}

public enum ParasitePiPLocation
{
    TopLeft,
    MiddleLeft,
    BottomLeft,
    TopRight,
    MiddleRight,
    BottomRight,
    Dynamic
}

public enum ParasitePiPSize
{
    Normal,
    Small,
    Large
}