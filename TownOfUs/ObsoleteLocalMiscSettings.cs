using BepInEx.Configuration;

namespace TownOfUs;

#pragma warning disable S2325;
[Obsolete("Please use the new TouLocalTab classes instead.")]
public class TownOfUsLocalMiscSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU: Misc";
    public override bool ShouldCreateButton => false;
    protected override bool ShouldCreateLabels => false;

    public ConfigEntry<float> AutoRejoinDelay =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.AutoRejoinDelay;

    public ConfigEntry<EndGameSummaryVisibility> EndSummaryVisibility =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.EndSummaryVisibility;

    public ConfigEntry<AutoRejoinSelection> AutoRejoinMode =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.AutoRejoinMode;

    public ConfigEntry<bool> SeparateChatBubbles =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.SeparateChatBubbles;

    public ConfigEntry<bool> ShowWelcomeMessageToggle =>
        LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ShowWelcomeMessageToggle;

    public ConfigEntry<bool> ShowRulesOnLobbyJoinToggle =>
        LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ShowRulesOnLobbyJoinToggle;

    public ConfigEntry<bool> ShowSummaryMessageToggle =>
        LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ShowSummaryMessageToggle;

    public ConfigEntry<GameSummaryAppearance> SummaryMessageAppearance =>
        LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.SummaryMessageAppearance;

    public ConfigEntry<bool> ShowPracticeButtons =>
        LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ShowPracticeButtons;

    public ConfigEntry<bool> ZoomingInLobby =>
        LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ZoomingInLobby;

    public ConfigEntry<bool> ZoomingInPractice =>
        LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ZoomingInPractice;

    public ConfigEntry<bool> RainbowColorAsFortegreen =>
        LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.RainbowColorAsFortegreen;
}
#pragma warning restore S2325;