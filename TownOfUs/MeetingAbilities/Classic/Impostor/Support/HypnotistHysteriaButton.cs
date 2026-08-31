using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Classic.Impostor.Support;

public class HypnotistHysteriaButton : MeetingActionButton
{
    public override string Name => LegacyAssets.IsLegacy
        ? string.Empty
        : MiraLocaleManager.Get("TownOfUsMira.Role.HypnotistMassHysteria");

    public override float Cooldown => 0.0001f;

    public override float InitialCooldown => 0.0001f;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite =>
        LegacyAssets.IsLegacy ? LegacyAssets.HysteriaSprite : TouAssets.HysteriaCleanSprite;

    public override bool HideUponWrapUp => true;
    public override bool DisableUponUse => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null && role is HypnotistRole { HysteriaActive: false } &&
               !PlayerControl.LocalPlayer.HasModifier<JailedModifier>() &&
               ModifierUtils.GetActiveModifiers<HypnotisedModifier>().HasAny();
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
               MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is HypnotistRole)
        {
            HypnotistRole.RpcHysteria(PlayerControl.LocalPlayer);
        }
    }
}
