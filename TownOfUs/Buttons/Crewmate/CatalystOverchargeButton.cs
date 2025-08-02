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
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Catalyst;
    public override float Cooldown => OptionGroupSingleton<CatalystOptions>.Instance.OverchargeCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<CatalystOptions>.Instance.OverchargeUses;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.OverchargeSprite;

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

        Target?.RpcAddModifier<CatalystOverchargedModifier>(PlayerControl.LocalPlayer);
    }
}