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
/// Local setting class for numbers.
/// </summary>
public class LocalizedLocalNumberSetting : LocalNumberSetting
{
    /// <summary>
    /// Gets the range of the button.
    /// </summary>
    public FloatRange NumberRange { get; }

    /// <summary>
    /// Gets the increment of the value when button is pressed.
    /// </summary>
    public float Increment { get; }

    /// <summary>
    /// Gets a format for the text to use to format the number.
    /// </summary>
    public string FormatString { get; }

    /// <summary>
    /// Gets the suffix for the number value.
    /// </summary>
    public MiraNumberSuffixes SuffixType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedLocalNumberSetting"/> class.
    /// </summary>
    /// <inheritdoc/>
    /// <param name="name">Name for the BepInEx config and locale key.</param>
    /// <param name="description">Optional description for the BepInEx config.</param>
    /// <param name="numberRange">The value range.</param>
    /// <param name="increment">The increment per click.</param>
    /// <param name="suffixType">The suffix used for formating.</param>
    /// <param name="formatString">The format string used for formating.</param>
    /// <param name="tab">The tab that the option belongs to.</param>
    /// <param name="configEntry">The BepInEx config entry the option is attached to.</param>
    public LocalizedLocalNumberSetting(
        Type tab,
        ConfigEntryBase configEntry,
        string? name = null,
        string? description = null,
        FloatRange? numberRange = null,
        float? increment = null,
        MiraNumberSuffixes? suffixType = null,
        string? formatString = null)
        : base(tab, configEntry, name, description)
    {
        SuffixType = suffixType ?? MiraNumberSuffixes.None;
        NumberRange = numberRange ?? new FloatRange(1, 5);
        Increment = increment ?? 1;
        FormatString = formatString ?? "0";
    }

    /// <inheritdoc />
    public override GameObject CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent, ref float offset, ref int order, bool last)
    {
        var button = Object.Instantiate(toggle, parent).GetComponent<PassiveButton>();
        var tmp = button.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        var rollover = button.GetComponent<ButtonRolloverHandler>();
        tmp.GetComponent<TextTranslatorTMP>().Destroy();
        button.gameObject.SetActive(true);

        var toggleComp = button.GetComponent<ToggleButtonBehaviour>();
        var background = toggleComp.Background;
        var highlight = button.transform.FindChild("ButtonHighlight")?.GetComponent<SpriteRenderer>();
        if (highlight != null)
        {
            highlight.color = Tab!.TabAppearance.NumberHoverColor;
            highlight.gameObject.SetActive(false);
        }
        toggleComp.Destroy();

        if (last && order == 1)
        {
            // Button in the middle
            button.transform.localPosition = new Vector3(0, 1.85f - offset, -7);
        }
        else
        {
            button.transform.localPosition = new Vector3(order == 1 ? -1.185f : 1.185f, 1.85f - offset, -7);
        }

        tmp.text = GetValueText();
        button.name = Name;
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        rollover.OutColor = Tab!.TabAppearance.NumberColor;
        rollover.OverColor = Tab!.TabAppearance.NumberHoverColor;
        background.color = Tab!.TabAppearance.NumberColor;

        button.OnClick.AddListener((UnityAction)(() =>
        {
            float value = GetValue();
            value += Increment;
            if (value > NumberRange.max)
            {
                value = NumberRange.min;
            }

            SetValue(value);
            tmp.text = GetValueText();
        }));
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!Description.IsNullOrWhiteSpace())
            {
                tmp.text = Description;
            }
            highlight?.gameObject.SetActive(true);
        }));
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            tmp.text = GetValueText();
            highlight?.gameObject.SetActive(false);
        }));

        Helpers.DivideSize(button.gameObject, 1.1f);

        order++;
        if (order > 2 && !last)
        {
            offset += 0.5f;
            order = 1;
        }
        if (last)
            offset += 0.6f;

        return button.gameObject;
    }

    /// <inheritdoc/>
    protected override string GetValueText()
    {
        var value = GetValue();
        var formated = Helpers.FormatValue(value, SuffixType, FormatString);
        return $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{TouLocale.Get(Name)}: <b>{formated}</font></b>";
    }
}
