using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class CatalystOverchargeButton : TownOfUsRoleButton<CatalystRole, PlayerControl>
{
    public override string Name => "Overcharge";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Catalyst;
    public override float Cooldown => OptionGroupSingleton<CatalystOptions>.Instance.OverchargeCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<CatalystOptions>.Instance.OverchargeUses;
    public override float EffectDuration => Math.Clamp(OptionGroupSingleton<CatalystOptions>.Instance.OverchargeDelay, 0.001f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.OverchargeSprite;
    public PlayerControl? _overchargedTarget;

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target!.HasModifier<CatalystOverchargedModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error($"{Name}: Target is null");
            return;
        }

        _overchargedTarget = Target;
        OverrideName("Overcharging");
    }

    public override void OnEffectEnd()
    {
        OverrideName("Overcharge");

        if (_overchargedTarget == null) return;

        _overchargedTarget.RpcAddModifier<CatalystOverchargedModifier>(PlayerControl.LocalPlayer);
        _overchargedTarget = null;
    }
}