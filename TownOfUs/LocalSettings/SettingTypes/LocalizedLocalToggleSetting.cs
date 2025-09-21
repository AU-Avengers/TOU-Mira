using BepInEx.Configuration;
using MiraAPI.LocalSettings.SettingTypes;
using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace TownOfUs.LocalSettings.SettingTypes;

/// <summary>
/// Local setting class for toggles.
/// </summary>
public class LocalizedLocalToggleSetting : LocalToggleSetting
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedLocalToggleSetting"/> class.
    /// </summary>
    /// <inheritdoc/>
    public LocalizedLocalToggleSetting(
        Type tab,
        ConfigEntryBase configEntry,
        string? name = null,
        string? description = null
        ) : base(tab, configEntry, name, description)
    {
    }

    /// <inheritdoc />
    public override GameObject CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent, ref float offset, ref int order, bool last)
    {
        var toggleObject = Object.Instantiate(toggle, parent).GetComponent<ToggleButtonBehaviour>();
        var tmp = toggleObject.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        var passiveButton = toggleObject.GetComponent<PassiveButton>();
        var rollover = toggleObject.GetComponent<ButtonRolloverHandler>();
        toggleObject.gameObject.SetActive(true);
        var localeName = Name;
        TouLocale.LocalizedToggles.TryAdd(toggleObject, localeName);

        if (last && order == 1)
        {
            // Toggle in the middle
            toggleObject.transform.localPosition = new Vector3(0, 1.85f - offset, -7);
        }
        else
        {
            toggleObject.transform.localPosition = new Vector3(order == 1 ? -1.185f : 1.185f, 1.85f - offset, -7);
        }

        toggleObject.BaseText = CustomStringName.CreateAndRegister(localeName);
        UpdateToggleText(toggleObject.Text, localeName, GetValue());
        toggleObject.name = localeName;
        toggleObject.Background.color = GetValue() ? Tab!.TabAppearance.ToggleActiveColor : Tab!.TabAppearance.ToggleInactiveColor;
        passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        rollover.OverColor = Tab!.TabAppearance.ToggleHoverColor;

        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            SetValue(!GetValue());
            UpdateToggleText(toggleObject.Text, localeName, GetValue());
            toggleObject.Background.color = GetValue() ? Tab!.TabAppearance.ToggleActiveColor : Tab!.TabAppearance.ToggleInactiveColor;
        }));
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!Description.IsNullOrWhiteSpace())
            {
                tmp.text = Description;
            }
        }));
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            UpdateToggleText(toggleObject.Text, localeName, GetValue());
            toggleObject.Background.color = GetValue() ? Tab!.TabAppearance.ToggleActiveColor : Tab!.TabAppearance.ToggleInactiveColor;
        }));

        Helpers.DivideSize(toggleObject.gameObject, 1.1f);

        order++;
        if (order > 2 && !last)
        {
            offset += 0.5f;
            order = 1;
        }
        if (last)
            offset += 0.6f;

        return toggleObject.gameObject;
    }

    public static void UpdateToggleText(TextMeshPro tmp, string localeName, bool value)
    {
        tmp.text = TouLocale.Get(localeName) + ": " + DestroyableSingleton<TranslationController>.Instance.GetString(value ? StringNames.SettingsOn : StringNames.SettingsOff);
    }
}
