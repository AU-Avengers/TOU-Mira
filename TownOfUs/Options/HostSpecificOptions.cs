using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using Reactor.Utilities;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class HostSpecificOptions : AbstractOptionGroup
{
    public override string GroupName => "Host-Specific Options";
    public override uint GroupPriority => 0;

    public ModdedToggleOption AntiCheatWarnings { get; set; } = new("Enable Anti Cheat Warnings", true, false);

    public ModdedToggleOption KickCheatMods { get; set; } = new("Kick Players Using Cheat Mods", true, false);

    public ModdedEnumOption BetaLoggingLevel { get; set; } = new("Advanced Logging Mode", (int)LoggingLevel.LogForEveryonePostGame, typeof(LoggingLevel),
        ["No Logging", "Log For Host", "Log For Everyone", "Log Post-Game"], false)
    {
        Visible = () => TownOfUsPlugin.IsDevBuild
    };

    public ModdedToggleOption LobbyFunMode { get; set; } = new("Allow Lobby-Only No-Clip", true, false);

    public ModdedToggleOption AllowAprilFools { get; set; } = new("Allow April Fools Visuals", true, false)
    {
        ChangedEvent = x =>
        {
            Debug("Toggle April Fools mode.");
            Coroutines.Start(CoSetAprilFools());
        }
    };
    public ModdedToggleOption EnableSpectators { get; set; } = new("Allow Spectators", true, false)
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

    public ModdedToggleOption NoGameEnd { get; set; } = new("No Game End", false, false)
    {
        Visible = () => TownOfUsPlugin.IsDevBuild
    };

    private static IEnumerator CoSetAprilFools()
    {
        yield return new WaitForSeconds(0.05f);
        
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            player.MyPhysics.SetForcedBodyType(player.BodyType);
            player.ResetAppearance();
        }
    }
}

public enum LoggingLevel
{
    NoLogging,
    LogForHost,
    LogForEveryone,
    LogForEveryonePostGame
}