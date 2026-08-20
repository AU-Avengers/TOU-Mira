using BepInEx.Configuration;

namespace TownOfUs;

#pragma warning disable S2325 // This is for compatibility with older tou extension mods.

[Obsolete("Please use the new TouLocalTab classes instead.")]
public class TownOfUsLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU: Mira";
    public override bool ShouldCreateButton => false;
    protected override bool ShouldCreateLabels => false;

    public ConfigEntry<bool> DeadSeeGhostsToggle =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.DeadSeeGhostsToggle;

    public ConfigEntry<bool> ShowVentsToggle =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.ShowVentsToggle;

    public ConfigEntry<bool> WikiOnBottomRow =>
        LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.WikiOnBottomRow;

    public ConfigEntry<bool> ZoomOnBottomRow =>
        LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ZoomOnBottomRow;

    public ConfigEntry<bool> PreciseCooldownsToggle =>
        LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.PreciseCooldownsToggle;

    public ConfigEntry<bool> OffsetButtonsToggle =>
        LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.OffsetButtonsToggle;

    public ConfigEntry<bool> ColorPlayerNameToggle =>
        LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.ColorPlayerNameToggle;

    public ConfigEntry<NameStyle> RoleNameStyle =>
        LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.RoleNameStyle;

    public ConfigEntry<ModStampLocation> ModStampPlacement =>
        LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ModStampPlacement;
}
#pragma warning restore S2325 // This is for compatibility with older tou extension mods.
