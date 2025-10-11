using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class CampButton : TownOfUsRoleButton<DeputyRole, PlayerControl>
{
    public bool Usable = true;
    public override string Name => TouLocale.Get("TouRoleDeputyCamp", "Camp");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Deputy;
    public override float Cooldown => 0.001f + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CampButtonSprite;

    public override bool CanUse()
    {
        return base.CanUse() && Usable;
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target?.HasModifier<DeputyCampedModifier>() == true;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Camp: Target is null");
            return;
        }

        var player = ModifierUtils.GetPlayersWithModifier<DeputyCampedModifier>(x => x.Deputy.AmOwner).FirstOrDefault();

        if (player != null)
        {
            player.RpcRemoveModifier<DeputyCampedModifier>();
        }

        Target.RpcAddModifier<DeputyCampedModifier>(PlayerControl.LocalPlayer);
        Usable = false;
        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TouLocale.GetParsed("TouRoleDeputyCampNotif").Replace("<player>", Target.Data.PlayerName)}</b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Deputy.LoadAsset());
        notif1.AdjustNotification();
    }
}