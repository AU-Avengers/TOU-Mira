using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Modules;
using TownOfUs.Roles.Other;

namespace TownOfUs.Options;

public sealed class HostSpecificOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.HostSpecific");
    public override uint GroupPriority => 0;

    public ModdedToggleOption AntiCheatWarnings { get; set; } =
        new("TouOptionAntiCheatWarnings", true, false);

    public ModdedToggleOption KickCheatMods { get; set; } =
        new("TouOptionKickCheatMods", true, false);

    public ModdedToggleOption MultiplayerFreeplay { get; set; } =
        new("TouOptionMultiplayerFreeplay", false, false);

    public ModdedEnumOption BetaLoggingLevel { get; set; } =
        new("TouOptionBetaLoggingLevel", (int)LoggingLevel.LogForEveryonePostGame, typeof(LoggingLevel),
            [
                "TouOptionBetaLoggingLevelEnumNoLogging",
                "TouOptionBetaLoggingLevelEnumLogForHost",
                "TouOptionBetaLoggingLevelEnumLogForEveryone",
                "TouOptionBetaLoggingLevelEnumLogPostGame"
            ], false)
        {
            Visible = () => TownOfUsPlugin.IsDevBuild
        };

    public ModdedToggleOption LobbyFunMode { get; set; } =
        new("TouOptionLobbyFunMode", true, false);

    public ModdedToggleOption ShowRulesOnLobbyJoin { get; set; } = new("TouOptionShowRulesOnLobbyJoin", true, false);

    /*public ModdedToggleOption AllowAprilFools { get; set; } =
        new("TouOptionAllowAprilFools", true, false)
    {
        ChangedEvent = x =>
        {
            Debug("Toggle April Fools mode.");
            Coroutines.Start(CoSetAprilFools());
        }
    };*/

    public ModdedToggleOption EnableSpectators { get; set; } =
        new("TouOptionEnableSpectators", true, false)
        {
            ChangedEvent = x =>
            {
                var list = SpectatorRole.TrackedSpectators;
                foreach (var name in list)
                {
                    SpectatorRole.TrackedSpectators.Remove(name);
                }

                Debug("Removed all spectators.");
            },
        };

    public ModdedToggleOption RequireSubmerged { get; set; } =
        new("TouOptionRequireSubmerged", true, false)
        {
            Visible = () => ModCompatibility.SubLoaded
        };

    public ModdedToggleOption RequireCrowded { get; set; } =
        new("TouOptionRequireCrowded", true, false)
        {
            Visible = () => ModCompatibility.CrowdedLoaded
        };

    public ModdedToggleOption RequireAleLudu { get; set; } =
        new("TouOptionRequireAleLudu", true, false)
        {
            Visible = () => ModCompatibility.AleLuduLoaded
        };

    public ModdedToggleOption NoGameEnd { get; set; } =
        new("TouOptionNoGameEnd", false, false)
        {
            Visible = () => TownOfUsPlugin.IsDevBuild
        };

    /*private static IEnumerator CoSetAprilFools()
    {
        yield return new WaitForSeconds(0.05f);
        
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            player.MyPhysics.SetForcedBodyType(player.BodyType);
            player.ResetAppearance();
        }
    }*/
}

public enum LoggingLevel
{
    NoLogging,
    LogForHost,
    LogForEveryone,
    LogForEveryonePostGame
}