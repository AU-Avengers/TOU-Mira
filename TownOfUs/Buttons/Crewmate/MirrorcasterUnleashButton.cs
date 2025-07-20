﻿using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MirrorcasterUnleashButton : TownOfUsRoleButton<MirrorcasterRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    public override string Name => "Unleash";
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Mirrorcaster;
    public override float Cooldown => OptionGroupSingleton<MirrorcasterOptions>.Instance.UnleashCooldown.Value + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.UnleashSprite;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Mirrorcaster Unleash: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        MirrorcasterRole.RpcMirrorcasterUnleash(PlayerControl.LocalPlayer);
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (Role.UnleashesAvailable <= 0)
        {
            return false;
        }

        return base.IsTargetValid(target);
    }
}