using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace TownOfUs.Options;

public sealed class PostmortemOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TouOptionTitlePostmortem");
    public override uint GroupPriority => 4;

    public ModdedToggleOption TheDeadKnow { get; set; } =
        new("TouOptionTheDeadKnow", true);

    public ModdedToggleOption DeadSeeVotes { get; set; } =
        new("TouOptionDeadSeeVotes", true);

    public ModdedEnumOption DeadSeePrivateChat { get; set; } =
        new("TouOptionDeadSeePrivateChat", (int)GhostModeGlobal.DisabledUponDeath,
            typeof(GhostModeGlobal),
            [
                "TouOptionDeadSeePrivateChatEnumDisabled",
                "TouOptionDeadSeePrivateChatEnumDisabledUponDeath",
                "TouOptionDeadSeePrivateChatEnumInMeetings",
                "TouOptionDeadSeePrivateChatEnumAlways"
            ]);

    public ModdedEnumOption DeadCanHaunt { get; set; } =
        new("TouOptionDeadCanHaunt", (int)GhostModeInGame.DisabledUponDeath,
            typeof(GhostModeInGame),
            [
                "TouOptionDeadCanHauntEnumDisabled",
                "TouOptionDeadCanHauntEnumDisabledUponDeath",
                "TouOptionDeadCanHauntEnumAlways"
            ]);

    public ModdedToggleOption HideChatButton { get; set; } =
        new("TouOptionHideChatButton", true);

    public ModdedToggleOption ShowTaskDead { get; set; } =
        new("TouOptionShowTaskDead", true);
}
public enum GhostModeInGame
{
    Disabled,
    DisabledUponDeath,
    Always,
}

public enum GhostModeGlobal
{
    Disabled,
    DisabledUponDeath,
    InMeetings,
    Always,
}