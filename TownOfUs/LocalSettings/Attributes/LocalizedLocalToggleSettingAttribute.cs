using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;

namespace TownOfUs.LocalSettings.Attributes;

/// <summary>
/// Creates a toggle setting for the <see cref="ConfigEntry{T}"/>.
/// </summary>
/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property)]
public class LocalizedLocalToggleSettingAttribute(
    string? name = null,
    string? description = null
) : LocalToggleSettingAttribute(name, description)
{
    private readonly string? _name = name;
    private readonly string? _description = description;

    /// <inheritdoc/>
    public override LocalizedLocalToggleSetting CreateSetting(Type tab, ConfigEntryBase configEntryBase)
    {
        return new LocalizedLocalToggleSetting(tab, configEntryBase, _name, _description);
    }
}