using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class UndertakerDragDropButton : TownOfUsRoleButton<UndertakerRole, DeadBody>, IAftermathableBodyButton
{
    public override string Name => TouLocale.Get("TouRoleUndertakerDrag", "Drag");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<UndertakerOptions>.Instance.DragCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.DragSprite;

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
        }

        OnClick();
        Button?.SetDisabled();
    }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer?.GetNearestDeadBody(PlayerControl.LocalPlayer.MaxReportDistance / 4f);
    }

    public void AftermathHandler()
    {
        DeadBody? deadBody = null;
        if (PlayerControl.LocalPlayer.TryGetModifier<DragModifier>(out var dragMod))
        {
            deadBody = dragMod.DeadBody!;
            UndertakerRole.RpcStopDragging(PlayerControl.LocalPlayer, dragMod.DeadBody!.transform.position);
        }

        var body = Helpers
            .GetNearestDeadBodies(PlayerControl.LocalPlayer.GetTruePosition(), Distance, Helpers.CreateFilter(Constants.NotShipMask))
            .Find(component => component && !component.Reported && component != deadBody);
        if (body == null)
        {
            return;
        }

        UndertakerRole.RpcStartDragging(PlayerControl.LocalPlayer, body.ParentId);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.TryGetModifier<DragModifier>(out var dragMod))
        {
            UndertakerRole.RpcStopDragging(PlayerControl.LocalPlayer, dragMod.DeadBody!.transform.position);
            Timer = Cooldown;
        }
        else
        {
            UndertakerRole.RpcStartDragging(PlayerControl.LocalPlayer, Target.ParentId);
        }
    }

    public override bool CanUse()
    {
        return base.CanUse() && Target && !PlayerControl.LocalPlayer.inVent &&
               (!PlayerControl.LocalPlayer.HasModifier<DragModifier>() || CanDrop());
    }

    private bool CanDrop()
    {
        if (Target == null)
        {
            return false;
        }

        return !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider,
            PlayerControl.LocalPlayer.Collider.bounds.center, Target.TruePosition, Constants.ShipAndAllObjectsMask,
            false);
    }

    public void SetDrag()
    {
        OverrideSprite(TouImpAssets.DragSprite.LoadAsset());
        OverrideName(TouLocale.Get("TouRoleUndertakerDrag", "Drag"));
    }

    public void SetDrop()
    {
        OverrideSprite(TouImpAssets.DropSprite.LoadAsset());
        OverrideName(TouLocale.Get("TouRoleUndertakerDrop", "Drop"));
    }

    public override bool IsTargetValid(DeadBody? target)
    {
        return target && target?.Reported == false;
    }
}