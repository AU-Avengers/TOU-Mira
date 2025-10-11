using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class ClericBarrierButton : TownOfUsRoleButton<ClericRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleClericBarrier", "Barrier");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Cleric;
    public override float Cooldown => OptionGroupSingleton<ClericOptions>.Instance.BarrierCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<ClericOptions>.Instance.BarrierDuration;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.BarrierSprite;

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target!.HasModifier<ClericBarrierModifier>();
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

        Target?.RpcAddModifier<ClericBarrierModifier>(PlayerControl.LocalPlayer);

        CustomButtonSingleton<ClericCleanseButton>.Instance.ResetCooldownAndOrEffect();
    }
}