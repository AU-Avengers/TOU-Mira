using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Events;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

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
        var roleAvailable = PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data &&
                            PlayerControl.LocalPlayer.Data.Role;
        if ((configEntry == ParasitePiPLocation || configEntry == ParasitePiPSize) &&
            roleAvailable &&
                 PlayerControl.LocalPlayer.Data.Role is Roles.Impostor.ParasiteRole parasiteRole)
        {
            // Apply PiP changes to the Parasite (controller) side.
            parasiteRole.MarkPiPSettingsDirty(resetManualThisSession: true);
            parasiteRole.TickPiP();
        }
        else if (configEntry == ShowRoleIconOnRoleTab)
        {
            TownOfUsEventHandlers.TryGetRoleTab();
        }
        else if (configEntry == SonarTargetType && roleAvailable && PlayerControl.LocalPlayer.Data.Role is SonarRole)
        {
            var update = OptionGroupSingleton<SonarOptions>.Instance.UpdateInterval;
            if (SonarTargetType.Value is SonarTargetStyle.Arrows)
            {
                var currentHeartbeats = ModifierUtils.GetPlayersWithModifier<SonarHeartbeatTargetModifier>();
                foreach (var plr in currentHeartbeats)
                {
                    plr.RemoveModifier<SonarHeartbeatTargetModifier>();

                    Color color = Palette.PlayerColors[plr.GetDefaultAppearance().ColorId];
                    plr.AddModifier<SonarArrowTargetModifier>(PlayerControl.LocalPlayer, color, update);
                }
            }
            else
            {
                var currentArrows = ModifierUtils.GetPlayersWithModifier<SonarArrowTargetModifier>();
                foreach (var plr in currentArrows)
                {
                    plr.RemoveModifier<SonarArrowTargetModifier>();

                    Color color = Palette.PlayerColors[plr.GetDefaultAppearance().ColorId];
                    plr.AddModifier<SonarHeartbeatTargetModifier>(PlayerControl.LocalPlayer, color, update);
                }
            }
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

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowShieldHudToggle { get; private set; } =
        config.Bind("Gameplay", "ShowShieldHud", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowBasicAssassinOnHud { get; private set; } =
        config.Bind("Gameplay", "ShowBasicAssassinOnHud", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowRoleIconOnRoleTab { get; private set; } =
        config.Bind("Gameplay", "ShowRoleIconOnRoleTab", true);

    [LocalizedLocalEnumSetting(names: ["ArrowDefault", "ArrowDarkGlow", "ArrowColorGlow", "ArrowLegacy"])]
    public ConfigEntry<ArrowStyleType> ArrowStyleEnum { get; private set; } =
        config.Bind("Gameplay", "ArrowStyle", ArrowStyleType.Default);

    [LocalizedLocalEnumSetting(names: ["PiPLocationTopLeft", "PiPLocationMiddleLeft", "PiPLocationBottomLeft", "PiPLocationTopRight", "PiPLocationMiddleRight", "PiPLocationBottomRight", "PiPLocationDynamic"])]
    public ConfigEntry<ParasitePiPLocation> ParasitePiPLocation { get; private set; } =
        config.Bind("Role Visuals", "ParasitePiPLocation", TownOfUs.ParasitePiPLocation.Dynamic);

    [LocalizedLocalEnumSetting(names: ["PiPSizeNormal", "PiPSizeSmall", "PiPSizeLarge"])]
    public ConfigEntry<ParasitePiPSize> ParasitePiPSize { get; private set; } =
        config.Bind("Role Visuals", "ParasitePiPSize", TownOfUs.ParasitePiPSize.Normal);

    [LocalizedLocalEnumSetting(names: ["FlashWhite", "FlashLightGray", "FlashGray", "FlashDarkGray"])]
    public ConfigEntry<GrenadeFlashColor> GrenadierFlashColor { get; private set; } =
        config.Bind("Role Visuals", "GrenadierFlashColor", GrenadeFlashColor.LightGray);

    [LocalizedLocalEnumSetting(names: ["SonarHeartbeats", "SonarArrows"])]
    public ConfigEntry<SonarTargetStyle> SonarTargetType { get; private set; } =
        config.Bind("Role Visuals", "SonarTargetType", SonarTargetStyle.Heartbeats);
}

public enum SonarTargetStyle
{
    Heartbeats,
    Arrows
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