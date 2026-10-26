using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TownOfUs.Options;

public sealed class PostmortemOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.Postmortem");
    public override LoadableAsset<Sprite> Sprite => TouRoleIcons.Spectre;
    public override uint GroupPriority => 4;

    public ModdedToggleOption TheDeadKnow { get; set; } =
        new("TouOptionTheDeadKnow", true);

    public ModdedToggleOption DeadSeeVotes { get; set; } =
        new("TouOptionDeadSeeVotes", true);

    public ModdedEnumOption<GhostModeGlobal> DeadSeePrivateChat { get; set; } =
        new("TouOptionDeadSeePrivateChat", GhostModeGlobal.DisabledUponDeath,
            [
                "TouOptionDeadSeePrivateChatEnumDisabled",
                "TouOptionDeadSeePrivateChatEnumDisabledUponDeath",
                "TouOptionDeadSeePrivateChatEnumInMeetings",
                "TouOptionDeadSeePrivateChatEnumAlways"
            ]);

    public ModdedEnumOption<GhostModeInGame> DeadCanHaunt { get; set; } =
        new("TouOptionDeadCanHaunt", GhostModeInGame.DisabledUponDeath,
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