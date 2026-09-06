using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Patches.Options;

namespace TownOfUs;

public class TouLocalTabPreferences(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "<size=65%>Preferences</size>";
    protected override bool ShouldCreateLabels => true;

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

    [LocalSliderSetting(min: 4f, max: 15f, suffixType: MiraNumberSuffixes.Seconds, formatString: "0", displayValue: true, roundValue: true)]
    public ConfigEntry<float> AutoRejoinDelay { get; private set; } =
        config.Bind("End Game Screen", "AutoRejoinDelay", 4f);

    [LocalEnumSetting(names: ["EndRejoinAlways", "EndRejoinHost", "EndRejoinClient", "EndRejoinNever"])]
    public ConfigEntry<AutoRejoinSelection> AutoRejoinMode { get; private set; } =
        config.Bind("End Game Screen", "AutoRejoinSelection", AutoRejoinSelection.Always);

    [LocalEnumSetting(names: ["EndSumHidden", "EndSumSplit", "EndSumLeftSide"])]
    public ConfigEntry<EndGameSummaryVisibility> EndSummaryVisibility { get; private set; } =
        config.Bind("End Game Screen", "EndSummaryVisibility", EndGameSummaryVisibility.LeftSide);

    [LocalToggleSetting]
    public ConfigEntry<bool> SortGuessingByAlignmentToggle { get; private set; } =
        config.Bind("Gameplay", "SortGuessingByAlignment", false);

    [LocalToggleSetting]
    public ConfigEntry<bool> SeparateChatBubbles { get; private set; } =
        config.Bind("Gameplay", "SeparateChatBubbles", false);

    [LocalToggleSetting]
    public ConfigEntry<bool> DeadSeeGhostsToggle { get; private set; } = config.Bind("Miscellaneous", "DeadSeeGhosts", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> ShowVentsToggle { get; private set; } = config.Bind("Miscellaneous", "ShowVents", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> RoleIconOnReveal { get; private set; } =
        config.Bind("Miscellaneous", "RoleIconOnReveal", false);

    [LocalToggleSetting]
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
    Both,
    None
}