using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;

namespace TownOfUs.LocalSettings.Attributes;

/// <summary>
/// Creates an enum setting for the <see cref="ConfigEntry{T}"/>.
/// </summary>
/// <param name="names">Optional custom enum names.</param>
/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property)]
public class LocalizedLocalEnumSettingAttribute(
    string? name = null,
    string? description = null,
    string[]? names = null
    ) : LocalEnumSettingAttribute(name, description)
{
    private readonly string? _name = name;
    private readonly string? _description = description;

    /// <inheritdoc/>
    public override LocalizedLocalEnumSetting CreateSetting(Type tab, ConfigEntryBase configEntryBase)
    {
        return new LocalizedLocalEnumSetting(tab, configEntryBase, configEntryBase.SettingType, _name, _description, names);
    }
}
