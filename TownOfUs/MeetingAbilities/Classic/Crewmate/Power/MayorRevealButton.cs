using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Classic.Crewmate.Power;

public class MayorRevealButton : MeetingActionButton
{
    public override string Name => LegacyAssets.IsLegacy
        ? string.Empty
        : MiraLocaleManager.Get("TownOfUsMira.Role.PoliticianReveal");

    public override float Cooldown => 0.0001f;

    public override float InitialCooldown => 0.0001f;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite =>
        LegacyAssets.IsLegacy ? LegacyAssets.RevealButtonSprite : TouAssets.RevealCleanSprite;

    public override bool HideUponWrapUp => true;
    public override bool DisableUponUse => true;
    public override Color TextOutlineColor => TownOfUsColors.Mayor;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null && (role is MayorRole { Revealed: false } || role is PoliticianRole) &&
               !PlayerControl.LocalPlayer.HasModifier<JailedModifier>();
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
               MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is MayorRole)
        {
            MayorRole.RpcAnimateNewReveal(PlayerControl.LocalPlayer);
        }
        else if (PlayerControl.LocalPlayer.Data.Role is PoliticianRole poli)
        {
            poli.AttemptReveal();
        }
    }
}
