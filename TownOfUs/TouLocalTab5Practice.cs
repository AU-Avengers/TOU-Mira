using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;

namespace TownOfUs;

public class TouLocalTabPractice(ConfigFile config) : LocalSettingsTab(config)
{
    public static DraftAudioCueMode CurrentDraftAudioCueMode { get; private set; } = DraftAudioCueMode.None;

    public override string TabName => "<size=50%>Lobby / Practice</size>";
    protected override bool ShouldCreateLabels => true;

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.LocalLobby,
        HideIconOnHover = false,
    };

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowWelcomeMessageToggle { get; private set; } =
        config.Bind("Lobby", "ShowWelcomeMessage", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowRulesOnLobbyJoinToggle { get; private set; } =
        config.Bind("Lobby", "ShowRulesOnLobbyJoin", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowSummaryMessageToggle { get; private set; } =
        config.Bind("Lobby", "ShowSummaryMessage", true);

    [LocalEnumSetting(names: ["SummarySimple", "SummaryNormal", "SummaryAdvanced"])]
    public ConfigEntry<GameSummaryAppearance> SummaryMessageAppearance { get; private set; } =
        config.Bind("Lobby", "SummaryMsgBreakdown", GameSummaryAppearance.Advanced);

    [LocalEnumSetting(names: ["DraftAudioStart", "DraftAudioYourTurn", "DraftAudioBoth", "DraftAudioNone"])]
    public ConfigEntry<DraftAudioCueMode> DraftAudioCue { get; private set; } =
        config.Bind("Lobby", "DraftAudioCue", DraftAudioCueMode.None);

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        if (configEntry == DraftAudioCue)
        {
            CurrentDraftAudioCueMode = DraftAudioCue.Value;
        }
    }

    [LocalToggleSetting]
    public ConfigEntry<bool> ZoomingInLobby { get; private set; } =
        config.Bind("Lobby", "ZoomingInLobby", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> ZoomingInPractice { get; private set; } =
        config.Bind("Practice Mode", "ZoomingInPractice", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowPracticeButtons { get; private set; } =
        config.Bind("Practice Mode", "ShowPracticeButtons", true);
}