using BepInEx.Configuration;
using InnerNet;
using MiraAPI.Hud;
using TownOfUs.Buttons;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Roles;

namespace TownOfUs;

public class TownOfUsLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "ToU: Mira";
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
        if (configEntry == OffsetButtonsToggle)
        {
            if ((AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
                 !TutorialManager.InstanceExists) || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null ||
                PlayerControl.LocalPlayer.Data.Role == null || !ShipStatus.Instance)
            {
                return;
            }

            var role = PlayerControl.LocalPlayer.Data.Role;

            var fakeVent = CustomButtonSingleton<FakeVentButton>.Instance;
            fakeVent.SetActive(fakeVent.Enabled(role), role);
            if (role is not ITownOfUsRole touRole)
            {
                return;
            }

            touRole.OffsetButtons();
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.TouMiraIcon
    };

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> DeadSeeGhostsToggle { get; private set; } = config.Bind("Gameplay", "DeadSeeGhosts", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ShowVentsToggle { get; private set; } = config.Bind("Gameplay", "ShowVents", true);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> PreciseCooldownsToggle { get; private set; } =
        config.Bind("UI/Visuals", "PreciseCooldowns", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> OffsetButtonsToggle { get; private set; } =
        config.Bind("UI/Visuals", "OffsetButtons", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> ColorPlayerNameToggle { get; private set; } =
        config.Bind("UI/Visuals", "ColorPlayerName", false);

    [LocalizedLocalToggleSetting]
    public ConfigEntry<bool> SeparateChatBubbles { get; private set; } =
        config.Bind("Miscellaneous", "SeparateChatBubbles", false);

    /*[LocalizedLocalToggleSetting]
    public ConfigEntry<bool> UseSeparateRedChat { get; private set; } =
        config.Bind("UI/Visuals", "UseSeparateRedChat", true);*/

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
}

public enum GameSummaryAppearance
{
    Simplified,
    Normal,
    Advanced
}