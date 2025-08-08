using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class BootleggerRoleblockButton : TownOfUsRoleButton<BootleggerRole, PlayerControl>
{
    public override string Name => "Drink With";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDelay;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.RoleblockButtonSprite;
    public PlayerControl? _roleblockedTarget;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        OverrideName("Roleblocking");

        _roleblockedTarget = Target;

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>You chose to roleblock {_roleblockedTarget.CachedPlayerData.PlayerName}. They will be roleblocked in {OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDelay} second(s)!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Bootlegger.LoadAsset());
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

    public override void OnEffectEnd()
    {
        OverrideName("Drink With");

        if (_roleblockedTarget == null) return;

        BootleggerRole.RpcRoleblock(PlayerControl.LocalPlayer, _roleblockedTarget);
        _roleblockedTarget = null;
    }

}
