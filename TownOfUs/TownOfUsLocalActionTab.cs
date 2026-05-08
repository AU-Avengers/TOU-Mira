using BepInEx.Configuration;
using MiraAPI.GameEnd;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using TownOfUs.GameOver;
using TownOfUs.LocalSettings.SettingTypes;
using MiraAPI.LocalSettings.Attributes;
using TownOfUs.Patches;

namespace TownOfUs;

public class TownOfUsLocalActionTab(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "Host Actions";
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
        TabIcon = TouAssets.ActionSprite,
        HideIconOnHover = true,
    };

    [LocalSettingsButton]
    public LocalSettingsButton SelfKillButton { get; private set; } = new("Self Kill", TriggerSelfKill);
    private static void TriggerSelfKill()
    {
        DoActionType(BindActionType.SelfKill);
    }

    [LocalSettingsButton]
    public LocalSettingsButton AbortGameButton { get; private set; } = new("Abort Game", TriggerAbortGame);
    private static void TriggerAbortGame()
    {
        DoActionType(BindActionType.AbortGame);
    }

    [LocalSettingsButton]
    public LocalSettingsButton StartMeetingButton { get; private set; } = new("Start Meeting", TriggerStartMeeting);
    private static void TriggerStartMeeting()
    {
        DoActionType(BindActionType.StartMeeting);
    }

    [LocalSettingsButton]
    public LocalSettingsButton EndMeetingButton { get; private set; } = new("End Meeting", TriggerEndMeeting);
    private static void TriggerEndMeeting()
    {
        DoActionType(BindActionType.EndMeeting);
    }

    private static void DoActionType(BindActionType type)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }
        var isHost = PlayerControl.LocalPlayer.IsHost();
        if (!isHost)
        {
            return;
        }

        var freeplay = TutorialManager.InstanceExists;

        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined || freeplay)
        {
            // Suicide Keybind (ENTER + T + Left Shift)
            if (!PlayerControl.LocalPlayer.HasDied() && type is BindActionType.SelfKill)
            {
                PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
            }

            // End Game Keybind (ENTER + L + Left Shift)
            if (type is BindActionType.AbortGame)
            {
                var gameFlow = GameManager.Instance.LogicFlow.Cast<LogicGameFlowNormal>();
                if (gameFlow != null)
                {
                    CustomGameOver.Trigger<HostGameOver>([]);
                }
            }

            // Start Meeting (ENTER + K + Left Shift)
            if (!MeetingHud.Instance &&
                !ExileController.Instance && type is BindActionType.StartMeeting)
            {
                Bindings.RpcHostStartMeeting(PlayerControl.LocalPlayer);
            }
        }

        // End Meeting Keybind (F6)
        if (type is BindActionType.EndMeeting && MeetingHud.Instance)
        {
            Bindings.RpcHostEndMeeting(PlayerControl.LocalPlayer);
        }
    }
}

public enum BindActionType
{
    SelfKill,
    AbortGame,
    StartMeeting,
    EndMeeting,
    RandomImpRole,
    RandomNeutralKillerRole,
}
//      Suicide Keybind (ENTER + T + Left Shift)
//      End Game Keybind (ENTER + L + Left Shift)
//      Start Meeting (ENTER + K + Left Shift)
//      End Meeting Keybind (F6)
//      Random Impostor Role (F3)
//      Random Neutral Killer Role (F4)
//      CTRL to pass through objects in lobby ONLY