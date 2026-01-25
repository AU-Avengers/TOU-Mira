using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class ClericBarrierButton : TownOfUsRoleButton<ClericRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleClericBarrier", "Barrier");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Cleric;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ClericOptions>.Instance.BarrierCooldown + MapCooldown, 5f, 120f);
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
            Error($"{Name}: Target is null");
            return;
        }

        Target?.RpcAddModifier<ClericBarrierModifier>(PlayerControl.LocalPlayer);

        CustomButtonSingleton<ClericCleanseButton>.Instance.ResetCooldownAndOrEffect();
    }
}