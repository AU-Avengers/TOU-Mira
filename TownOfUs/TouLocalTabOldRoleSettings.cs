using BepInEx.Configuration;

namespace TownOfUs;

#pragma warning disable S2325 // This is for compatibility with older tou extension mods.

[Obsolete("Please use the new TouLocalTab classes instead.")]
public class TownOfUsLocalRoleSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU:M Roles";
    public override bool ShouldCreateButton => false;
    protected override bool ShouldCreateLabels => false;

    public ConfigEntry<bool> SortGuessingByAlignmentToggle =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.SortGuessingByAlignmentToggle;

    public ConfigEntry<bool> UseCrewmateTeamColorToggle =>
        LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.UseCrewmateTeamColorToggle;

    public ConfigEntry<bool> ShowShieldHudToggle =>
        LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ShowShieldHudToggle;

    public ConfigEntry<bool> ShowBasicAssassinOnHud =>
        LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ShowBasicAssassinOnHud;

    public ConfigEntry<ArrowStyleType> ArrowStyleEnum =>
        LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.ArrowStyleEnum;

    public ConfigEntry<ParasitePiPLocation> ParasitePiPLocation =>
        LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.ParasitePiPLocation;

    public ConfigEntry<ParasitePiPSize> ParasitePiPSize =>
        LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.ParasitePiPSize;

    public ConfigEntry<GrenadeFlashColor> GrenadierFlashColor =>
        LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.GrenadierFlashColor;

    public ConfigEntry<SonarTargetStyle> SonarTargetType =>
        LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.SonarTargetType;
}
#pragma warning restore S2325 // This is for compatibility with older tou extension mods.
