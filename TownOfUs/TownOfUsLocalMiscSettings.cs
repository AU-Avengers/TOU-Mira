using BepInEx.Configuration;
using MiraAPI.Utilities;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Patches.Options;

namespace TownOfUs;

public class TownOfUsLocalMiscSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU: Misc";
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

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        if (configEntry == SeparateChatBubbles)
        {
            if (!HudManager.InstanceExists)
            {
                return;
            }
            TeamChatPatches.UpdateChat();
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouModifierIcons.Aftermath
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
    public ConfigEntry<bool> SeparateChatBubbles { get; private set; } =
        config.Bind("Miscellaneous", "SeparateChatBubbles", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowWelcomeMessageToggle { get; private set; } =
        config.Bind("Miscellaneous", "ShowWelcomeMessage", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowRulesOnLobbyJoinToggle { get; private set; } =
        config.Bind("Miscellaneous", "ShowRulesOnLobbyJoin", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowSummaryMessageToggle { get; private set; } =
        config.Bind("Miscellaneous", "ShowSummaryMessage", true);

    [LocalizedLocalEnumSetting(names: ["SummarySimple", "SummaryNormal", "SummaryAdvanced"])]
    public ConfigEntry<GameSummaryAppearance> SummaryMessageAppearance { get; private set; } =
        config.Bind("Miscellaneous", "SummaryMsgBreakdown", GameSummaryAppearance.Advanced);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowPracticeButtons { get; private set; } =
        config.Bind("Miscellaneous", "ShowPracticeButtons", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ZoomingInLobby { get; private set; } =
        config.Bind("Miscellaneous", "ZoomingInLobby", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ZoomingInPractice { get; private set; } =
        config.Bind("Miscellaneous", "ZoomingInPractice", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> RainbowColorAsFortegreen { get; private set; } =
        config.Bind("Miscellaneous", "RainbowColorAsFortegreen", false);
}

public enum GameSummaryAppearance
{
    Simplified,
    Normal,
    Advanced
}

public enum EndGameSummaryVisibility
{
    Hidden,
    Split,
    LeftSide,
}

public enum AutoRejoinSelection
{
    Always,
    Host,
    Client,
    Never
}