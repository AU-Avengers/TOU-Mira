using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.Utilities;
using TownOfUs.LocalSettings.SettingTypes;

namespace TownOfUs.LocalSettings.Attributes;

/// <summary>
/// Creates a number setting for the <see cref="ConfigEntry{T}"/>.
/// </summary>
/// <param name="name">Name for the BepInEx config and locale key.</param>
/// <param name="description">Optional description for the BepInEx config.</param>
/// <param name="min">Minimum range.</param>
/// <param name="max">Maximum range.</param>
/// <param name="increment">Increment per use.</param>
/// <param name="suffixType">Suffix for the value.</param>
/// <param name="formatString">Format string used when formating.</param>
/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property)]
public class LocalizedLocalNumberSettingAttribute(
    string? name = null,
    string? description = null,
    float min = 1,
    float max = 5,
    float increment = 1,
    MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
    string? formatString = null
) : LocalNumberSettingAttribute(name, description)
{
    private readonly string? _name = name;
    private readonly string? _description = description;

    /// <inheritdoc/>
    public override LocalizedLocalNumberSetting CreateSetting(Type tab, ConfigEntryBase configEntryBase)
    {
        return new LocalizedLocalNumberSetting(tab, configEntryBase, _name, _description, new FloatRange(min, max),
            increment, suffixType, formatString);
    }
}