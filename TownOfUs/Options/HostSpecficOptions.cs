using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Options;

public sealed class HostSpecificOptions : AbstractOptionGroup
{
    public override string GroupName => "Host-Specific Options";
    public override uint GroupPriority => 0;

    public ModdedToggleOption AntiCheatWarnings { get; set; } = new("Enable Anti Cheat Warnings", true, false);

    public ModdedEnumOption BetaLoggingLevel { get; set; } = new("Advanced Logging Mode", (int)LoggingLevel.LogForEveryone, typeof(LoggingLevel),
        ["No Logging", "Log For Host", "Log For Everyone", "Log Post-Game"], false)
    {
        Visible = () => TownOfUsPlugin.IsDevBuild
    };

    public ModdedToggleOption AllowAprilFools { get; set; } = new("Allow April Fools Visuals", true, false)
    {
        ChangedEvent = x =>
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.MyPhysics.SetForcedBodyType(player.BodyType);
                player.ResetAppearance();
            }

            Debug("Toggle April Fools mode.");
        }
    };
    public ModdedToggleOption EnableSpectators { get; set; } = new("Allow More Spectators", true, false)
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
}

public enum LoggingLevel
{
    NoLogging,
    LogForHost,
    LogForEveryone,
    LogForEveryonePostGame
}