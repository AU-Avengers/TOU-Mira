using BepInEx.Configuration;
using MiraAPI.Utilities;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.LocalSettings.Attributes;

namespace TownOfUs;

public class TouLocalTabPractice(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => TutorialManager.InstanceExists ? "Practice" : "Lobby";
    protected override bool ShouldCreateLabels => true;

    public override void Open()
    {
        base.Open();

        foreach (var entry in TouLocale.LocalizedToggles)
        {
            var toggleObject = entry.Key;
            LocalizedLocalToggleSetting.UpdateToggleText(toggleObject.Text, entry.Value, toggleObject.onState);
        }

        foreach (var entry in TouLocale.LocalizedSliders)
        {
            var sliderObject = entry.Key;
            sliderObject.SliderObject.Title.text =
                LocalizedLocalSliderSetting.GetLocalizedValueText(sliderObject, sliderObject.LocaleKey);
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.LocalLobby,
        HideIconOnHover = false,
    };

    [LocalizedLocalSliderSetting(min: 4f, max: 15f, suffixType: MiraNumberSuffixes.Seconds, formatString: "0", displayValue: true, roundValue: true)]
    public ConfigEntry<float> AutoRejoinDelay { get; private set; } =
        config.Bind("End Game Screen", "AutoRejoinDelay", 4f);

    [LocalizedLocalEnumSetting(names: ["EndSumHidden", "EndSumSplit", "EndSumLeftSide"])]
    public ConfigEntry<EndGameSummaryVisibility> EndSummaryVisibility { get; private set; } =
        config.Bind("End Game Screen", "EndSummaryVisibility", EndGameSummaryVisibility.LeftSide);

    [LocalizedLocalEnumSetting(names: ["EndRejoinAlways", "EndRejoinHost", "EndRejoinClient", "EndRejoinNever"])]
    public ConfigEntry<AutoRejoinSelection> AutoRejoinMode { get; private set; } =
        config.Bind("End Game Screen", "AutoRejoinSelection", AutoRejoinSelection.Always);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowWelcomeMessageToggle { get; private set; } =
        config.Bind("Lobby", "ShowWelcomeMessage", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowRulesOnLobbyJoinToggle { get; private set; } =
        config.Bind("Lobby", "ShowRulesOnLobbyJoin", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowSummaryMessageToggle { get; private set; } =
        config.Bind("Lobby", "ShowSummaryMessage", true);

    [LocalizedLocalEnumSetting(names: ["SummarySimple", "SummaryNormal", "SummaryAdvanced"])]
    public ConfigEntry<GameSummaryAppearance> SummaryMessageAppearance { get; private set; } =
        config.Bind("Lobby", "SummaryMsgBreakdown", GameSummaryAppearance.Advanced);

    [LocalizedLocalEnumSetting(names: ["DraftAudioStart", "DraftAudioYourTurn", "DraftAudioNone"])]
    public ConfigEntry<DraftAudioCueMode> DraftAudioCue { get; private set; } =
        config.Bind("Lobby", "DraftAudioCue", DraftAudioCueMode.None);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ZoomingInLobby { get; private set; } =
        config.Bind("Lobby", "ZoomingInLobby", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ZoomingInPractice { get; private set; } =
        config.Bind("Practice Mode", "ZoomingInPractice", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowPracticeButtons { get; private set; } =
        config.Bind("Practice Mode", "ShowPracticeButtons", true);
}