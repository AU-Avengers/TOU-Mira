using System.Collections;
using BepInEx.Configuration;
using InnerNet;
using MiraAPI;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using TownOfUs.Buttons;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Patches;
using TownOfUs.Patches.Misc;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs;

public class TouLocalTabButtons(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "UI / UX";
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
        if (configEntry == OffsetButtonsToggle)
        {
            if ((AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
                 !TutorialManager.InstanceExists) || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data ||
                PlayerControl.LocalPlayer.Data.Role == null || !ShipStatus.Instance)
            {
                return;
            }

            var role = PlayerControl.LocalPlayer.Data.Role;

            var fakeVent = CustomButtonSingleton<FakeVentButton>.Instance;
            fakeVent.SetActive(fakeVent.Enabled(role), role);
            if (role is not ITownOfUsRole touRole)
            {
                return;
            }

            touRole.OffsetButtons();
        }
        else if (configEntry == WikiOnBottomRow || configEntry == ZoomOnBottomRow)
        {
            MiraApiSettings.SetUpButtonPositions();
        }
        else if (configEntry == ModStampPlacement)
        {
            ModStampPatch.StampPlacement = ModStampPlacement.Value;
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.LocalButtons,
        HideIconOnHover = false,
    };

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> WikiOnBottomRow { get; private set; } =
        config.Bind("UI / Visuals", "WikiOnBottomRow", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ZoomOnBottomRow { get; private set; } =
        config.Bind("UI / Visuals", "ZoomOnBottomRow", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowShieldHudToggle { get; private set; } =
        config.Bind("UI / Visuals", "ShowShieldHud", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowBasicAssassinOnHud { get; private set; } =
        config.Bind("UI / Visuals", "ShowBasicAssassinOnHud", true);

    [LocalizedLocalEnumSetting(names: ["ModStampTopLeft", "ModStampTopRight", "ModStampBottomLeft", "ModStampBottomRight"])]
    public ConfigEntry<ModStampLocation> ModStampPlacement { get; private set; } =
        config.Bind("UI / Visuals", "ModStampPlacement", ModStampLocation.TopRight);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> PreciseCooldownsToggle { get; private set; } =
        config.Bind("Abilities", "PreciseCooldowns", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> OffsetButtonsToggle { get; private set; } =
        config.Bind("Abilities", "OffsetButtons", false);
}

public enum ModStampLocation
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}