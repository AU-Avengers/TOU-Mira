using BepInEx.Configuration;
using InnerNet;
using MiraAPI.Hud;
using TownOfUs.Buttons;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Patches;
using TownOfUs.Roles;

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
            sliderObject.SliderObject.Title.text =
                LocalizedLocalSliderSetting.GetLocalizedValueText(sliderObject, sliderObject.LocaleKey);
        }
    }

    public static void SetUpButtonPositions()
    {
        var topUi = HudManagerPatches.UiTopRight;
        var extraTopUi = HudManagerPatches.ExtraUiTopRight;
        var wikiButton = HudManagerPatches.WikiButton;
        var zoomButton = HudManagerPatches.ZoomButton;
        var subButton = HudManagerPatches.SubmergedFloorButton;
        var modDisplay = HudManagerPatches.ModifierDisplayOnRight ? HudManagerPatches.ModifierDisplayObject : null;
        ResetButtonPositions();
        if (topUi && extraTopUi)
        {
            var opts = LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance;
            wikiButton?.transform.SetParent(opts.WikiOnBottomRow.Value ? extraTopUi.transform : topUi.transform);
            zoomButton?.transform.SetParent(opts.ZoomOnBottomRow.Value ? extraTopUi.transform : topUi.transform);
            subButton?.transform.SetParent(extraTopUi.transform);
            modDisplay?.transform.SetParent(extraTopUi.transform);
        }
    }

    public static void ResetButtonPositions()
    {
        var topUi = HudManagerPatches.UiTopRight;
        var extraTopUi = HudManagerPatches.ExtraUiTopRight;
        var subButton = HudManagerPatches.SubmergedFloorButton;
        var modDisplay = HudManagerPatches.ModifierDisplayOnRight ? HudManagerPatches.ModifierDisplayObject : null;
        if (topUi && extraTopUi)
        {
            var wikiButton = HudManagerPatches.WikiButton;
            var zoomButton = HudManagerPatches.ZoomButton;
            wikiButton?.transform.SetParent(null);
            zoomButton?.transform.SetParent(null);
            subButton?.transform.SetParent(null);
            modDisplay?.transform.SetParent(null);
        }
    }

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        if (configEntry == OffsetButtonsToggle)
        {
            if ((AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
                 !TutorialManager.InstanceExists) || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null ||
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

        if (configEntry == WikiOnBottomRow || configEntry == ZoomOnBottomRow)
        {
            SetUpButtonPositions();
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
    public ConfigEntry<bool> WikiOnBottomRow { get; private set; } =
        config.Bind("UI/Visuals", "WikiOnBottomRow", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ZoomOnBottomRow { get; private set; } =
        config.Bind("UI/Visuals", "ZoomOnBottomRow", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> PreciseCooldownsToggle { get; private set; } =
        config.Bind("UI/Visuals", "PreciseCooldowns", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> OffsetButtonsToggle { get; private set; } =
        config.Bind("UI/Visuals", "OffsetButtons", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ColorPlayerNameToggle { get; private set; } =
        config.Bind("UI/Visuals", "ColorPlayerName", false);
}