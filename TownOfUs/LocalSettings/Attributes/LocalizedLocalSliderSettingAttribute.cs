using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.Utilities;
using TownOfUs.LocalSettings.SettingTypes;

namespace TownOfUs.LocalSettings.Attributes;

/// <summary>
/// Creates a slider setting for the <see cref="ConfigEntry{T}"/>.
/// </summary>
/// <inheritdoc/>
/// <param name="name">Name for the BepInEx config and locale key.</param>
/// <param name="description">Optional description for the BepInEx config.</param>
/// <param name="min">Minimum range.</param>
/// <param name="max">Maximum range.</param>
/// <param name="displayValue">Toggle to show or hide the value in text.</param>
/// <param name="formatString">The format string used for formating.</param>
/// <param name="roundValue">Should the value be rounded.</param>
/// <param name="suffixType">Suffix for the value.</param>
[AttributeUsage(AttributeTargets.Property)]
public class LocalizedLocalSliderSettingAttribute(
    string? name = null,
    float min = 0,
    float max = 100,
    string? description = null,
    bool displayValue = false,
    string? formatString = null,
    bool roundValue = false,
    MiraNumberSuffixes suffixType = MiraNumberSuffixes.None
    ) : LocalSliderSettingAttribute(name, description:description)
{
    private readonly string? _name = name;
    private readonly string? _description = description;

    /// <inheritdoc/>
    public override LocalizedLocalSliderSetting CreateSetting(Type tab, ConfigEntryBase configEntryBase)
    {
        return new LocalizedLocalSliderSetting(tab, configEntryBase, _name, _description, new FloatRange(min, max), displayValue, suffixType, formatString, roundValue);
    }
}
