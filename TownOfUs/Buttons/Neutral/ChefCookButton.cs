using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ChefCookButton : TownOfUsRoleButton<ChefRole, DeadBody>
{
    public override string Name => TouLocale.Get("TouRoleChefCook", "Cook");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override int MaxUses => (int)OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded;
    public override Color TextOutlineColor => TownOfUsColors.Chef;
    public override float Cooldown => OptionGroupSingleton<ChefOptions>.Instance.CookCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.IgniteButtonSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is ChefRole && !PlayerControl.LocalPlayer.Data.IsDead;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Chef Cook: Target is null");
            return;
        }

        ChefRole.RpcCookBody(PlayerControl.LocalPlayer, Target);
        CustomButtonSingleton<ChefServeButton>.Instance.UpdateServingType();
        if (OptionGroupSingleton<ChefOptions>.Instance.ResetCooldowns)
        {
            CustomButtonSingleton<ChefServeButton>.Instance.ResetCooldownAndOrEffect();
        }
    }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer?.GetNearestDeadBody(PlayerControl.LocalPlayer.MaxReportDistance / 4f);
    }

    public override bool IsTargetValid(DeadBody? target)
    {
        return target && target?.Reported == false;
    }
}