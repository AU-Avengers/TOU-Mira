using MiraAPI.GameOptions;
using MiraAPI.MeetingAbilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Classic.Crewmate.Power;

public class ProsecutorToggleButton : MeetingActionButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ProsecutorProsecuteToggle");

    public override float Cooldown => 0.0001f;

    public override float InitialCooldown => 0.0001f;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite =>
        TouAssets.RevealCleanSprite;

    public override bool HideUponWrapUp => true;
    public override bool DisableUponVoting => true;
    public override Color TextOutlineColor => TownOfUsColors.Prosecutor;


    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ProsecutorRole pros && !pros.HideProsButton && !pros.HasProsecuted && pros.ProsecutionsCompleted <
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
               MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.NotVoted;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is ProsecutorRole pros)
        {
            pros.WantsToPros = !pros.WantsToPros;
        }
    }
}
