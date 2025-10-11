using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class AmnesiacRememberButton : TownOfUsRoleButton<AmnesiacRole, DeadBody>
{
    public override string Name => TouLocale.Get("TouRoleAmnesiacRemember", "Remember");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Amnesiac;
    public override float Cooldown => 0.001f + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.RememberButtonSprite;

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        var targetId = Target.ParentId;
        var targetPlayer = MiscUtils.PlayerById(targetId);

        if (targetPlayer == null)
        {
            return; // Someone may have left mid game or something and gc just vacuumed, but idk. better safe than sorry ig.
        }

        AmnesiacRole.RpcRemember(PlayerControl.LocalPlayer, targetPlayer);
    }
}