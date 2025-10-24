using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace TownOfUs.Options;

public sealed class PostmortemOptions : AbstractOptionGroup
{
    public override string GroupName => "Postmortem Options";
    public override uint GroupPriority => 2;

    public ModdedToggleOption TheDeadKnow { get; set; } = new("The Dead Know Players", true);

    public ModdedToggleOption DeadSeeVotes { get; set; } = new("The Dead See Votes", true);

    public ModdedEnumOption DeadSeePrivateChat { get; set; } = new("The Dead See Private Chat", (int)GhostMode.DisabledUponDeath, typeof(GhostMode),
        ["Disabled", "Disabled Upon Death", "In Meetings", "Always"]);

    public ModdedEnumOption DeadCanHaunt { get; set; } = new("Haunt (Follow) Mode", (int)GhostModeInGame.DisabledUponDeath, typeof(GhostModeInGame),
        ["Disabled", "Disabled Upon Death", "Always"]);

    public ModdedToggleOption HideChatButton { get; set; } = new("Temporarily Hide Chat Upon Death", true);

}

public enum GhostModeInGame
{
    Disabled,
    DisabledUponDeath,
    Always,
}

public enum GhostMode
{
    Disabled,
    DisabledUponDeath,
    InMeetings,
    Always,
}