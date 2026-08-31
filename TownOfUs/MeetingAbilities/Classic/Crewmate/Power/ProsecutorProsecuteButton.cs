using MiraAPI.GameOptions;
using MiraAPI.MeetingAbilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Classic.Crewmate.Power;

public class ProsecutorProsecuteButton : TargetedMeetingButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ProsecutorProsecute");

    public override int MaxUses => 0;

    public override float InitialCooldown => 5;

    public override float Cooldown => 3;

    public override LoadableAsset<Sprite> Sprite => TouAssets.ProsecuteMeetingSprite;

    public override Color OutlineColor => TownOfUsColors.Crewmate;

    public override bool Enabled(RoleBehaviour r)
    {
        return r is ProsecutorRole pros && !pros.HideProsButton && !pros.HasProsecuted && pros.ProsecutionsCompleted <
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
    }

    protected override void OnClick(PlayerVoteArea playerVoteArea)
    {
        ProsecutorRole.RpcProsecute(PlayerControl.LocalPlayer, playerVoteArea.PlayerId);
    }
}