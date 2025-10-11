using BepInEx.Configuration;
using MiraAPI.LocalSettings.SettingTypes;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace TownOfUs.LocalSettings.SettingTypes;

/// <summary>
/// Local setting class for sliders.
/// </summary>
public class LocalizedLocalSliderSetting : LocalSliderSetting
{
    /// <summary>
    /// Gets the range of the slider.
    /// </summary>
    public FloatRange SliderRange { get; }

    /// <summary>
    /// Gets a value indicating whether the value should be displayed next to name.
    /// </summary>
    public bool DisplayValue { get; }

    /// <summary>
    /// Gets a format for the text to use to format the number.
    /// </summary>
    public string FormatString { get; }

    /// <summary>
    /// Gets a value indicating whether the value should be rounded.
    /// </summary>
    public bool RoundValue { get; }

    /// <summary>
    /// Gets the suffix for the number value.
    /// </summary>
    public MiraNumberSuffixes SuffixType { get; }

    /// <summary>
    /// Gets the locale name.
    /// </summary>
    public string LocaleKey { get; }

    /// <summary>
    /// Gets the SliderBar object.
    /// </summary>
    public SlideBar SliderObject { get; private set; }

    /// <summary>
    /// Gets the value of the slider beforehand.
    /// </summary>
    public float OldValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedLocalSliderSetting"/> class.
    /// </summary>
    /// <inheritdoc/>
    /// <param name="name">Name for the BepInEx config and locale key.</param>
    /// <param name="description">Optional description for the BepInEx config.</param>
    /// <param name="sliderRange">The value range.</param>
    /// <param name="suffixType">The suffix used for formating.</param>
    /// <param name="displayValue">Toggle to show or hide the value in text.</param>
    /// <param name="formatString">The format string used for formating.</param>
    /// <param name="roundValue">Determines if the value should be rounded.</param>
    /// <param name="tab">The tab that the option belongs to.</param>
    /// <param name="configEntry">The BepInEx config entry the option is attached to.</param>
    public LocalizedLocalSliderSetting(
        Type tab,
        ConfigEntryBase configEntry,
        string? name = null,
        string? description = null,
        FloatRange? sliderRange = null,
        bool displayValue = false,
        MiraNumberSuffixes? suffixType = null,
        string? formatString = null,
        bool roundValue = false)
        : base(tab, configEntry, name, description)
    {
        LocaleKey = Name;
        SliderRange = sliderRange ?? new FloatRange(0, 100);
        DisplayValue = displayValue;
        SuffixType = suffixType ?? MiraNumberSuffixes.None;
        FormatString = formatString ?? "0.0";
        RoundValue = roundValue;
    }

    /// <inheritdoc />
    public override GameObject CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent,
        ref float offset, ref int order, bool last)
    {
        var newSlider = Object.Instantiate(slider, parent).GetComponent<SlideBar>();
        var rollover = newSlider.GetComponent<ButtonRolloverHandler>();
        newSlider.Title =
            newSlider.transform.FindChild("Text_TMP")
                .GetComponent<TextMeshPro>(); // Why the hell slider has a title property that is not even assigned???
        newSlider.Title.GetComponent<TextTranslatorTMP>().Destroy();
        newSlider.gameObject.SetActive(true);
        newSlider.Bar.color = Tab!.TabAppearance.SliderColor;
        TouLocale.LocalizedSliders.TryAdd(this, LocaleKey);
        SliderObject = newSlider;

        if (order == 2)
        {
            offset += 0.5f;
        }

        newSlider.Bar.transform.localPosition = new Vector3(2.85f, 0, 0);
        newSlider.transform.localPosition = new Vector3(-2.12f, 1.85f - offset, -7);
        newSlider.name = Name;
        newSlider.Range = new FloatRange(-1.5f, 1.5f);
        OldValue = GetValue();
        newSlider.SetValue(Mathf.InverseLerp(SliderRange.min, SliderRange.max, GetValue()));
        rollover.OutColor = Tab!.TabAppearance.SliderColor;
        rollover.OverColor = Tab!.TabAppearance.SliderHoverColor;
        newSlider.Title.transform.localPosition = new Vector3(0.5f, 0, -1f);
        newSlider.Title.horizontalAlignment =
            DisplayValue ? HorizontalAlignmentOptions.Left : HorizontalAlignmentOptions.Center;
        newSlider.Title.text = GetValueText();

        newSlider.OnValueChange.AddListener((UnityAction)(() =>
        {
            OldValue = GetValue();
            SetValue(RoundValue
                ? Mathf.Round(Mathf.Lerp(SliderRange.min, SliderRange.max, newSlider.Value))
                : Mathf.Lerp(SliderRange.min, SliderRange.max, newSlider.Value));

            newSlider.Title.text = GetValueText();
        }));

        order = 1;
        offset += 0.5f;
        return newSlider.gameObject;
    }

    /// <inheritdoc/>
    protected override string GetValueText()
    {
        if (DisplayValue)
        {
            var value = GetValue();
            var formated = Helpers.FormatValue(value, SuffixType, FormatString);
            var maxFormated = Helpers.FormatValue(SliderRange.max, SuffixType, FormatString);
            return
                $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{TouLocale.Get(LocaleKey)}: <b>{formated} / {maxFormated}</font></b>";
        }

        return
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{TouLocale.Get(LocaleKey)}</font></b>";
    }

    /// <inheritdoc/>
    public static string GetLocalizedValueText(LocalizedLocalSliderSetting slider, string localeName)
    {
        if (slider.DisplayValue)
        {
            var value = slider.GetValue();
            var formated = Helpers.FormatValue(value, slider.SuffixType, slider.FormatString);
            var maxFormated = Helpers.FormatValue(slider.SliderRange.max, slider.SuffixType, slider.FormatString);
            return
                $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{TouLocale.Get(localeName)}: <b>{formated} / {maxFormated}</font></b>";
        }

        return
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{TouLocale.Get(localeName)}</font></b>";
    }
}