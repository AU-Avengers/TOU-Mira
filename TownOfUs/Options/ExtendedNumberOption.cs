using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options;

public class ExtendedNumberOption : ModdedNumberOption
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedNumberOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a float.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    /// <param name="color">Optional color for the option notification.</param>
    /// <param name="asset">Optional icon for the option notification.</param>
    /// <param name="assetName">Optional icon tmp name for the option notification.</param>
    /// <param name="assetScale">Optional icon scale for the option notification.</param>
    /// <param name="zeroBehavior">Determines what is shown for zero. Options: ∞, #, or a word value (such as Off).</param>
    /// <param name="negativeBehavior">Determines what is shown for negative 1. Options: ∞, #, or a word value (such as Off).</param>
    /// <param name="halfIncrements">Whether increments can be split in half.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ExtendedNumberOption(
        string title,
        float defaultValue,
        float min,
        float max,
        float increment,
        string zeroBehavior = "#",
        string negativeBehavior = "#",
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        Color? color = null,
        LoadableAsset<Sprite>? asset = null,
        string assetName = "",
        float assetScale = 1,
        bool halfIncrements = false,
        bool includeInPreset = true) : base(title, defaultValue, min, max, increment, zeroBehavior, negativeBehavior, suffixType, formatString, halfIncrements, includeInPreset)
    {
        OptionColor = color ?? new Color(0.7333f, 0.7333f, 0.7333f, 1);
        if (asset != null && assetName != "")
        {
            ResourceAsset = asset;
            AssetName = assetName;
            AssetScale = assetScale;
        }
    }

    public Color OptionColor { get; set; }
    public LoadableAsset<Sprite> ResourceAsset { get; set; }
    public string AssetName { get; set; }
    public float AssetScale { get; set; } = 1f;
    /// <inheritdoc />
    public override OptionNotifConfiguration Configuration => new(OptionColor, ResourceAsset != null ? TmpSpriteUtils.CreateSpriteAsset(ResourceAsset.LoadAsset(), AssetName, AssetScale) : null!);
}

public class AmountChanceOption : ExtendedNumberOption
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmountChanceOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a float.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    /// <param name="color">Optional color for the option notification.</param>
    /// <param name="asset">Optional icon for the option notification.</param>
    /// <param name="assetName">Optional icon tmp name for the option notification.</param>
    /// <param name="assetScale">Optional icon scale for the option notification.</param>
    /// <param name="zeroBehavior">Determines what is shown for zero. Options: ∞, #, or a word value (such as Off).</param>
    /// <param name="negativeBehavior">Determines what is shown for negative 1. Options: ∞, #, or a word value (such as Off).</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public AmountChanceOption(
        string title,
        float defaultValue,
        float min,
        float max,
        float increment,
        string zeroBehavior = "#",
        string negativeBehavior = "#",
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        Color? color = null,
        LoadableAsset<Sprite>? asset = null,
        string assetName = "",
        float assetScale = 1,
        bool includeInPreset = true) : base(title, defaultValue, min, max, increment, zeroBehavior, negativeBehavior,
        suffixType, formatString, color, asset, assetName, assetScale, false, includeInPreset)
    {

    }

    /// <inheritdoc />
    protected override void OnValueChanged(float newValue)
    {
        Value = Mathf.Clamp(newValue, Min, Max);

        if (OptionBehaviour is NumberOption opt)
        {
            opt.Value = Value;
        }
    }

    public void AddSettingsChangeMessage(NotificationPopper notif, StringNames key, string title,
        string roleCount, string roleChance)
    {
        string item;
        var text = Configuration.PopUpTextColor.ToTextColor();
        if (Configuration.PopUpIconTmp != null)
        {
            item = TranslationController.Instance.GetString(
                StringNames.LobbyChangeSettingNotificationRole,
                string.Concat(
                    "<sprite name=\"",
                    Configuration.PopUpIconTmp.name,
                    "\"><font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
                    text,
                    title,
                    "</color></font>"),
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleCount + "</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleChance
            );
        }
        else
        {
            item = TranslationController.Instance.GetString(
                StringNames.LobbyChangeSettingNotificationRole,
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" +
                text +
                title + "</color></font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleCount + "</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleChance);
        }

        notif.SettingsChangeMessageLogic(key, item, true);
    }
}