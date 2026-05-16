using System.Collections;
using BepInEx.Configuration;
using InnerNet;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using TownOfUs.Buttons;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Patches;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs;

public class TownOfUsLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU: Mira";
    protected override bool ShouldCreateLabels => true;
    public static float OldButtonScaleFactor { get; set; }

    public override void Open()
    {
        base.Open();
        OldButtonScaleFactor = ButtonUIFactorSlider.Value;

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
        HudManagerPatches.UiGrid.ArrangeChilds();
        HudManagerPatches.ExtraUiGrid.ArrangeChilds();
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

    public static IEnumerator CoResizeSettingsUI()
    {
        while (!HudManager.Instance || !HudManagerPatches.UiGrid || !HudManagerPatches.ExtraUiGrid)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.01f);
        ResizeUI(LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ButtonUIFactorSlider.Value);
    }

    public static void ResizeUI(float scaleFactor)
    {
        var alteredScale = scaleFactor * 0.85f;
        var actualScaleVal = scaleFactor * 1.176470588235294f;
        var actualScale = new Vector3(actualScaleVal, actualScaleVal, 1);
        var baseAspect = HudManagerPatches.UiAspectPos;
        var baseGrid = HudManagerPatches.UiGrid;
        var baseUi = HudManagerPatches.UiTopRight;
        if (baseUi && baseAspect && baseGrid)
        {
            baseAspect.DistanceFromEdge = new Vector2(0.435f * scaleFactor, 0.475f * scaleFactor);

            foreach (var button in baseUi.GetComponentsInChildren<PassiveButton>(true))
            {
                if (button.gameObject == null)
                {
                    continue;
                }
                if (button.transform.name.Contains("Friends List Button"))
                {
                    button.gameObject.transform.localScale = new Vector3(0.2675f * actualScaleVal, 0.2675f * actualScaleVal, 1);
                    continue;
                }

                button.gameObject.transform.localScale = actualScale;
            }

            baseGrid.CellSize = new Vector2(alteredScale, alteredScale);
            if (baseGrid.gameObject.transform.childCount != 0)
            {
                baseGrid.ArrangeChilds();
            }
        }

        var extraAspect = HudManagerPatches.ExtraUiAspectPos;
        var extraGrid = HudManagerPatches.ExtraUiGrid;
        var extraUi = HudManagerPatches.ExtraUiTopRight;
        if (extraUi && extraAspect && extraGrid)
        {
            extraAspect.DistanceFromEdge = new Vector3(0.435f * scaleFactor, 1.25f * scaleFactor, 65f);

            foreach (var button in extraUi.GetAllChildren())
            {
                if (button.gameObject == null)
                {
                    continue;
                }
                if (button.transform.name.Contains("Modifiers"))
                {
                    button.gameObject.transform.localScale = new Vector3(0.65f * scaleFactor, 0.65f * scaleFactor, 1);
                    continue;
                }

                button.gameObject.transform.localScale = actualScale;
            }

            extraGrid.CellSize = new Vector2(alteredScale, alteredScale);
            if (extraGrid.gameObject.transform.childCount != 0)
            {
                extraGrid.ArrangeChilds();
            }
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

        if (configEntry == ButtonUIFactorSlider)
        {
            if (HudManager.InstanceExists)
            {
                ResizeUI(ButtonUIFactorSlider.Value);
            }
            OldButtonScaleFactor = ButtonUIFactorSlider.Value;
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
    
    [LocalizedLocalSliderSetting(min: 0.3f, max: 2f, suffixType: MiraNumberSuffixes.Multiplier, formatString: "0.00", displayValue: true)]
    public ConfigEntry<float> ButtonUIFactorSlider { get; private set; } =
        config.Bind("UI/Visuals", "TopRightUiScale", 1f);

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