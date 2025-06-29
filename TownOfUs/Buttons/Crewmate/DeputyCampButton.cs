﻿using MiraAPI.Modifiers;
using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using MiraAPI.Utilities;

namespace TownOfUs.Buttons.Crewmate;

public sealed class CampButton : TownOfUsRoleButton<DeputyRole, PlayerControl>
{
    public override string Name => "Camp";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Deputy;
    public override float Cooldown => 0.001f + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CampButtonSprite;
    public bool Usable = true;

    public override bool CanUse()
    {
        return base.CanUse() && Usable;
    }
    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target?.HasModifier<DeputyCampedModifier>() == true;
    }

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Camp: Target is null");
            return;
        }

        var player = ModifierUtils.GetPlayersWithModifier<DeputyCampedModifier>(x => x.Deputy == PlayerControl.LocalPlayer).FirstOrDefault();

        if (player != null)
        {
            player.RpcRemoveModifier<DeputyCampedModifier>();
        }

        Target.RpcAddModifier<DeputyCampedModifier>(PlayerControl.LocalPlayer);
        Usable = false;  
        var notif1 = Helpers.CreateAndShowNotification($"<b>Wait for {Target.Data.PlayerName}'s death so you can avenge them in the meeting.</b>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Deputy.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);
    }
}
