using BepInEx.Configuration;
using MiraAPI.Utilities;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Patches.Options;

namespace TownOfUs;

public class TouLocalTabPreferences(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "<size=65%>Preferences</size>";
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
        TabIcon = TouAssets.LocalPreferences,
        HideIconOnHover = false,
    };

    [LocalizedLocalSliderSetting(min: 4f, max: 15f, suffixType: MiraNumberSuffixes.Seconds, formatString: "0", displayValue: true, roundValue: true)]
    public ConfigEntry<float> AutoRejoinDelay { get; private set; } =
        config.Bind("End Game Screen", "AutoRejoinDelay", 4f);

    [LocalizedLocalEnumSetting(names: ["EndRejoinAlways", "EndRejoinHost", "EndRejoinClient", "EndRejoinNever"])]
    public ConfigEntry<AutoRejoinSelection> AutoRejoinMode { get; private set; } =
        config.Bind("End Game Screen", "AutoRejoinSelection", AutoRejoinSelection.Always);

    [LocalizedLocalEnumSetting(names: ["EndSumHidden", "EndSumSplit", "EndSumLeftSide"])]
    public ConfigEntry<EndGameSummaryVisibility> EndSummaryVisibility { get; private set; } =
        config.Bind("End Game Screen", "EndSummaryVisibility", EndGameSummaryVisibility.LeftSide);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> SortGuessingByAlignmentToggle { get; private set; } =
        config.Bind("Gameplay", "SortGuessingByAlignment", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> DeadSeeGhostsToggle { get; private set; } = config.Bind("Miscellaneous", "DeadSeeGhosts", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowVentsToggle { get; private set; } = config.Bind("Miscellaneous", "ShowVents", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> SeparateChatBubbles { get; private set; } =
        config.Bind("Miscellaneous", "SeparateChatBubbles", false);

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

public enum DraftAudioCueMode
{
    Start,
    YourTurn,
    None
}