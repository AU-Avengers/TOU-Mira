using InnerNet;
using MiraAPI.GameOptions;
using TownOfUs.Options;

namespace TownOfUs.Modules;

public static class MultiplayerFreeplayMode
{
    public static bool Enabled =>
        !TutorialManager.InstanceExists &&
        AmongUsClient.Instance != null &&
        AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started &&
        OptionGroupSingleton<HostSpecificOptions>.Instance.MultiplayerFreeplay.Value;
}